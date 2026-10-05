using System.Linq;
 
namespace ImperiosEnGuerra.Modelo
{
    public class Partida
    {
        public const int CostoCasaMadera = 100;
        public const int CostoSoldadoComida = 100;
 
        public Mapa Mapa { get; set; }
        public Jugador Jugador1 { get; set; }
        public Jugador Jugador2 { get; set; }
        public bool Finalizada { get; set; }
        public int SiguienteIdUnidad { get; set; }
        public int SiguienteIdEdificio { get; set; }
 
        public Partida()
        {
            Mapa = new Mapa(15, 15);
            Jugador1 = new Jugador(1, "Jugador 1");
            Jugador2 = new Jugador(2, "Jugador 2");
            Finalizada = false;
            SiguienteIdUnidad = 1;
            SiguienteIdEdificio = 1;
 
            Mapa.AgregarRecurso(3, 3, TipoRecurso.Oro, 1000);
            Mapa.AgregarRecurso(3, 4, TipoRecurso.Madera, 1000);
            Mapa.AgregarRecurso(11, 11, TipoRecurso.Oro, 1000);
            Mapa.AgregarRecurso(11, 10, TipoRecurso.Madera, 1000);
            Mapa.AgregarRecurso(7, 7, TipoRecurso.Comida, 1000);
 
            CrearCentro(Jugador1, 1, 1);
            CrearCentro(Jugador2, 13, 13);
        }
 
        private void CrearCentro(Jugador jugador, int x, int y)
        {
            Edificio centro = new Edificio(
                SiguienteIdEdificio++, TipoEdificio.CentroUrbano,
                1000, x, y, jugador.Id);
 
            jugador.AgregarEdificio(centro);
            Mapa.Ocupar(x, y);
        }
 
        public Jugador ObtenerJugador(int id) => id == 1 ? Jugador1 : Jugador2;
 
        // Cada jugador genera sus propios IDs (rangos distintos), para que dos
        // instancias del juego en red nunca asignen el mismo ID a objetos distintos.
        public int NuevoIdUnidad(Jugador jugador)
        {
            jugador.ContadorUnidades++;
            return jugador.Id * 1000 + jugador.ContadorUnidades;
        }
 
        public int NuevoIdEdificio(Jugador jugador)
        {
            jugador.ContadorEdificios++;
            return jugador.Id * 1000 + jugador.ContadorEdificios;
        }
 
        public Unidad BuscarUnidad(int id) =>
            Jugador1.Unidades.Concat(Jugador2.Unidades).FirstOrDefault(u => u.Id == id);
 
        public Edificio BuscarEdificio(int id) =>
            Jugador1.Edificios.Concat(Jugador2.Edificios).FirstOrDefault(e => e.Id == id);
 
        public bool TieneCentroUrbano(Jugador jugador) =>
            jugador.Edificios.Any(e =>
                e.Tipo == TipoEdificio.CentroUrbano && !e.EstaDestruido());
 
        // Sin unidades vivas, sin soldados en entrenamiento y sin comida para entrenar otro.
        public bool SinUnidadesMilitares(Jugador jugador)
        {
            bool hayUnidades = jugador.Unidades.Any(u => u.EstaViva());
            bool puedeReponer = jugador.EnEntrenamiento > 0 ||
                jugador.TieneRecursos(TipoRecurso.Comida, CostoSoldadoComida);
 
            return !hayUnidades && !puedeReponer;
        }
 
        // Devuelve el motivo de la derrota, o null si el jugador sigue en juego.
        public string MotivoDerrota(Jugador jugador)
        {
            if (!TieneCentroUrbano(jugador))
                return "su Centro Urbano fue destruido";
 
            if (SinUnidadesMilitares(jugador))
                return "se quedó sin unidades militares";
 
            return null;
        }
 
        public bool HaPerdido(Jugador jugador) => MotivoDerrota(jugador) != null;
 
        public void VerificarVictoria()
        {
            Finalizada = HaPerdido(Jugador1) || HaPerdido(Jugador2);
        }
 
        public Jugador ObtenerGanador()
        {
            bool pierde1 = HaPerdido(Jugador1);
            bool pierde2 = HaPerdido(Jugador2);
 
            if (pierde2 && !pierde1) return Jugador1;
            if (pierde1 && !pierde2) return Jugador2;
            return null;
        }
    }
}