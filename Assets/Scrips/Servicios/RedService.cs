using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
 
namespace ImperiosEnGuerra.Servicios
{
    public enum EstadoRed
    {
        Inactivo,
        Esperando,
        Conectando,
        Conectado,
        Desconectado,
        Error
    }
 
    // Comunicación entre los dos jugadores con sockets TCP.
    // Un jugador es el anfitrión (escucha) y el otro el cliente (se conecta).
    // Cada mensaje es una línea de texto con un objeto JSON (ver MensajeRed).
    //
    // Hilos creados (todos de fondo, para no bloquear el hilo principal de Unity):
    //   Red-Aceptar    (anfitrión) espera la conexión del oponente
    //   Red-Conectar   (cliente)   intenta conectarse al anfitrión
    //   Red-Recepcion  lee líneas del socket y las deja en una cola thread-safe
    //   Red-Envio      toma mensajes de una cola bloqueante y los escribe en el socket
    // Los mensajes se serializan y leen en el hilo principal (JsonUtility); los hilos de
    // red solo mueven texto. Detener() cierra sockets y hace terminar todos los hilos.
    public class RedService
    {
        public const int PuertoPorDefecto = 5000;
 
        private TcpListener servidor;
        private TcpClient cliente;
        private readonly object candado = new object();
 
        private readonly CancellationTokenSource cancelacion = new CancellationTokenSource();
        private readonly BlockingCollection<string> salida = new BlockingCollection<string>();
        private readonly ConcurrentQueue<string> entrada = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> eventos = new ConcurrentQueue<string>();
 
        private int estado = (int)EstadoRed.Inactivo;
        private int cerrado;
 
        public EstadoRed Estado
        {
            get { return (EstadoRed)Volatile.Read(ref estado); }
        }
 
        private void CambiarEstado(EstadoRed nuevo)
        {
            Volatile.Write(ref estado, (int)nuevo);
        }
 
        // ---------- Conexión ----------
 
        public void IniciarAnfitrion(int puerto)
        {
            CambiarEstado(EstadoRed.Esperando);
 
            Thread hilo = new Thread(() => EsperarOponente(puerto));
            hilo.IsBackground = true;
            hilo.Name = "Red-Aceptar";
            hilo.Start();
        }
 
        public void IniciarCliente(string ip, int puerto)
        {
            CambiarEstado(EstadoRed.Conectando);
 
            Thread hilo = new Thread(() => ConectarAlAnfitrion(ip, puerto));
            hilo.IsBackground = true;
            hilo.Name = "Red-Conectar";
            hilo.Start();
        }
 
        private void EsperarOponente(int puerto)
        {
            try
            {
                lock (candado)
                {
                    servidor = new TcpListener(IPAddress.Any, puerto);
                    servidor.Start();
                }
 
                TcpClient entrante = servidor.AcceptTcpClient();
 
                lock (candado)
                {
                    servidor.Stop();
                }
 
                Conectar(entrante);
            }
            catch (Exception ex)
            {
                if (Volatile.Read(ref cerrado) == 0)
                    Fallar("No se pudo abrir la partida: " + Descripcion(ex));
            }
        }
 
        private void ConectarAlAnfitrion(string ip, int puerto)
        {
            try
            {
                TcpClient nuevo = new TcpClient();
                Task conexion = nuevo.ConnectAsync(ip, puerto);
 
                if (!conexion.Wait(5000))
                {
                    nuevo.Close();
                    throw new TimeoutException("el anfitrión no respondió en 5 segundos");
                }
 
                Conectar(nuevo);
            }
            catch (Exception ex)
            {
                if (Volatile.Read(ref cerrado) == 0)
                    Fallar("No se pudo conectar: " + Descripcion(ex));
            }
        }
 
        private void Conectar(TcpClient c)
        {
            c.NoDelay = true;
 
            lock (candado)
            {
                cliente = c;
            }
 
            CambiarEstado(EstadoRed.Conectado);
            eventos.Enqueue("Conectado");
 
            Thread recepcion = new Thread(() => BucleRecepcion(c));
            recepcion.IsBackground = true;
            recepcion.Name = "Red-Recepcion";
            recepcion.Start();
 
            Thread envio = new Thread(() => BucleEnvio(c));
            envio.IsBackground = true;
            envio.Name = "Red-Envio";
            envio.Start();
        }
 
