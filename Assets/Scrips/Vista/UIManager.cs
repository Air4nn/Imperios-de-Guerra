using UnityEngine;
using TMPro;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Servicios;
 
namespace ImperiosEnGuerra.Vista
{
    public class UIManager : MonoBehaviour
    {
        // Mismo nombre de campo que antes: la referencia del Inspector se conserva.
        public TMP_Text informacion;
 
        private InteraccionMapa interaccion;
 
        private void Start()
        {
            interaccion = FindAnyObjectByType<InteraccionMapa>();
        }
 
        private void Update()
        {
            if (GameManager.Instancia == null || informacion == null)
                return;
 
            var controller = GameManager.Instancia.Controller;
            Partida partida = controller.Partida;
 
            string texto = "<b>IMPERIOS EN GUERRA</b>\n\n";
 
            if (partida.Finalizada)
            {
                Jugador ganador = partida.ObtenerGanador();
 
                texto += "<size=140%><b>" +
                    (ganador != null
                        ? "GANÓ " + ganador.Nombre.ToUpper()
                        : "EMPATE") +
                    "</b></size>\n\n";
            }
            else if (interaccion != null)
            {
                texto += "Controlas: <b>Jugador " + interaccion.JugadorActual + "</b>" +
                    "   Modo: <b>" + interaccion.Modo + "</b>\n";
 
                if (interaccion.Seleccionada != null &&
                    interaccion.Seleccionada.EstaViva())
                {
                    texto += "Unidad #" + interaccion.Seleccionada.Id +
                        "  Vida: " + interaccion.Seleccionada.Vida + "\n";
                }
 
                texto += "\n";
            }
 
            texto += ResumenJugador("#4D80FF", partida.Jugador1);
            texto += ResumenJugador("#FF4D4D", partida.Jugador2);
            texto += TextoTareas(controller);
 
            informacion.text = texto;
        }
 
        private string ResumenJugador(string colorHex, Jugador jugador)
        {
            int unidadesVivas =
                jugador.Unidades.FindAll(u => u.EstaViva()).Count;
 
            int edificiosEnPie =
                jugador.Edificios.FindAll(e => !e.EstaDestruido()).Count;
 
            return
                "<color=" + colorHex + "><b>" + jugador.Nombre.ToUpper() +
                "</b></color>\n" +
                "Oro: " + jugador.ObtenerRecurso(TipoRecurso.Oro) +
                "   Madera: " + jugador.ObtenerRecurso(TipoRecurso.Madera) +
                "   Comida: " + jugador.ObtenerRecurso(TipoRecurso.Comida) +
                "\n" +
                "Unidades: " + unidadesVivas +
                "   Edificios: " + edificiosEnPie +
                "\n\n";
        }
 
        // Avance en tiempo real de las tareas que corren en hilos de fondo.
        private string TextoTareas(ImperiosEnGuerra.Controlador.JuegoController controller)
        {
            string texto = "";
 
            foreach (TareaProgreso tarea in controller.TareasEnCurso)
            {
                if (tarea.Total <= 0)
                    continue;
 
                int llenos = Mathf.Clamp(
                    Mathf.RoundToInt((float)(10 * tarea.Transcurrido / tarea.Total)), 0, 10);
 
                texto +=
                    "J" + tarea.JugadorId + " " + tarea.Descripcion +
                    " [" + new string('#', llenos) + new string('-', 10 - llenos) + "] " +
                    tarea.Transcurrido.ToString("0.0") + "/" + tarea.Total + " s\n";
            }
 
            if (texto.Length == 0)
                return "";
 
            return "<b>TAREAS EN CURSO</b>\n" + texto;
        }
    }
}