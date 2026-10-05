using System;
using ImperiosEnGuerra.Modelo;
 
namespace ImperiosEnGuerra.Servicios
{
    public enum ResultadoPaso
    {
        EnCamino,
        Llego,
        Bloqueado,
        Cancelado
    }
 
    public class JuegoService
    {
        private readonly Partida partida;
 
        // Protege el modelo compartido: varios hilos de fondo (recolección, movimiento,
        // temporizadores) y el hilo principal de Unity acceden a él.
        private readonly object candado = new object();
 
        public JuegoService(Partida partida) { this.partida = partida; }
 
        // ---------- Preparación de la partida ----------
 
        public bool CrearUnidadInicial(int jugadorId, int x, int y)
        {
            lock (candado)
            {
                Jugador jugador = partida.ObtenerJugador(jugadorId);
 
                if (!partida.Mapa.EstaLibre(x, y)) return false;
 
                Unidad soldado = new Unidad(
                    partida.NuevoIdUnidad(jugador), TipoUnidad.Soldado,
                    100, 25, x, y, jugadorId);
 
                jugador.AgregarUnidad(soldado);
                partida.Mapa.Ocupar(x, y);
                return true;
            }
        }
 
        // ---------- Construcción: primero se reserva, luego se completa ----------
 
        // Valida, cobra la madera y reserva la celda. La casa aparece al completarse.
        public bool ReservarConstruccionCasa(int jugadorId, int x, int y)
        {
            lock (candado)
            {
                Jugador jugador = partida.ObtenerJugador(jugadorId);
 
                if (partida.Finalizada) return false;
                if (!partida.Mapa.EstaLibre(x, y)) return false;
                if (!jugador.TieneRecursos(TipoRecurso.Madera, Partida.CostoCasaMadera)) return false;
 
                jugador.GastarRecurso(TipoRecurso.Madera, Partida.CostoCasaMadera);
                partida.Mapa.Ocupar(x, y);
                return true;
            }
        }
 
        // Se ejecuta en el hilo principal cuando termina el temporizador.
        public void CompletarConstruccionCasa(int jugadorId, int x, int y)
        {
            lock (candado)
            {
                Jugador jugador = partida.ObtenerJugador(jugadorId);
 
                Edificio casa = new Edificio(
                    partida.NuevoIdEdificio(jugador), TipoEdificio.Casa,
                    300, x, y, jugadorId);
 
                jugador.AgregarEdificio(casa);
            }
        }
 
        // ---------- Entrenamiento: primero se reserva, luego se completa ----------
 
        public bool ReservarEntrenamiento(int jugadorId, int x, int y)
        {
            lock (candado)
            {
                Jugador jugador = partida.ObtenerJugador(jugadorId);
 
                if (partida.Finalizada) return false;
                if (!partida.Mapa.EstaLibre(x, y)) return false;
                if (!jugador.TieneRecursos(TipoRecurso.Comida, Partida.CostoSoldadoComida)) return false;
 
                jugador.GastarRecurso(TipoRecurso.Comida, Partida.CostoSoldadoComida);
                partida.Mapa.Ocupar(x, y);
                jugador.EnEntrenamiento++;
                return true;
            }
        }
 
        // Se ejecuta en el hilo principal cuando termina el temporizador.
        public void CompletarEntrenamiento(int jugadorId, int x, int y)
        {
            lock (candado)
            {
                Jugador jugador = partida.ObtenerJugador(jugadorId);
 
                Unidad soldado = new Unidad(
                    partida.NuevoIdUnidad(jugador), TipoUnidad.Soldado,
                    100, 25, x, y, jugadorId);
 
                jugador.AgregarUnidad(soldado);
 
                if (jugador.EnEntrenamiento > 0)
                    jugador.EnEntrenamiento--;
            }
        }
 
        // ---------- Movimiento: una celda por llamada ----------
 
        // Avanza una celda hacia el destino (diagonal; si está bloqueada, en X o en Y).
        public ResultadoPaso DarPaso(int jugadorId, int unidadId, int destinoX, int destinoY)
        {
            lock (candado)
            {
                Unidad unidad = partida.BuscarUnidad(unidadId);
 
                if (unidad == null || unidad.JugadorId != jugadorId ||
                    !unidad.EstaViva() || partida.Finalizada)
                    return ResultadoPaso.Cancelado;
 
                if (unidad.X == destinoX && unidad.Y == destinoY)
                    return ResultadoPaso.Llego;
 
                int dx = Math.Sign(destinoX - unidad.X);
                int dy = Math.Sign(destinoY - unidad.Y);
 
                int[,] opciones = { { dx, dy }, { dx, 0 }, { 0, dy } };
 
                for (int i = 0; i < 3; i++)
                {
                    int ox = opciones[i, 0];
                    int oy = opciones[i, 1];
 
                    if (ox == 0 && oy == 0) continue;
 
                    int nx = unidad.X + ox;
                    int ny = unidad.Y + oy;
 
                    if (!partida.Mapa.EstaLibre(nx, ny)) continue;
 
                    partida.Mapa.Liberar(unidad.X, unidad.Y);
                    unidad.X = nx;
                    unidad.Y = ny;
                    partida.Mapa.Ocupar(nx, ny);
 
                    return (nx == destinoX && ny == destinoY)
                        ? ResultadoPaso.Llego
                        : ResultadoPaso.EnCamino;
                }
 
                return ResultadoPaso.Bloqueado;
            }
        }
 
        // ---------- Combate ----------
 
        public bool AtacarUnidad(int jugadorId, int atacanteId, int objetivoId)
        {
            lock (candado)
            {
                Unidad atacante = partida.BuscarUnidad(atacanteId);
                Unidad objetivo = partida.BuscarUnidad(objetivoId);
 
                if (atacante == null || objetivo == null) return false;
                if (atacante.JugadorId != jugadorId) return false;
                if (objetivo.JugadorId == jugadorId) return false;
 
                objetivo.RecibirDanio(atacante.Ataque);
 
                if (!objetivo.EstaViva())
                {
                    partida.Mapa.Liberar(objetivo.X, objetivo.Y);
                }
 
                partida.VerificarVictoria();
                return true;
            }
        }
 
        public bool AtacarEdificio(int jugadorId, int atacanteId, int objetivoId)
        {
            lock (candado)
            {
                Unidad atacante = partida.BuscarUnidad(atacanteId);
                Edificio objetivo = partida.BuscarEdificio(objetivoId);
 
                if (atacante == null || objetivo == null) return false;
                if (atacante.JugadorId != jugadorId) return false;
                if (objetivo.JugadorId == jugadorId) return false;
 
                objetivo.RecibirDanio(atacante.Ataque);
                partida.VerificarVictoria();
                return true;
            }
        }
 
        // ---------- Recolección ----------
 
        public void RecolectarRecurso(int jugadorId, int unidadId)
        {
            lock (candado)
            {
                Unidad unidad = partida.BuscarUnidad(unidadId);
                if (unidad == null || unidad.JugadorId != jugadorId) return;
 
                Celda celda = partida.Mapa.Celdas[unidad.X, unidad.Y];
 
                if (celda.Recurso == null || celda.CantidadRecurso <= 0) return;
 
                int cantidad = Math.Min(10, celda.CantidadRecurso);
                partida.ObtenerJugador(jugadorId).AgregarRecurso(celda.Recurso.Value, cantidad);
                celda.CantidadRecurso -= cantidad;
            }
        }
    }
}