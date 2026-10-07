using UnityEngine;
using UnityEngine.EventSystems;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Servicios;
 
namespace ImperiosEnGuerra.Vista
{
    public enum ModoAccion
    {
        Mover = 0,
        Construir = 1,
        Atacar = 2
    }
 
    public class InteraccionMapa : MonoBehaviour
    {
        private Camera camara;
 
        // Solo para el modo local (Tab alterna entre los dos jugadores).
        private int jugadorLocal = 1;
 
        // Jugador que controla esta instancia: en red es fijo, en local lo cambia Tab.
        public int JugadorActual
        {
            get
            {
                var controller = GameManager.Instancia.Controller;
                return controller.EnRed ? controller.JugadorLocalId : jugadorLocal;
            }
        }
 
        // Unidad seleccionada del jugador actual.
        public Unidad Seleccionada { get; private set; }
 
        // Acción que se ejecutará con el próximo click izquierdo sobre el mapa.
        public ModoAccion Modo { get; private set; } = ModoAccion.Mover;
 
        private void Start()
        {
            camara = Camera.main;
        }
 
        private void Update()
        {
            if (GameManager.Instancia == null)
                return;
 
            var controller = GameManager.Instancia.Controller;
 
            // Bloqueado mientras se elige el modo, se espera al oponente o la partida terminó.
            if (!controller.PuedeJugar || controller.Partida.Finalizada)
                return;
 
            // Atajos de teclado (equivalen a los botones).
            if (Input.GetKeyDown(KeyCode.Tab)) CambiarJugador();
            if (Input.GetKeyDown(KeyCode.M)) EstablecerModo(ModoAccion.Mover);
            if (Input.GetKeyDown(KeyCode.C)) EstablecerModo(ModoAccion.Construir);
            if (Input.GetKeyDown(KeyCode.A)) EstablecerModo(ModoAccion.Atacar);
            if (Input.GetKeyDown(KeyCode.S)) Entrenar();
            if (Input.GetKeyDown(KeyCode.R)) Recolectar();
 
            // Un click sobre un botón o sobre el panel no debe actuar sobre el mapa.
            bool sobreUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();
 
            if (sobreUI)
                return;
 
            if (Input.GetMouseButtonDown(0)) ClickIzquierdo();
            if (Input.GetMouseButtonDown(1)) ClickDerecho();
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
 
        // ---------- Acciones que también usan los botones ----------
 
        public void CambiarJugador()
        {
            var controller = GameManager.Instancia.Controller;
 
            if (controller.EnRed)
            {
                controller.Aviso("En red cada instancia controla un solo jugador");
                return;
            }
 
            jugadorLocal = jugadorLocal == 1 ? 2 : 1;
            Seleccionada = null;
            Modo = ModoAccion.Mover;
 
            controller.Aviso("Ahora controlas al Jugador " + jugadorLocal);
        }
 
        public void EstablecerModo(ModoAccion modo)
        {
            Modo = modo;
 
            string ayuda;
 
            switch (modo)
            {
                case ModoAccion.Construir:
                    ayuda = "Construir: haz click en una celda libre";
                    break;
                case ModoAccion.Atacar:
                    ayuda = "Atacar: haz click en un enemigo (desde una celda contigua)";
                    break;
                default:
                    ayuda = "Mover: haz click en el destino de la unidad seleccionada";
                    break;
            }
 
            GameManager.Instancia.Controller.Aviso(ayuda);
        }
 
        // Entrena un soldado del jugador actual junto a su Centro Urbano.
        public void Entrenar()
        {
            GameManager.Instancia.Controller.Entrenar(Actual);
        }
 
        // La unidad seleccionada recolecta 10 segundos en un hilo aparte.
        public void Recolectar()
        {
            if (!AsegurarSeleccion())
            {
                GameManager.Instancia.Controller.Aviso("No tienes unidades disponibles");
                return;
            }
 
            GameManager.Instancia.Controller.RecolectarAutomatico(Seleccionada, ConcurrenciaService.SegundosRecoleccion);
        }
 
        // ---------- Auxiliares ----------
 
        // Convierte la posición del mouse en una celda (x, y) del mapa.
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
 
        // Si no hay unidad válida seleccionada, toma la primera viva del jugador actual.
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
 
        // ---------- Clicks sobre el mapa ----------
 
        // Click izquierdo: selecciona una unidad propia o ejecuta el modo activo.
        private void ClickIzquierdo()
        {
            if (!ObtenerCeldaBajoMouse(out int x, out int y))
                return;
 
            var controller = GameManager.Instancia.Controller;
 
            Unidad propia = BuscarUnidadVivaEn(Actual, x, y);
 
            if (propia != null)
            {
                Seleccionada = propia;
                return;
            }
 
            switch (Modo)
            {
                case ModoAccion.Construir:
                    controller.Construir(Actual, x, y);
                    Modo = ModoAccion.Mover;
                    break;
 
                case ModoAccion.Atacar:
                    Atacar(x, y);
                    break;
 
                default:
                    if (AsegurarSeleccion())
                        controller.Mover(Seleccionada, x, y);
                    else
                        controller.Aviso("No tienes unidades disponibles");
                    break;
            }
        }
 
        // Click derecho: ataque rápido en cualquier modo.
        private void ClickDerecho()
        {
            if (!ObtenerCeldaBajoMouse(out int x, out int y))
                return;
 
            Atacar(x, y);
        }
 
        // Ataca la unidad o el edificio enemigo que haya en la celda (x, y).
        private void Atacar(int x, int y)
        {
            var controller = GameManager.Instancia.Controller;
 
            if (!AsegurarSeleccion())
            {
                controller.Aviso("No tienes unidades disponibles");
                return;
            }
 
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
                return;
            }
 
            controller.Aviso("No hay un enemigo en esa celda");
        }
    }
}