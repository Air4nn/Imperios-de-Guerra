using System;
using ImperiosEnGuerra.Modelo;
 
namespace ImperiosEnGuerra.Servicios
{
    public class JuegoService
    {
        private readonly Partida partida;
 
        // Protege el modelo: la recoleccion automatica corre en otro hilo
        // y las demas acciones en el hilo principal de Unity.
        private readonly object candado = new object();
 
        public JuegoService(Partida partida) { this.partida = partida; }
 
        public bool ConstruirCasa(int jugadorId, int x, int y)
        {
            lock (candado)
            {
                Jugador jugador = partida.ObtenerJugador(jugadorId);
 
                if (!partida.Mapa.EstaLibre(x, y)) return false;
                if (!jugador.TieneRecursos(TipoRecurso.Madera, 100)) return false;
 
                jugador.GastarRecurso(TipoRecurso.Madera, 100);
 
                Edificio casa = new Edificio(
                    partida.SiguienteIdEdificio++, TipoEdificio.Casa,
                    300, x, y, jugadorId);
 
                jugador.AgregarEdificio(casa);
                partida.Mapa.Ocupar(x, y);
                return true;
            }
        }
 
        public bool EntrenarSoldado(int jugadorId, int x, int y)
        {
            lock (candado)
            {
                Jugador jugador = partida.ObtenerJugador(jugadorId);
 
                if (!partida.Mapa.EstaLibre(x, y)) return false;
                if (!jugador.TieneRecursos(TipoRecurso.Comida, 100)) return false;
 
                jugador.GastarRecurso(TipoRecurso.Comida, 100);
 
                Unidad soldado = new Unidad(
                    partida.SiguienteIdUnidad++, TipoUnidad.Soldado,
                    100, 25, x, y, jugadorId);
 
                jugador.AgregarUnidad(soldado);
                partida.Mapa.Ocupar(x, y);
                return true;
            }
        }
 
        public bool MoverUnidad(int jugadorId, int unidadId, int x, int y)
        {
            lock (candado)
            {
                Unidad unidad = partida.BuscarUnidad(unidadId);
 
                if (unidad == null || unidad.JugadorId != jugadorId) return false;
                if (!partida.Mapa.EstaLibre(x, y)) return false;
 
                partida.Mapa.Liberar(unidad.X, unidad.Y);
                unidad.X = x;
                unidad.Y = y;
                partida.Mapa.Ocupar(x, y);
                return true;
            }
        }
 
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