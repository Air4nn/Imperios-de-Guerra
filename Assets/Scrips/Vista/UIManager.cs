using UnityEngine;
using TMPro;
using ImperiosEnGuerra.Modelo;
 
namespace ImperiosEnGuerra.Vista
{
    public class UIManager : MonoBehaviour
    {
        // Mismo nombre de campo que antes: la referencia del Inspector se conserva.
        public TMP_Text informacion;
 
        private InteraccionMapa interaccion;
 
        private void Start()
        {
            interaccion = FindFirstObjectByType<InteraccionMapa>();
        }
 
        private void Update()
        {
            if (GameManager.Instancia == null || informacion == null)
                return;
 
            Partida partida =
                GameManager.Instancia.Controller.Partida;
 
            string texto = "<b>IMPERIOS EN GUERRA</b>\n\n";
 
            if (partida.Finalizada)
            {
                Jugador ganador = partida.ObtenerGanador();
 
                texto += "<size=140%><b>" +
                    (ganador != null
                        ? "GANO " + ganador.Nombre.ToUpper()
                        : "EMPATE") +
                    "</b></size>\n\n";
            }
            else if (interaccion != null)
            {
                texto += "Controlas: <b>Jugador " +
                    interaccion.JugadorActual + "</b>\n";
 
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
 
            texto +=
                "CONTROLES\n" +
                "Tab = Cambiar de jugador\n" +
                "Click izq = Seleccionar / Mover\n" +
                "Click der = Atacar (desde una celda contigua)\n" +
                "C = Construir casa (bajo el mouse)\n" +
                "S = Entrenar soldado";
 
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
    }
}