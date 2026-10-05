using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
 
namespace ImperiosEnGuerra.Vista
{
    // Barra inferior con los botones de acción y los mensajes del juego.
    // Se agrega al objeto Canvas; construye su propia interfaz al iniciar.
    public class PanelAcciones : MonoBehaviour
    {
        private const int MaxMensajes = 4;
 
        private static readonly Color ColorPanel = new Color(0.08f, 0.09f, 0.12f, 0.88f);
        private static readonly Color ColorBoton = new Color(0.20f, 0.24f, 0.32f, 1f);
        private static readonly Color ColorActivo = new Color(0.85f, 0.65f, 0.12f, 1f);
 
        private InteraccionMapa interaccion;
        private TextMeshProUGUI textoMensajes;
 
        private readonly List<string> historial = new List<string>();
        private readonly Image[] fondosModo = new Image[3];
 
        private void Start()
        {
            interaccion = FindAnyObjectByType<InteraccionMapa>();
 
            RectTransform panel = CrearPanel();
 
            textoMensajes = CrearTexto(panel, "Mensajes", 22, TextAlignmentOptions.TopLeft);
 
            RectTransform rt = textoMensajes.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -10f);
            rt.sizeDelta = new Vector2(-40f, 100f);
 
            // Fila de botones, centrada en la parte baja del panel.
            string[] etiquetas =
            {
                "Mover", "Construir", "Atacar", "Entrenar", "Recolectar", "Cambiar jugador"
            };
 
            UnityAction[] acciones =
            {
                () => interaccion.EstablecerModo(ModoAccion.Mover),
                () => interaccion.EstablecerModo(ModoAccion.Construir),
                () => interaccion.EstablecerModo(ModoAccion.Atacar),
                () => interaccion.Entrenar(),
                () => interaccion.Recolectar(),
                () => interaccion.CambiarJugador()
            };
 
            for (int i = 0; i < etiquetas.Length; i++)
            {
                Image fondo = CrearBoton(panel, etiquetas[i], i, etiquetas.Length, acciones[i]);
 
                // Los tres primeros son modos: se resaltan cuando están activos.
                if (i < fondosModo.Length)
                    fondosModo[i] = fondo;
            }
        }
 
        private void Update()
        {
            if (GameManager.Instancia == null || textoMensajes == null)
                return;
 
            // Lee los mensajes que el Controlador dejó en su cola thread-safe.
            var controller = GameManager.Instancia.Controller;
            bool cambio = false;
            string mensaje;
 
            while (controller.Mensajes.TryDequeue(out mensaje))
            {
                historial.Add(mensaje);
 
                if (historial.Count > MaxMensajes)
                    historial.RemoveAt(0);
 
                cambio = true;
            }
 
            if (cambio)
                textoMensajes.text = string.Join("\n", historial);
 
            // Resalta el modo activo.
            if (interaccion != null)
            {
                for (int i = 0; i < fondosModo.Length; i++)
                {
                    if (fondosModo[i] != null)
                        fondosModo[i].color = (int)interaccion.Modo == i ? ColorActivo : ColorBoton;
                }
            }
        }
 
        private RectTransform CrearPanel()
        {
            GameObject go = new GameObject("PanelAcciones_UI", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
 
            Image fondo = go.GetComponent<Image>();
            fondo.color = ColorPanel;
 
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 200f);
 
            return rt;
        }
 
        private TextMeshProUGUI CrearTexto(
            Transform padre, string nombre, float tamano, TextAlignmentOptions alineacion)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
 
            TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.color = Color.white;
            texto.alignment = alineacion;
            texto.raycastTarget = false;
 
            return texto;
        }
 
        private Image CrearBoton(
            Transform padre, string etiqueta, int indice, int total, UnityAction accion)
        {
            const float ancho = 180f;
            const float alto = 70f;
            const float separacion = 12f;
 
            GameObject go = new GameObject(
                "Boton_" + etiqueta, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
 
            Image fondo = go.GetComponent<Image>();
            fondo.color = ColorBoton;
 
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(ancho, alto);
 
            float anchoTotal = total * ancho + (total - 1) * separacion;
            float x = -anchoTotal / 2f + ancho / 2f + indice * (ancho + separacion);
            rt.anchoredPosition = new Vector2(x, 14f);
 
            go.GetComponent<Button>().onClick.AddListener(accion);
 
            TextMeshProUGUI texto = CrearTexto(go.transform, "Texto", 24, TextAlignmentOptions.Center);
            texto.text = etiqueta;
 
            RectTransform rtTexto = texto.rectTransform;
            rtTexto.anchorMin = Vector2.zero;
            rtTexto.anchorMax = Vector2.one;
            rtTexto.offsetMin = Vector2.zero;
            rtTexto.offsetMax = Vector2.zero;
 
            return fondo;
        }
    }
}