using System.Linq;

namespace ImperiosEnGuerra.Modelo
{
    public class Partida
    {
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

        public Unidad BuscarUnidad(int id) =>
            Jugador1.Unidades.Concat(Jugador2.Unidades).FirstOrDefault(u => u.Id == id);

        public Edificio BuscarEdificio(int id) =>
            Jugador1.Edificios.Concat(Jugador2.Edificios).FirstOrDefault(e => e.Id == id);

        public void VerificarVictoria()
        {
            bool centro1 = Jugador1.Edificios.Any(e =>
                e.Tipo == TipoEdificio.CentroUrbano && !e.EstaDestruido());

            bool centro2 = Jugador2.Edificios.Any(e =>
                e.Tipo == TipoEdificio.CentroUrbano && !e.EstaDestruido());

            Finalizada = !centro1 || !centro2;
        }

        public Jugador ObtenerGanador()
        {
            bool centro1 = Jugador1.Edificios.Any(e =>
                e.Tipo == TipoEdificio.CentroUrbano && !e.EstaDestruido());

            bool centro2 = Jugador2.Edificios.Any(e =>
                e.Tipo == TipoEdificio.CentroUrbano && !e.EstaDestruido());

            if (centro1 && !centro2) return Jugador1;
            if (centro2 && !centro1) return Jugador2;
            return null;
        }
    }
}