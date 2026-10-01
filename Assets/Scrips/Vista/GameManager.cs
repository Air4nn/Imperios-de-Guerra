using UnityEngine;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Controlador;

namespace ImperiosEnGuerra.Vista
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instancia { get; private set; }

        public JuegoController Controller { get; private set; }

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;

            Controller = new JuegoController();

            CrearUnidadInicial(
                Controller.Partida.Jugador1,
                2,
                2
            );

            CrearUnidadInicial(
                Controller.Partida.Jugador2,
                12,
                12
            );
        }

        private void CrearUnidadInicial(
            Jugador jugador,
            int x,
            int y)
        {
            if (!Controller.Partida.Mapa.EstaLibre(x, y))
                return;

            Unidad unidad = new Unidad(
                Controller.Partida.SiguienteIdUnidad++,
                TipoUnidad.Soldado,
                100,
                25,
                x,
                y,
                jugador.Id
            );

            jugador.AgregarUnidad(unidad);

            Controller.Partida.Mapa.Ocupar(x, y);
        }
    }
}