        // ---------- Hilos de comunicación ----------
 
        private void BucleRecepcion(TcpClient c)
        {
            try
            {
                StreamReader lector = new StreamReader(c.GetStream(), Encoding.UTF8);
                string linea;
 
                while ((linea = lector.ReadLine()) != null)
                {
                    if (linea.Length > 0)
                        entrada.Enqueue(linea);
                }
            }
            catch (Exception)
            {
                // El socket se cerró o se perdió la red: se trata abajo como desconexión.
            }
 
            Desconexion();
        }
 
        private void BucleEnvio(TcpClient c)
        {
            try
            {
                StreamWriter escritor =
                    new StreamWriter(c.GetStream(), new UTF8Encoding(false));
                escritor.AutoFlush = true;
 
                foreach (string linea in salida.GetConsumingEnumerable(cancelacion.Token))
                {
                    escritor.WriteLine(linea);
                }
            }
            catch (Exception)
            {
                // Idem: socket cerrado, red caída o cancelación.
            }
 
            Desconexion();
        }
 
        // Pasa de Conectado a Desconectado una sola vez, aunque fallen los dos hilos.
        private void Desconexion()
        {
            if (Volatile.Read(ref cerrado) == 1)
                return;
 
            int anterior = Interlocked.CompareExchange(
                ref estado, (int)EstadoRed.Desconectado, (int)EstadoRed.Conectado);
 
            if (anterior == (int)EstadoRed.Conectado)
            {
                eventos.Enqueue("Desconectado");
 
                try { salida.CompleteAdding(); } catch (Exception) { }
 
                CerrarSockets();
            }
        }
 
        private void Fallar(string texto)
        {
            CambiarEstado(EstadoRed.Error);
            eventos.Enqueue("Error: " + texto);
            CerrarSockets();
        }
 
        // ---------- Uso desde el Controlador (hilo principal) ----------
 
        public void Enviar(MensajeRed mensaje)
        {
            if (Estado != EstadoRed.Conectado)
                return;
 
            try
            {
                salida.Add(JsonUtility.ToJson(mensaje));
            }
            catch (InvalidOperationException)
            {
                // La cola ya se cerró porque la conexión terminó.
            }
        }
 
        // Devuelve el siguiente mensaje válido recibido; descarta los que no se pueden leer.
        public bool TryLeerMensaje(out MensajeRed mensaje)
        {
            mensaje = null;
            string linea;
 
            while (entrada.TryDequeue(out linea))
            {
                try
                {
                    mensaje = JsonUtility.FromJson<MensajeRed>(linea);
                }
                catch (Exception)
                {
                    mensaje = null;
                }
 
                if (mensaje != null && !string.IsNullOrEmpty(mensaje.Tipo))
                    return true;
 
                eventos.Enqueue("Mensaje de red inválido descartado");
            }
 
            return false;
        }
 
        public bool TryLeerEvento(out string evento)
        {
            return eventos.TryDequeue(out evento);
        }
 
        // ---------- Cierre ----------
 
        public void Detener()
        {
            if (Interlocked.Exchange(ref cerrado, 1) == 1)
                return;
 
            cancelacion.Cancel();
 
            try { salida.CompleteAdding(); } catch (Exception) { }
 
            CerrarSockets();
        }
 
        private void CerrarSockets()
        {
            lock (candado)
            {
                try { if (servidor != null) servidor.Stop(); } catch (Exception) { }
                try { if (cliente != null) cliente.Close(); } catch (Exception) { }
            }
        }
 
        // ---------- Auxiliares ----------
 
        private static string Descripcion(Exception ex)
        {
            AggregateException agregada = ex as AggregateException;
 
            if (agregada != null && agregada.InnerException != null)
                return agregada.InnerException.Message;
 
            return ex.Message;
        }
 
        public static string ObtenerIpLocal()
        {
            try
            {
                foreach (IPAddress direccion in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (direccion.AddressFamily == AddressFamily.InterNetwork)
                        return direccion.ToString();
                }
            }
            catch (Exception)
            {
            }
 
            return "127.0.0.1";
        }
    }
}
