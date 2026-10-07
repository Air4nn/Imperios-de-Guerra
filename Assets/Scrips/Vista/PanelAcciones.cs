using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Servicios;
 
namespace ImperiosEnGuerra.Vista
{
    // Barra inferior con los botones de acción (con íconos), una línea que explica el botón
    // bajo el mouse y los mensajes del juego. También agrega al Canvas la ventana de ayuda
    // y el menú final. Se coloca en el objeto Canvas; construye su interfaz al iniciar.
    public class PanelAcciones : MonoBehaviour
    {
        private const int MaxMensajes = 4;
        private const string TextoPorDefecto =
            "Pasa el mouse sobre un botón para ver qué hace  |  H = Ayuda";
 
        private static readonly Color ColorPanel = new Color(0.08f, 0.09f, 0.12f, 0.88f);
        private static readonly Color ColorBoton = new Color(0.20f, 0.24f, 0.32f, 1f);
        private static readonly Color ColorActivo = new Color(0.85f, 0.65f, 0.12f, 1f);
 
        private InteraccionMapa interaccion;
        private PanelAyuda ayuda;
        private TextMeshProUGUI textoMensajes;
        private TextMeshProUGUI textoExplicacion;
 
        private readonly List<string> historial = new List<string>();
        private readonly Image[] fondosModo = new Image[3];
 
        private RectTransform[] rectsBotones;
        private string[] explicaciones;
 
        private void Start()
        {
            interaccion = FindAnyObjectByType<InteraccionMapa>();
 
            // Ventana de ayuda y menú final: se agregan solas, sin configurar la escena.
            ayuda = GetComponent<PanelAyuda>();
 
            if (ayuda == null)
                ayuda = gameObject.AddComponent<PanelAyuda>();
 
            if (GetComponent<MenuFinal>() == null)
                gameObject.AddComponent<MenuFinal>();
 
            RectTransform panel = CrearPanel();
 
            // Mensajes del juego (arriba del panel)
            textoMensajes = CrearTexto(panel, "Mensajes", 22, TextAlignmentOptions.TopLeft);
 
            RectTransform rt = textoMensajes.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -8f);
            rt.sizeDelta = new Vector2(-40f, 106f);
 
            // Explicación del botón bajo el mouse (entre los mensajes y los botones)
            textoExplicacion = CrearTexto(panel, "Explicacion", 22, TextAlignmentOptions.MidlineLeft);
            textoExplicacion.color = new Color(1f, 0.85f, 0.35f);
            textoExplicacion.text = TextoPorDefecto;
 
            RectTransform rtExp = textoExplicacion.rectTransform;
            rtExp.anchorMin = new Vector2(0f, 0f);
            rtExp.anchorMax = new Vector2(1f, 0f);
            rtExp.pivot = new Vector2(0.5f, 0f);
            rtExp.anchoredPosition = new Vector2(0f, 92f);
            rtExp.sizeDelta = new Vector2(-40f, 36f);
 
            string[] etiquetas =
            {
                "Mover", "Construir", "Atacar", "Entrenar", "Recolectar", "Cambiar jugador", "Ayuda"
            };
 
            string[] iconos =
            {
                "mover", "construir", "atacar", "entrenar", "recolectar", "jugador", "ayuda"
            };
 
            explicaciones = new[]
            {
                "Mover: selecciona una unidad y haz click en el destino. Avanza una celda cada 0,4 s.",
 
                "Construir: levanta una casa (300 de vida) en la celda libre que elijas. Cuesta " +
                Partida.CostoCasaMadera + " de madera y tarda " + ConcurrenciaService.SegundosConstruccion + " s.",
 
                "Atacar: golpea (25 de daño) a un enemigo desde una celda contigua. También sirve el click derecho.",
 
                "Entrenar: crea un soldado junto a tu Centro Urbano (" + Partida.CostoSoldadoComida +
                " de comida, " + ConcurrenciaService.SegundosEntrenamiento +
                " s). Más soldados = más fuerza para atacar, recolectar y defender.",
 
                "Recolectar: la unidad seleccionada, sobre un recurso, junta 10 por segundo durante " +
                ConcurrenciaService.SegundosRecoleccion + " s.",
 
                "Cambiar jugador: alterna entre el Jugador 1 y el 2 (solo en partida local).",
 
                "Ayuda: explica el objetivo del juego y cada acción (también con la tecla H)."
            };
 
            UnityAction[] acciones =
            {
                () => interaccion.EstablecerModo(ModoAccion.Mover),
                () => interaccion.EstablecerModo(ModoAccion.Construir),
                () => interaccion.EstablecerModo(ModoAccion.Atacar),
                () => interaccion.Entrenar(),
                () => interaccion.Recolectar(),
                () => interaccion.CambiarJugador(),
                () => ayuda.Alternar()
            };
 
            rectsBotones = new RectTransform[etiquetas.Length];
 
            for (int i = 0; i < etiquetas.Length; i++)
            {
                Image fondo = CrearBoton(panel, etiquetas[i], iconos[i], i, etiquetas.Length, acciones[i]);
                rectsBotones[i] = fondo.rectTransform;
 
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
 
            ActualizarExplicacion();
        }
 
        // Muestra qué hace el botón sobre el que está el mouse.
        private void ActualizarExplicacion()
        {
            if (rectsBotones == null || textoExplicacion == null)
                return;
 
            string texto = TextoPorDefecto;
 
            for (int i = 0; i < rectsBotones.Length; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(
                        rectsBotones[i], Input.mousePosition, null))
                {
                    texto = explicaciones[i];
                    break;
                }
            }
 
            if (textoExplicacion.text != texto)
                textoExplicacion.text = texto;
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
            rt.sizeDelta = new Vector2(0f, 240f);
 
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
            Transform padre, string etiqueta, string nombreIcono,
            int indice, int total, UnityAction accion)
        {
            const float ancho = 200f;
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
 
            // Ícono a la izquierda
            Sprite sprite = IconosUI.Cargar(nombreIcono);
 
            GameObject iconoGo = new GameObject("Icono", typeof(RectTransform), typeof(Image));
            iconoGo.transform.SetParent(go.transform, false);
 
            Image icono = iconoGo.GetComponent<Image>();
            icono.sprite = sprite;
            icono.enabled = sprite != null;
            icono.preserveAspect = true;
            icono.raycastTarget = false;
 
            RectTransform rtIcono = iconoGo.GetComponent<RectTransform>();
            rtIcono.anchorMin = new Vector2(0f, 0.5f);
            rtIcono.anchorMax = new Vector2(0f, 0.5f);
            rtIcono.pivot = new Vector2(0f, 0.5f);
            rtIcono.anchoredPosition = new Vector2(10f, 0f);
            rtIcono.sizeDelta = new Vector2(48f, 48f);
 
            // Texto a la derecha del ícono
            TextMeshProUGUI texto = CrearTexto(go.transform, "Texto", 22, TextAlignmentOptions.Center);
            texto.text = etiqueta;
 
            RectTransform rtTexto = texto.rectTransform;
            rtTexto.anchorMin = Vector2.zero;
            rtTexto.anchorMax = Vector2.one;
            rtTexto.offsetMin = new Vector2(62f, 0f);
            rtTexto.offsetMax = new Vector2(-6f, 0f);
 
            return fondo;
        }
    }
}