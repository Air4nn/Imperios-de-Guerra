using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ImperiosEnGuerra.Modelo;
using ImperiosEnGuerra.Servicios;
 
namespace ImperiosEnGuerra.Vista
{
    // Ventana de ayuda: objetivo del juego y explicación de cada acción.
    // Se abre con el botón "Ayuda" o con la tecla H, y se cierra con H, Esc o "Cerrar".
    // PanelAcciones la agrega al Canvas automáticamente.
    public class PanelAyuda : MonoBehaviour
    {
        private GameObject panel;
 
        private void Start()
        {
            panel = new GameObject("PanelAyuda_UI", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.70f);
 
            RectTransform rtPanel = panel.GetComponent<RectTransform>();
            rtPanel.anchorMin = Vector2.zero;
            rtPanel.anchorMax = Vector2.one;
            rtPanel.offsetMin = Vector2.zero;
            rtPanel.offsetMax = Vector2.zero;
 
            GameObject tarjeta = new GameObject("Tarjeta", typeof(RectTransform), typeof(Image));
            tarjeta.transform.SetParent(panel.transform, false);
            tarjeta.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.17f, 0.98f);
 
            RectTransform rt = tarjeta.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1400f, 840f);
            rt.anchoredPosition = Vector2.zero;
 
            TextMeshProUGUI titulo = CrearTexto(tarjeta.transform, "Titulo",
                new Vector2(0f, 375f), new Vector2(1300f, 70f), TextAlignmentOptions.Center);
            titulo.fontSize = 52;
            titulo.text = "<b>CÓMO JUGAR</b>";
 
            TextMeshProUGUI cuerpo = CrearTexto(tarjeta.transform, "Cuerpo",
                new Vector2(0f, 20f), new Vector2(1300f, 680f), TextAlignmentOptions.TopLeft);
            cuerpo.enableAutoSizing = true;
            cuerpo.fontSizeMin = 14;
            cuerpo.fontSizeMax = 24;
            cuerpo.text = TextoAyuda();
 
            CrearBotonCerrar(tarjeta.transform);
 
            panel.SetActive(false);
        }
 
        private void Update()
        {
            if (panel == null || GameManager.Instancia == null)
                return;
 
            // Solo responde al teclado cuando la partida ya empezó (no mientras se escribe la IP).
            if (!GameManager.Instancia.Controller.PuedeJugar)
                return;
 
            if (Input.GetKeyDown(KeyCode.H))
                Alternar();
 
            if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                panel.SetActive(false);
        }
 
        public void Alternar()
        {
            if (panel == null)
                return;
 
            panel.SetActive(!panel.activeSelf);
 
            if (panel.activeSelf)
                panel.transform.SetAsLastSibling();
        }
 
        private string TextoAyuda()
        {
            return
                "<b><color=#FFD54F>OBJETIVO</color></b>\n" +
                "Destruye el Centro Urbano del rival o deja al rival sin unidades. " +
                "Pierdes si tu Centro Urbano cae, o si te quedas sin soldados y sin comida para entrenar otro.\n\n" +
 
                "<b><color=#FFD54F>ACCIONES</color></b>\n" +
                "• <b>Mover:</b> haz click en una unidad tuya para seleccionarla (aro amarillo) y luego en el destino. " +
                "Avanza una celda cada 0,4 s.\n" +
                "• <b>Construir:</b> levanta una casa en la celda libre que elijas. Cuesta " +
                Partida.CostoCasaMadera + " de madera y tarda " + ConcurrenciaService.SegundosConstruccion +
                " s. Es un edificio con 300 de vida que refuerza tu territorio (no produce recursos).\n" +
                "• <b>Entrenar:</b> crea un soldado nuevo junto a tu Centro Urbano. Cuesta " +
                Partida.CostoSoldadoComida + " de comida y tarda " + ConcurrenciaService.SegundosEntrenamiento +
                " s. Los soldados son tu fuerza: atacan (25 de daño por golpe), recolectan recursos y defienden tu base. " +
                "Entrenar más soldados te permite atacar con varios a la vez, reponer bajas y no quedarte sin unidades.\n" +
                "• <b>Recolectar:</b> pon un soldado sobre una celda de recurso (oro, madera o comida) y pulsa el botón: " +
                "junta 10 por segundo durante " + ConcurrenciaService.SegundosRecoleccion + " s. " +
                "Los recursos pagan las construcciones y el entrenamiento.\n" +
                "• <b>Atacar:</b> desde una celda contigua al enemigo, haz click en su unidad o edificio. " +
                "Las barras de vida muestran cuánto le queda y aparece el daño de cada golpe.\n" +
                "• <b>Cambiar jugador:</b> alterna entre el Jugador 1 y el 2 (solo en partida local).\n\n" +
 
                "<b><color=#FFD54F>TECLAS</color></b>\n" +
                "M = Mover    C = Construir    A = Atacar    S = Entrenar    R = Recolectar    " +
                "Tab = Cambiar jugador    H = Ayuda\n" +
                "Click derecho = ataque rápido en cualquier modo.\n\n" +
 
                "<b><color=#FFD54F>CONSEJO</color></b>\n" +
                "Recolecta comida y oro al inicio, entrena varios soldados y ataca el Centro Urbano enemigo con todos a la vez.";
        }
 
        // ---------- Construcción de la interfaz ----------
 
        private TextMeshProUGUI CrearTexto(
            Transform padre, string nombre, Vector2 posicion, Vector2 dimension,
            TextAlignmentOptions alineacion)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
 
            TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
            texto.color = Color.white;
            texto.alignment = alineacion;
            texto.raycastTarget = false;
 
            RectTransform rt = texto.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = dimension;
 
            return texto;
        }
 
        private void CrearBotonCerrar(Transform padre)
        {
            GameObject go = new GameObject("Boton_Cerrar", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
 
            go.GetComponent<Image>().color = new Color(0.20f, 0.24f, 0.32f, 1f);
            go.GetComponent<Button>().onClick.AddListener(() => panel.SetActive(false));
 
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -375f);
            rt.sizeDelta = new Vector2(280f, 64f);
 
            TextMeshProUGUI texto = CrearTexto(go.transform, "Texto", Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center);
            texto.fontSize = 28;
            texto.text = "Cerrar (H)";
 
            RectTransform rtTexto = texto.rectTransform;
            rtTexto.anchorMin = Vector2.zero;
            rtTexto.anchorMax = Vector2.one;
            rtTexto.offsetMin = Vector2.zero;
            rtTexto.offsetMax = Vector2.zero;
        }
    }
}