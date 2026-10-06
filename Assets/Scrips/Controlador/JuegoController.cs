using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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
 
        // Mensajes para el jugador. Los hilos de fondo los encolan y la vista los lee.
        public ConcurrentQueue<string> Mensajes { get; private set; }
 
        // ---------- Estado de red ----------
 
        public RedService Red { get; private set; }
 
        // true si la partida se juega entre dos instancias conectadas.
        public bool EnRed { get; private set; }
 
        // Jugador que controla esta instancia en red (anfitrión = 1, cliente = 2).
        public int JugadorLocalId { get; private set; }
 
        // false mientras se elige el modo de juego o se espera al oponente.
        public bool PuedeJugar { get; private set; }
 
        public bool ConexionPerdida { get; private set; }
 
        public string UltimoErrorRed { get; private set; }
 
        public string EstadoRedTexto
        {
            get
            {
                if (!EnRed || Red == null) return "Local";
                return Red.Estado + " (eres el Jugador " + JugadorLocalId + ")";
            }
        }
 
        // Trabajo que los hilos de fondo piden ejecutar en el hilo principal de Unity.
        private readonly ConcurrentQueue<Action> pendientes = new ConcurrentQueue<Action>();
 
        private bool resultadoGuardado;
 
        public JuegoController()
        {
            Mensajes = new ConcurrentQueue<string>();
 
            JugadorLocalId = 1;
 
            Partida = new Partida();
 
            Juego = new JuegoService(Partida);
 
            Archivos = new ArchivoService();
 
            Concurrencia = new ConcurrenciaService(Juego, EncolarHiloPrincipal, Registrar);
        }
 
        public ICollection<TareaProgreso> TareasEnCurso
        {
            get { return Concurrencia.TareasEnCurso; }
        }
 
        // ---------- Inicio y cierre ----------
 
        public void IniciarPartida()
        {
            Archivos.ReiniciarLog();
 
            Juego.CrearUnidadInicial(1, 2, 2);
            Juego.CrearUnidadInicial(2, 12, 12);
 
            Archivos.GuardarConfiguracion(GenerarTextoConfiguracion());
 
            Registrar(0, "Inicio de partida", "Mapa de 15x15 creado, configuración guardada");
        }
 
        public void Detener()
        {
            Concurrencia.Detener();
 
            if (Red != null)
                Red.Detener();
        }
 
        // ---------- Modos de juego ----------
 
        public void IniciarModoLocal()
        {
            EnRed = false;
            PuedeJugar = true;
 
            Registrar(0, "Modo de juego", "Partida local de dos jugadores en un mismo equipo");
        }
 
        public void IniciarComoAnfitrion(int puerto)
        {
            PrepararRed(1);
            Red.IniciarAnfitrion(puerto);
        }
 
        public void IniciarComoCliente(string ip, int puerto)
        {
            PrepararRed(2);
            Red.IniciarCliente(ip, puerto);
        }
 
        public void CancelarRed()
        {
            if (Red != null)
            {
                Red.Detener();
                Red = null;
            }
 
            EnRed = false;
            PuedeJugar = false;
            JugadorLocalId = 1;
            UltimoErrorRed = null;
        }
 
        private void PrepararRed(int jugadorLocal)
        {
            if (Red != null)
                Red.Detener();
 
            Red = new RedService();
            EnRed = true;
            JugadorLocalId = jugadorLocal;
            PuedeJugar = false;
            UltimoErrorRed = null;
        }
 
        // ---------- Cola hacia el hilo principal y red ----------
 
        private void EncolarHiloPrincipal(Action accion)
        {
            pendientes.Enqueue(accion);
        }
 
        // La vista la llama en cada frame (GameManager.Update, hilo principal de Unity).
        public void ProcesarPendientes()
        {
            Action accion;
 
            while (pendientes.TryDequeue(out accion))
            {
                try
                {
                    accion();
                }
                catch (Exception ex)
                {
                    Archivos.RegistrarEvento("Error en tarea pendiente: " + ex.Message);
                }
            }
 
            ProcesarRed();
        }
 
        private void ProcesarRed()
        {
            if (Red == null)
                return;
 
            string evento;
 
            while (Red.TryLeerEvento(out evento))
            {
                if (evento == "Conectado")
                {
                    PuedeJugar = true;
                    Registrar(0, "Red", "Conexión establecida, controlas al Jugador " + JugadorLocalId);
                }
                else if (evento == "Desconectado")
                {
                    ManejarDesconexion();
                }
                else
                {
                    if (evento.StartsWith("Error"))
                        UltimoErrorRed = evento;
 
                    Registrar(0, "Red", evento);
                }
            }
 
            MensajeRed mensaje;
 
            while (Red != null && Red.TryLeerMensaje(out mensaje))
            {
                AplicarMensajeRemoto(mensaje);
            }
        }
 
        private void ManejarDesconexion()
        {
            if (Partida.Finalizada || ConexionPerdida)
                return;
 
            ConexionPerdida = true;
            PuedeJugar = false;
 
            Registrar(0, "Red", "Se perdió la conexión con el oponente; la partida se interrumpe");
 
            Concurrencia.Detener();
 
            if (!resultadoGuardado)
            {
                resultadoGuardado = true;
                Archivos.GuardarResultadoFinal(GenerarTextoResultado(null));
            }
        }
 
        private void EnviarRed(MensajeRed mensaje)
        {
            if (EnRed && Red != null)
                Red.Enviar(mensaje);
        }
 
        // Aplica una acción del oponente sobre el modelo local (sin volver a enviarla).
        private void AplicarMensajeRemoto(MensajeRed mensaje)
        {
            int remoto = JugadorLocalId == 1 ? 2 : 1;
 
            if (mensaje.JugadorId != remoto)
            {
                Registrar(0, "Red", "Mensaje descartado: jugador inesperado");
                return;
            }
 
            Jugador jugador = Partida.ObtenerJugador(remoto);
 
            switch (mensaje.Tipo)
            {
                case MensajeRed.Construir:
                    EjecutarConstruir(jugador, mensaje.X, mensaje.Y);
                    break;
 
                case MensajeRed.Entrenar:
                    EjecutarEntrenar(jugador, mensaje.X, mensaje.Y);
                    break;
 
                case MensajeRed.Mover:
                {
                    Unidad unidad = Partida.BuscarUnidad(mensaje.UnidadId);
 
                    if (unidad != null && unidad.JugadorId == remoto)
                        EjecutarMover(unidad, mensaje.X, mensaje.Y);
 
                    break;
                }
 
                case MensajeRed.Atacar:
                {
                    Unidad atacante = Partida.BuscarUnidad(mensaje.UnidadId);
 
                    if (atacante == null || atacante.JugadorId != remoto)
                        break;
 
                    // El oponente ya validó el rango en su equipo: aquí se confía en su acción.
                    if (mensaje.ObjetivoEsEdificio)
                    {
                        Edificio edificio = Partida.BuscarEdificio(mensaje.ObjetivoId);
 
                        if (edificio != null)
                            EjecutarAtaqueEdificio(atacante, edificio, false);
                    }
                    else
                    {
                        Unidad objetivo = Partida.BuscarUnidad(mensaje.ObjetivoId);
 
                        if (objetivo != null)
                            EjecutarAtaqueUnidad(atacante, objetivo, false);
                    }
 
                    break;
                }
 
                case MensajeRed.Recolectar:
                {
                    Unidad unidad = Partida.BuscarUnidad(mensaje.UnidadId);
 
                    if (unidad != null && unidad.JugadorId == remoto)
                        EjecutarRecolectar(unidad, Math.Max(1, Math.Min(mensaje.Segundos, 60)));
 
                    break;
                }
 
                default:
                    Registrar(0, "Red", "Mensaje desconocido ignorado: " + mensaje.Tipo);
                    break;
            }
        }
 
        // ---------- Mensajes y registro ----------
 
        // jugadorId 0 = mensaje del sistema.
        private void Registrar(int jugadorId, string accion, string resultado)
        {
            string quien = jugadorId > 0 ? "Jugador " + jugadorId : "Sistema";
 
            Archivos.RegistrarAccion(quien, accion, resultado);
            Mensajes.Enqueue(quien + ": " + resultado);
        }
 
        private bool Rechazar(int jugadorId, string accion, string motivo)
        {
            Registrar(jugadorId, accion, "Rechazado - " + motivo);
            return false;
        }
 
        // Mensaje solo para pantalla (no se guarda en el log).
        public void Aviso(string texto)
        {
            Mensajes.Enqueue(texto);
        }
 
        // ---------- Acciones del jugador local (se validan, se ejecutan y se envían) ----------
 
        // En red, cada instancia solo puede actuar con su propio jugador.
        private bool PuedeActuar(int jugadorId)
        {
            if (!PuedeJugar || Partida.Finalizada) return false;
            if (EnRed && jugadorId != JugadorLocalId) return false;
            return true;
        }
 
        public bool Construir(Jugador jugador, int x, int y)
        {
            if (!PuedeActuar(jugador.Id)) return false;
 
            bool ok = EjecutarConstruir(jugador, x, y);
 
            if (ok)
                EnviarRed(new MensajeRed
                {
                    Tipo = MensajeRed.Construir,
                    JugadorId = jugador.Id,
                    X = x,
                    Y = y
                });
 
            return ok;
        }
 
        public bool Entrenar(Jugador jugador)
        {
            if (!PuedeActuar(jugador.Id)) return false;
 
            int x, y;
 
            if (!BuscarCeldaLibreCercaDelCentro(jugador, out x, out y))
                return Rechazar(jugador.Id, "Entrenar soldado",
                    "no hay espacio libre junto al Centro Urbano");
 
            bool ok = EjecutarEntrenar(jugador, x, y);
 
            // Se envía la celda elegida para que el oponente cree el soldado en el mismo sitio.
            if (ok)
                EnviarRed(new MensajeRed
                {
                    Tipo = MensajeRed.Entrenar,
                    JugadorId = jugador.Id,
                    X = x,
                    Y = y
                });
 
            return ok;
        }
 
        public bool Mover(Unidad unidad, int x, int y)
        {
            if (!PuedeActuar(unidad.JugadorId)) return false;
 
            bool ok = EjecutarMover(unidad, x, y);
 
            if (ok)
                EnviarRed(new MensajeRed
                {
                    Tipo = MensajeRed.Mover,
                    JugadorId = unidad.JugadorId,
                    UnidadId = unidad.Id,
                    X = x,
                    Y = y
                });
 
            return ok;
        }
 
        // Ataque cuerpo a cuerpo: el atacante debe estar en una celda contigua.
        public bool AtacarUnidad(Unidad atacante, Unidad objetivo)
        {
            if (!PuedeActuar(atacante.JugadorId)) return false;
 
            bool ok = EjecutarAtaqueUnidad(atacante, objetivo, true);
 
            if (ok)
                EnviarRed(new MensajeRed
                {
                    Tipo = MensajeRed.Atacar,
                    JugadorId = atacante.JugadorId,
                    UnidadId = atacante.Id,
                    ObjetivoId = objetivo.Id,
                    ObjetivoEsEdificio = false
                });
 
            return ok;
        }
 
        public bool AtacarEdificio(Unidad atacante, Edificio objetivo)
        {
            if (!PuedeActuar(atacante.JugadorId)) return false;
 
            bool ok = EjecutarAtaqueEdificio(atacante, objetivo, true);
 
            if (ok)
                EnviarRed(new MensajeRed
                {
                    Tipo = MensajeRed.Atacar,
                    JugadorId = atacante.JugadorId,
                    UnidadId = atacante.Id,
                    ObjetivoId = objetivo.Id,
                    ObjetivoEsEdificio = true
                });
 
            return ok;
        }
 
        // Recolección en segundo plano: la unidad debe estar sobre una celda con recurso.
        public bool RecolectarAutomatico(Unidad unidad, int segundos)
        {
            if (!PuedeActuar(unidad.JugadorId)) return false;
 
            bool ok = EjecutarRecolectar(unidad, segundos);
 
            if (ok)
                EnviarRed(new MensajeRed
                {
                    Tipo = MensajeRed.Recolectar,
                    JugadorId = unidad.JugadorId,
                    UnidadId = unidad.Id,
                    Segundos = segundos
                });
 
            return ok;
        }
 
        public void VerificarEstado()
        {
            Partida.VerificarVictoria();
            RegistrarResultadoSiTermino();
        }
 
        // ---------- Ejecución de las acciones (las usan el jugador local y el oponente) ----------
 
        private bool EjecutarConstruir(Jugador jugador, int x, int y)
        {
            if (Partida.Finalizada) return false;
 
            if (!Partida.Mapa.EstaDentro(x, y))
                return Rechazar(jugador.Id, "Construir casa", "coordenadas fuera del mapa");
 
            if (!Partida.Mapa.EstaLibre(x, y))
                return Rechazar(jugador.Id, "Construir casa", "la celda (" + x + "," + y + ") está ocupada");
 
            if (!jugador.TieneRecursos(TipoRecurso.Madera, Partida.CostoCasaMadera))
                return Rechazar(jugador.Id, "Construir casa",
                    "madera insuficiente (se necesitan " + Partida.CostoCasaMadera + ")");
 
            if (!Juego.ReservarConstruccionCasa(jugador.Id, x, y))
                return Rechazar(jugador.Id, "Construir casa", "no se pudo iniciar la construcción");
 
            Concurrencia.ProgramarConstruccion(jugador.Id, x, y);
 
            Registrar(jugador.Id, "Construir casa",
                "Construcción iniciada en (" + x + "," + y + "), lista en " +
                ConcurrenciaService.SegundosConstruccion + " s");
 
            return true;
        }
 
        private bool EjecutarEntrenar(Jugador jugador, int x, int y)
        {
            if (Partida.Finalizada) return false;
 
            if (!jugador.TieneRecursos(TipoRecurso.Comida, Partida.CostoSoldadoComida))
                return Rechazar(jugador.Id, "Entrenar soldado",
                    "comida insuficiente (se necesitan " + Partida.CostoSoldadoComida + ")");
 
            if (!Partida.Mapa.EstaDentro(x, y) || !Partida.Mapa.EstaLibre(x, y))
                return Rechazar(jugador.Id, "Entrenar soldado", "la celda (" + x + "," + y + ") no está libre");
 
            if (!Juego.ReservarEntrenamiento(jugador.Id, x, y))
                return Rechazar(jugador.Id, "Entrenar soldado", "no se pudo iniciar el entrenamiento");
 
            Concurrencia.ProgramarEntrenamiento(jugador.Id, x, y);
 
            Registrar(jugador.Id, "Entrenar soldado",
                "Entrenamiento iniciado, listo en " +
                ConcurrenciaService.SegundosEntrenamiento + " s");
 
            return true;
        }
 
        private bool EjecutarMover(Unidad unidad, int x, int y)
        {
            if (Partida.Finalizada) return false;
 
            if (!unidad.EstaViva())
                return Rechazar(unidad.JugadorId, "Mover", "la unidad no está disponible");
 
            if (!Partida.Mapa.EstaDentro(x, y))
                return Rechazar(unidad.JugadorId, "Mover", "coordenadas fuera del mapa");
 
            if (!Partida.Mapa.EstaLibre(x, y))
                return Rechazar(unidad.JugadorId, "Mover", "la celda (" + x + "," + y + ") está ocupada");
 
            Concurrencia.IniciarMovimiento(unidad.JugadorId, unidad.Id, x, y);
 
            Registrar(unidad.JugadorId, "Mover",
                "Unidad #" + unidad.Id + " en camino a (" + x + "," + y + ")");
 
            return true;
        }
 
        private bool EjecutarAtaqueUnidad(Unidad atacante, Unidad objetivo, bool verificarRango)
        {
            if (Partida.Finalizada) return false;
 
            if (!atacante.EstaViva() || !objetivo.EstaViva())
                return Rechazar(atacante.JugadorId, "Ataque", "la unidad no está disponible");
 
            if (verificarRango && !EnRango(atacante, objetivo.X, objetivo.Y))
                return Rechazar(atacante.JugadorId, "Ataque",
                    "fuera de rango, acércate a una celda contigua");
 
            if (!Juego.AtacarUnidad(atacante.JugadorId, atacante.Id, objetivo.Id))
                return Rechazar(atacante.JugadorId, "Ataque", "objetivo no válido");
 
            string resultado = objetivo.EstaViva()
                ? "Impacto - Unidad enemiga #" + objetivo.Id + " recibió " + atacante.Ataque +
                  " de daño (vida: " + objetivo.Vida + ")"
                : "Impacto - Unidad enemiga destruida";
 
            Registrar(atacante.JugadorId, "Ataque", resultado);
            RegistrarResultadoSiTermino();
            return true;
        }
 
        private bool EjecutarAtaqueEdificio(Unidad atacante, Edificio objetivo, bool verificarRango)
        {
            if (Partida.Finalizada) return false;
 
            if (!atacante.EstaViva() || objetivo.EstaDestruido())
                return Rechazar(atacante.JugadorId, "Ataque", "objetivo no disponible");
 
            if (verificarRango && !EnRango(atacante, objetivo.X, objetivo.Y))
                return Rechazar(atacante.JugadorId, "Ataque",
                    "fuera de rango, acércate a una celda contigua");
 
            if (!Juego.AtacarEdificio(atacante.JugadorId, atacante.Id, objetivo.Id))
                return Rechazar(atacante.JugadorId, "Ataque", "objetivo no válido");
 
            string resultado = objetivo.EstaDestruido()
                ? "Impacto - Edificio enemigo destruido"
                : "Impacto - Edificio enemigo #" + objetivo.Id + " recibió " + atacante.Ataque +
                  " de daño (vida: " + objetivo.Vida + ")";
 
            Registrar(atacante.JugadorId, "Ataque", resultado);
            RegistrarResultadoSiTermino();
            return true;
        }
 
        private bool EjecutarRecolectar(Unidad unidad, int segundos)
        {
            if (Partida.Finalizada) return false;
 
            if (!unidad.EstaViva())
                return Rechazar(unidad.JugadorId, "Recolectar", "la unidad no está disponible");
 
            Celda celda = Partida.Mapa.Celdas[unidad.X, unidad.Y];
 
            if (celda.Recurso == null || celda.CantidadRecurso <= 0)
                return Rechazar(unidad.JugadorId, "Recolectar", "la unidad no está sobre un recurso");
 
            Concurrencia.RecoleccionAutomatica(unidad.JugadorId, unidad.Id, segundos);
 
            Registrar(unidad.JugadorId, "Recolectar",
                "Unidad #" + unidad.Id + " recolecta " + celda.Recurso.Value +
                " durante " + segundos + " s");
 
            return true;
        }
 
        // ---------- Fin de la partida ----------
 
        // Guarda resultado_final.txt una sola vez y detiene los hilos de fondo.
        private void RegistrarResultadoSiTermino()
        {
            if (!Partida.Finalizada || resultadoGuardado)
                return;
 
            resultadoGuardado = true;
 
            Jugador ganador = Partida.ObtenerGanador();
 
            string linea = ganador != null
                ? "Ganó " + ganador.Nombre
                : "Empate";
 
            Archivos.GuardarResultadoFinal(GenerarTextoResultado(ganador));
            Registrar(0, "Fin de partida", linea);
 
            Concurrencia.Detener();
        }
 
        private string GenerarTextoResultado(Jugador ganador)
        {
            StringBuilder sb = new StringBuilder();
 
            sb.AppendLine("RESULTADO FINAL - IMPERIOS EN GUERRA");
            sb.AppendLine("Fecha: " + DateTime.Now);
 
            if (ConexionPerdida)
                sb.AppendLine("Ganador: sin ganador (partida interrumpida por pérdida de conexión)");
            else
                sb.AppendLine("Ganador: " + (ganador != null ? ganador.Nombre : "Empate"));
 
            foreach (Jugador j in new[] { Partida.Jugador1, Partida.Jugador2 })
            {
                string motivo = Partida.MotivoDerrota(j);
 
                if (motivo != null && !ConexionPerdida)
                    sb.AppendLine(j.Nombre + " perdió porque " + motivo + ".");
            }
 
            sb.AppendLine();
 
            foreach (Jugador j in new[] { Partida.Jugador1, Partida.Jugador2 })
            {
                int unidadesVivas = j.Unidades.FindAll(u => u.EstaViva()).Count;
                int edificiosEnPie = j.Edificios.FindAll(e => !e.EstaDestruido()).Count;
 
                sb.AppendLine(j.Nombre + ": Oro=" + j.ObtenerRecurso(TipoRecurso.Oro) +
                    ", Madera=" + j.ObtenerRecurso(TipoRecurso.Madera) +
                    ", Comida=" + j.ObtenerRecurso(TipoRecurso.Comida) +
                    ", Unidades vivas=" + unidadesVivas +
                    ", Edificios en pie=" + edificiosEnPie);
            }
 
            sb.AppendLine();
            sb.AppendLine("Estado final del mapa:");
            sb.Append(GenerarMapaTexto());
            sb.AppendLine();
            sb.AppendLine("Leyenda: C=Centro Urbano, H=Casa, S=Soldado (el número es el jugador),");
            sb.AppendLine("$=Oro, W=Madera, F=Comida, .=celda libre");
 
            return sb.ToString();
        }
 
        private string GenerarMapaTexto()
        {
            StringBuilder sb = new StringBuilder();
 
            for (int x = 0; x < Partida.Mapa.Filas; x++)
            {
                for (int y = 0; y < Partida.Mapa.Columnas; y++)
                {
                    sb.Append(SimboloCelda(x, y).PadRight(3));
                }
 
                sb.AppendLine();
            }
 
            return sb.ToString();
        }
 
        private string SimboloCelda(int x, int y)
        {
            foreach (Jugador j in new[] { Partida.Jugador1, Partida.Jugador2 })
            {
                foreach (Edificio e in j.Edificios)
                {
                    if (!e.EstaDestruido() && e.X == x && e.Y == y)
                        return (e.Tipo == TipoEdificio.CentroUrbano ? "C" : "H") + j.Id;
                }
 
                foreach (Unidad u in j.Unidades)
                {
                    if (u.EstaViva() && u.X == x && u.Y == y)
                        return "S" + j.Id;
                }
            }
 
            Celda celda = Partida.Mapa.Celdas[x, y];
 
            if (celda.Recurso != null && celda.CantidadRecurso > 0)
            {
                switch (celda.Recurso.Value)
                {
                    case TipoRecurso.Oro: return "$";
                    case TipoRecurso.Madera: return "W";
                    default: return "F";
                }
            }
 
            return ".";
        }
 
        private string GenerarTextoConfiguracion()
        {
            StringBuilder sb = new StringBuilder();
 
            sb.AppendLine("CONFIGURACIÓN INICIAL - IMPERIOS EN GUERRA");
            sb.AppendLine("Fecha: " + DateTime.Now);
            sb.AppendLine("Mapa: " + Partida.Mapa.Filas + " x " + Partida.Mapa.Columnas);
            sb.AppendLine();
            sb.AppendLine("Recursos en el mapa:");
 
            for (int x = 0; x < Partida.Mapa.Filas; x++)
            {
                for (int y = 0; y < Partida.Mapa.Columnas; y++)
                {
                    Celda c = Partida.Mapa.Celdas[x, y];
 
                    if (c.Recurso != null)
                        sb.AppendLine("  " + c.Recurso.Value + " (" + c.CantidadRecurso +
                            ") en (" + x + "," + y + ")");
                }
            }
 
            foreach (Jugador j in new[] { Partida.Jugador1, Partida.Jugador2 })
            {
                sb.AppendLine();
                sb.AppendLine(j.Nombre + ":");
                sb.AppendLine("  Recursos: Oro=" + j.ObtenerRecurso(TipoRecurso.Oro) +
                    ", Madera=" + j.ObtenerRecurso(TipoRecurso.Madera) +
                    ", Comida=" + j.ObtenerRecurso(TipoRecurso.Comida));
 
                foreach (Edificio e in j.Edificios)
                    sb.AppendLine("  Edificio " + e.Tipo + " #" + e.Id +
                        " en (" + e.X + "," + e.Y + "), vida " + e.Vida);
 
                foreach (Unidad u in j.Unidades)
                    sb.AppendLine("  Unidad " + u.Tipo + " #" + u.Id +
                        " en (" + u.X + "," + u.Y + "), vida " + u.Vida);
            }
 
            return sb.ToString();
        }
 
        // ---------- Auxiliares ----------
 
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