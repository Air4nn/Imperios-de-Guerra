using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ImperiosEnGuerra.Controlador;
using ImperiosEnGuerra.Modelo;
 
namespace ImperiosEnGuerra.Vista
{
    // Pantalla que aparece cuando termina la partida (o se pierde la conexión):
    // muestra el ganador, el motivo, un resumen de cada jugador y dos botones.
    // PanelAcciones la agrega al Canvas automáticamente.
    public class MenuFinal : MonoBehaviour
    {
        // Pausa breve para ver el golpe final antes de mostrar el menú.
        private const float RetardoMostrar = 1.5f;
 
        private GameObject panel;
        private TextMeshProUGUI titulo;
        private TextMeshProUGUI subtitulo;
        private TextMeshProUGUI detalle;
        private TextMeshProUGUI pie;
 
        private float tiempoFin;
        private bool mostrado;
 
        private void Start()
        {
            panel = new GameObject("MenuFinal_UI", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
 
            RectTransform rtPanel = panel.GetComponent<RectTransform>();
            rtPanel.anchorMin = Vector2.zero;
            rtPanel.anchorMax = Vector2.one;
            rtPanel.offsetMin = Vector2.zero;
            rtPanel.offsetMax = Vector2.zero;
 
            GameObject tarjeta = new GameObject("Tarjeta", typeof(RectTransform), typeof(Image));
            tarjeta.transform.SetParent(panel.transform, false);
            tarjeta.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.17f, 1f);
 
            RectTransform rt = tarjeta.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1000f, 680f);
            rt.anchoredPosition = Vector2.zero;
 
            titulo = CrearTexto(tarjeta.transform, "Titulo", 76,
                new Vector2(0f, 250f), new Vector2(940f, 110f));
            subtitulo = CrearTexto(tarjeta.transform, "Subtitulo", 32,
                new Vector2(0f, 140f), new Vector2(940f, 110f));
            detalle = CrearTexto(tarjeta.transform, "Detalle", 28,
                new Vector2(0f, 20f), new Vector2(940f, 130f));
            pie = CrearTexto(tarjeta.transform, "Pie", 20,
                new Vector2(0f, -100f), new Vector2(940f, 80f));
 
            CrearBoton(tarjeta.transform, "Jugar de nuevo", new Vector2(-230f, -250f),
                new Color(0.18f, 0.48f, 0.30f), JugarDeNuevo);
            CrearBoton(tarjeta.transform, "Salir", new Vector2(230f, -250f),
                new Color(0.50f, 0.20f, 0.20f), Salir);
 
            panel.SetActive(false);
        }
 
        private void Update()
        {
            if (mostrado || GameManager.Instancia == null)
                return;
 
            JuegoController controller = GameManager.Instancia.Controller;
 
            if (!controller.Partida.Finalizada && !controller.ConexionPerdida)
                return;
 
            tiempoFin += Time.deltaTime;
 
            if (tiempoFin >= RetardoMostrar)
                Mostrar(controller);
        }
 
        private void Mostrar(JuegoController controller)
        {
            mostrado = true;
 
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
 
            Partida partida = controller.Partida;
 
            if (controller.ConexionPerdida)
            {
                titulo.text = "<color=#FFB74D>CONEXIÓN PERDIDA</color>";
                subtitulo.text = "Se perdió la conexión con el oponente.\nLa partida se interrumpió.";
            }
            else
            {
                Jugador ganador = partida.ObtenerGanador();
 
                if (ganador == null)
                {
                    titulo.text = "EMPATE";
                    subtitulo.text = "Ningún jugador logró imponerse.";
                }
                else
                {
                    string color = ganador.Id == 1 ? "#4D80FF" : "#FF4D4D";
                    titulo.text = "<color=" + color + ">¡GANÓ " + ganador.Nombre.ToUpper() + "!</color>";
 
                    Jugador perdedor = partida.ObtenerJugador(ganador.Id == 1 ? 2 : 1);
                    string motivo = partida.MotivoDerrota(perdedor);
 
                    subtitulo.text = perdedor.Nombre + " perdió porque " +
                        (motivo != null ? motivo : "fue derrotado") + ".";
 
                    if (controller.EnRed)
                    {
                        subtitulo.text += "\n" + (ganador.Id == controller.JugadorLocalId
                            ? "¡Felicitaciones, ganaste!"
                            : "Esta vez perdiste. ¡Pide la revancha!");
                    }
                }
            }
 
            detalle.text =
                Resumen("#4D80FF", partida.Jugador1) + "\n" +
                Resumen("#FF4D4D", partida.Jugador2);
 
            pie.text = "Los registros de la partida se guardaron en:\n" + controller.Archivos.Carpeta;
        }
 
        private string Resumen(string colorHex, Jugador jugador)
        {
            int unidades = jugador.Unidades.FindAll(u => u.EstaViva()).Count;
            int edificios = jugador.Edificios.FindAll(e => !e.EstaDestruido()).Count;
 
            return
                "<color=" + colorHex + "><b>" + jugador.Nombre + "</b></color>   " +
                "Unidades: " + unidades +
                "   Edificios: " + edificios +
                "   Oro: " + jugador.ObtenerRecurso(TipoRecurso.Oro) +
                "   Madera: " + jugador.ObtenerRecurso(TipoRecurso.Madera) +
                "   Comida: " + jugador.ObtenerRecurso(TipoRecurso.Comida);
        }
 
        // ---------- Botones ----------
 
        private void JugarDeNuevo()
        {
            // Cierra hilos y sockets antes de recargar la escena.
            if (GameManager.Instancia != null)
                GameManager.Instancia.Controller.Detener();
 
            Scene escena = SceneManager.GetActiveScene();
 
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                escena.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(escena.buildIndex);
#endif
        }
 
        private void Salir()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
 
        // ---------- Construcción de la interfaz ----------
 
        private TextMeshProUGUI CrearTexto(
            Transform padre, string nombre, float tamano, Vector2 posicion, Vector2 dimension)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
 
            TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.color = Color.white;
            texto.alignment = TextAlignmentOptions.Center;
            texto.raycastTarget = false;
 
            RectTransform rt = texto.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = dimension;
 
            return texto;
        }
 
        private void CrearBoton(
            Transform padre, string etiqueta, Vector2 posicion, Color color, UnityAction accion)
        {
            GameObject go = new GameObject(
                "Boton_" + etiqueta, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
 
            go.GetComponent<Image>().color = color;
            go.GetComponent<Button>().onClick.AddListener(accion);
 
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = new Vector2(380f, 72f);
 
            TextMeshProUGUI texto = CrearTexto(go.transform, "Texto", 30, Vector2.zero, Vector2.zero);
            texto.text = etiqueta;
 
            RectTransform rtTexto = texto.rectTransform;
            rtTexto.anchorMin = Vector2.zero;
            rtTexto.anchorMax = Vector2.one;
            rtTexto.offsetMin = Vector2.zero;
            rtTexto.offsetMax = Vector2.zero;
        }
    }
}