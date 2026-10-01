using UnityEngine;
using ImperiosEnGuerra.Modelo;
 
namespace ImperiosEnGuerra.Vista
{
    public class InteraccionMapa : MonoBehaviour
    {
        private Camera camara;
 
        // Jugador que esta controlando el teclado/mouse (Tab lo cambia).
        public int JugadorActual { get; private set; } = 1;
 
        // Unidad seleccionada del jugador actual.
        public Unidad Seleccionada { get; private set; }
 
        private void Start()
        {
            camara = Camera.main;
        }
 
        private void Update()
        {
            if (GameManager.Instancia == null)
                return;
 
            if (GameManager.Instancia.Controller.Partida.Finalizada)
                return;
 
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CambiarJugador();
            }
 
            if (Input.GetKeyDown(KeyCode.C))
            {
                Construir();
            }
 
            if (Input.GetKeyDown(KeyCode.S))
            {
                Entrenar();
            }
 
            if (Input.GetMouseButtonDown(0))
            {
                ClickIzquierdo();
            }
 
            if (Input.GetMouseButtonDown(1))
            {
                ClickDerecho();
            }
        }
 
        private Jugador Actual
        {
            get
            {
                return GameManager.Instancia.Controller.Partida
                    .ObtenerJugador(JugadorActual);
            }
        }
 
        private Jugador Rival
        {
            get
            {
                return GameManager.Instancia.Controller.Partida
                    .ObtenerJugador(JugadorActual == 1 ? 2 : 1);
            }
        }
 
        private void CambiarJugador()
        {
            JugadorActual = JugadorActual == 1 ? 2 : 1;
            Seleccionada = null;
        }
 
        // Convierte la posicion del mouse en una celda (x, y) del mapa.
        private bool ObtenerCeldaBajoMouse(out int x, out int y)
        {
            x = 0;
            y = 0;
 
            if (camara == null)
                return false;
 
            Ray ray = camara.ScreenPointToRay(Input.mousePosition);
 
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return false;
 
            x = Mathf.RoundToInt(hit.point.x);
            y = Mathf.RoundToInt(hit.point.z);
            return true;
        }
 
        private Unidad BuscarUnidadVivaEn(Jugador jugador, int x, int y)
        {
            return jugador.Unidades.Find(
                u => u.EstaViva() && u.X == x && u.Y == y);
        }
 
        // Si no hay unidad valida seleccionada, toma la primera viva del jugador actual.
        private bool AsegurarSeleccion()
        {
            if (Seleccionada == null ||
                !Seleccionada.EstaViva() ||
                Seleccionada.JugadorId != JugadorActual)
            {
                Seleccionada = Actual.Unidades.Find(u => u.EstaViva());
            }
 
            return Seleccionada != null;
        }
 
        // C: construye una casa del jugador actual en la celda bajo el mouse.
        private void Construir()
        {
            if (!ObtenerCeldaBajoMouse(out int x, out int y))
                return;
 
            GameManager.Instancia.Controller.Construir(Actual, x, y);
        }
 
        // S: entrena un soldado del jugador actual junto a su Centro Urbano.
        private void Entrenar()
        {
            GameManager.Instancia.Controller.Entrenar(Actual);
        }
 
        // Click izquierdo: selecciona una unidad propia o mueve la seleccionada.
        private void ClickIzquierdo()
        {
            if (!ObtenerCeldaBajoMouse(out int x, out int y))
                return;
 
            Unidad propia = BuscarUnidadVivaEn(Actual, x, y);
 
            if (propia != null)
            {
                Seleccionada = propia;
                return;
            }
 
            if (!AsegurarSeleccion())
                return;
 
            GameManager.Instancia.Controller.Mover(Seleccionada, x, y);
        }
 
        // Click derecho: ataca la unidad o el edificio enemigo de esa celda.
        private void ClickDerecho()
        {
            if (!ObtenerCeldaBajoMouse(out int x, out int y))
                return;
 
            if (!AsegurarSeleccion())
                return;
 
            var controller = GameManager.Instancia.Controller;
 
            Unidad enemiga = BuscarUnidadVivaEn(Rival, x, y);
 
            if (enemiga != null)
            {
                controller.AtacarUnidad(Seleccionada, enemiga);
                return;
            }
 
            Edificio edificio = Rival.Edificios.Find(
                e => !e.EstaDestruido() && e.X == x && e.Y == y);
 
            if (edificio != null)
            {
                controller.AtacarEdificio(Seleccionada, edificio);
            }
        }
    }
}