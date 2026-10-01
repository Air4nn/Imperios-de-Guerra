using System;
using System.Threading.Tasks;
using ImperiosEnGuerra.Datos;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Servicios;
 
namespace ImperiosEnGuerra.Controlador
{
    public class JuegoController
    {
        public Partida Partida { get; private set; }
 
        public JuegoService Juego { get; private set; }
 
        public ArchivoService Archivos { get; private set; }
 
        public ConcurrenciaService Concurrencia { get; private set; }
 
        private bool resultadoGuardado;
 
        public JuegoController()
        {
            Partida = new Partida();
 
            Juego = new JuegoService(Partida);
 
            Archivos = new ArchivoService();
 
            Concurrencia = new ConcurrenciaService(Juego);
 
            Archivos.RegistrarEvento("Partida iniciada");
        }
 
        public bool Construir(Jugador jugador, int x, int y)
        {
            bool ok = Juego.ConstruirCasa(jugador.Id, x, y);
 
            if (ok)
                Archivos.RegistrarEvento(
                    jugador.Nombre + " construyo una casa en (" + x + "," + y + ")");
 
            return ok;
        }
 
        // El servicio necesita una celda; se busca una libre junto al Centro Urbano.
        public bool Entrenar(Jugador jugador)
        {
            if (!BuscarCeldaLibreCercaDelCentro(jugador, out int x, out int y))
                return false;
 
            bool ok = Juego.EntrenarSoldado(jugador.Id, x, y);
 
            if (ok)
                Archivos.RegistrarEvento(
                    jugador.Nombre + " entreno un soldado en (" + x + "," + y + ")");
 
            return ok;
        }
 
        public bool Mover(Unidad unidad, int x, int y)
        {
            if (Partida.Finalizada || !unidad.EstaViva())
                return false;
 
            return Juego.MoverUnidad(unidad.JugadorId, unidad.Id, x, y);
        }
 
        // Ataque cuerpo a cuerpo: el atacante debe estar en una celda contigua.
        public bool AtacarUnidad(Unidad atacante, Unidad objetivo)
        {
            if (Partida.Finalizada) return false;
            if (!atacante.EstaViva() || !objetivo.EstaViva()) return false;
            if (!EnRango(atacante, objetivo.X, objetivo.Y)) return false;
 
            bool ok = Juego.AtacarUnidad(
                atacante.JugadorId, atacante.Id, objetivo.Id);
 
            if (ok)
            {
                Archivos.RegistrarEvento(
                    "Unidad " + atacante.Id + " ataco a la unidad " + objetivo.Id +
                    " (vida restante: " + objetivo.Vida + ")");
 
                RegistrarResultadoSiTermino();
            }
 
            return ok;
        }
 
        public bool AtacarEdificio(Unidad atacante, Edificio objetivo)
        {
            if (Partida.Finalizada) return false;
            if (!atacante.EstaViva() || objetivo.EstaDestruido()) return false;
            if (!EnRango(atacante, objetivo.X, objetivo.Y)) return false;
 
            bool ok = Juego.AtacarEdificio(
                atacante.JugadorId, atacante.Id, objetivo.Id);
 
            if (ok)
            {
                Archivos.RegistrarEvento(
                    "Unidad " + atacante.Id + " ataco al edificio " + objetivo.Id +
                    " (vida restante: " + objetivo.Vida + ")");
 
                RegistrarResultadoSiTermino();
            }
 
            return ok;
        }
 
        // El servicio recolecta en la celda donde esta la unidad (devuelve void).
        public void Recolectar(Jugador jugador, Unidad unidad)
        {
            Juego.RecolectarRecurso(jugador.Id, unidad.Id);
        }
 
        // Recoleccion en segundo plano (hilo) usando ConcurrenciaService.
        public Task RecolectarAutomatico(Unidad unidad, int segundos)
        {
            return Concurrencia.RecoleccionAutomatica(
                unidad.JugadorId, unidad.Id, segundos);
        }
 
        public void VerificarEstado()
        {
            Partida.VerificarVictoria();
            RegistrarResultadoSiTermino();
        }
 
        // Guarda el resultado final en archivo una sola vez cuando la partida termina.
        private void RegistrarResultadoSiTermino()
        {
            if (!Partida.Finalizada || resultadoGuardado)
                return;
 
            resultadoGuardado = true;
 
            Jugador ganador = Partida.ObtenerGanador();
 
            string texto = ganador != null
                ? "Ganador: " + ganador.Nombre
                : "Empate";
 
            Archivos.GuardarResultadoFinal(texto);
            Archivos.RegistrarEvento("Partida finalizada. " + texto);
        }
 
        private bool EnRango(Unidad atacante, int x, int y)
        {
            return Math.Max(
                Math.Abs(atacante.X - x),
                Math.Abs(atacante.Y - y)) <= 1;
        }
 
        private bool BuscarCeldaLibreCercaDelCentro(Jugador jugador, out int x, out int y)
        {
            x = 0;
            y = 0;
 
            Edificio centro = jugador.Edificios.Find(
                e => e.Tipo == TipoEdificio.CentroUrbano);
 
            if (centro == null)
                return false;
 
            for (int radio = 1; radio <= 3; radio++)
            {
                for (int dx = -radio; dx <= radio; dx++)
                {
                    for (int dy = -radio; dy <= radio; dy++)
                    {
                        int cx = centro.X + dx;
                        int cy = centro.Y + dy;
 
                        if (Partida.Mapa.EstaLibre(cx, cy))
                        {
                            x = cx;
                            y = cy;
                            return true;
                        }
                    }
                }
            }
 
            return false;
        }
    }
}