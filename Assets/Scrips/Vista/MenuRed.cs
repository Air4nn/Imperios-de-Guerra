using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ImperiosEnGuerra.Controlador;
using ImperiosEnGuerra.Servicios;
 
namespace ImperiosEnGuerra.Vista
{
    // Pantalla inicial: jugar local, crear partida (anfitrión) o unirse a una (cliente).
    // Se agrega al objeto Canvas; construye su propia interfaz al iniciar.
    public class MenuRed : MonoBehaviour
    {
        private enum Fase
        {
            Principal,
            IngresandoIp,
            Esperando
        }
 
        private static readonly Color ColorFondo = new Color(0.04f, 0.06f, 0.09f, 0.97f);
        private static readonly Color ColorBoton = new Color(0.20f, 0.24f, 0.32f, 1f);
 
        private GameObject panel;
        private GameObject grupoPrincipal;
        private GameObject grupoIp;
        private GameObject grupoEspera;
 
        private TextMeshProUGUI textoIp;
        private TextMeshProUGUI textoEstado;
 
        private Fase fase = Fase.Principal;
        private string ip = "127.0.0.1";
 
        private void Start()
        {
            panel = new GameObject("MenuRed_UI", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            panel.GetComponent<Image>().color = ColorFondo;
 
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
 
            TextMeshProUGUI titulo = CrearTexto(
                panel.transform, "Titulo", 64,
                new Vector2(0f, 280f), new Vector2(1200f, 110f), TextAlignmentOptions.Center);
            titulo.text = "<b>IMPERIOS EN GUERRA</b>";
 
            // Menú principal
            grupoPrincipal = CrearGrupo("GrupoPrincipal");
            CrearBoton(grupoPrincipal.transform, "Jugar local (2 jugadores, un equipo)",
                new Vector2(0f, 90f), JugarLocal);
            CrearBoton(grupoPrincipal.transform, "Crear partida (Anfitrión)",
                new Vector2(0f, 0f), CrearPartida);
            CrearBoton(grupoPrincipal.transform, "Unirse a una partida (Cliente)",
                new Vector2(0f, -90f), UnirsePartida);
 
            // Ingreso de la IP del anfitrión
            grupoIp = CrearGrupo("GrupoIp");
            textoIp = CrearTexto(grupoIp.transform, "TextoIp", 36,
                new Vector2(0f, 90f), new Vector2(1000f, 70f), TextAlignmentOptions.Center);
 
            TextMeshProUGUI ayuda = CrearTexto(grupoIp.transform, "Ayuda", 22,
                new Vector2(0f, 30f), new Vector2(1000f, 50f), TextAlignmentOptions.Center);
            ayuda.text = "Escribe la IP del anfitrión con el teclado. Enter = conectar.";
 
            CrearBoton(grupoIp.transform, "Conectar", new Vector2(0f, -60f), Conectar);
            CrearBoton(grupoIp.transform, "Volver", new Vector2(0f, -150f), Volver);
 
            // Espera / conexión en curso
            grupoEspera = CrearGrupo("GrupoEspera");
            textoEstado = CrearTexto(grupoEspera.transform, "TextoEstado", 30,
                new Vector2(0f, 60f), new Vector2(1100f, 200f), TextAlignmentOptions.Center);
            CrearBoton(grupoEspera.transform, "Cancelar", new Vector2(0f, -140f), Cancelar);
 
            MostrarFase(Fase.Principal);
        }
 
        private void Update()
        {
            if (panel == null || !panel.activeSelf || GameManager.Instancia == null)
                return;
 
            // El menú siempre queda por encima del resto de la interfaz.
            panel.transform.SetAsLastSibling();
 
            JuegoController controller = GameManager.Instancia.Controller;
 
            // Conexión lista: el menú desaparece y empieza la partida.
            if (controller.PuedeJugar)
            {
                panel.SetActive(false);
                return;
            }
 
            if (fase == Fase.IngresandoIp)
            {
                LeerTeclado();
                textoIp.text = "IP del anfitrión: <b>" + ip + "</b>_";
            }
            else if (fase == Fase.Esperando)
            {
                if (controller.Red != null && controller.Red.Estado == EstadoRed.Error)
                {
                    string error = string.IsNullOrEmpty(controller.UltimoErrorRed)
                        ? "Error de conexión"
                        : controller.UltimoErrorRed;
 
                    string texto = error + "\nPulsa Cancelar para volver.";
 
                    if (textoEstado.text != texto)
                        textoEstado.text = texto;
                }
            }
        }
 
        // ---------- Botones ----------
 
        private void JugarLocal()
        {
            GameManager.Instancia.Controller.IniciarModoLocal();
            panel.SetActive(false);
        }
 
        private void CrearPartida()
        {
            GameManager.Instancia.Controller.IniciarComoAnfitrion(RedService.PuertoPorDefecto);
 
            textoEstado.text =
                "Esperando al oponente...\n" +
                "Tu IP: " + RedService.ObtenerIpLocal() +
                "   Puerto: " + RedService.PuertoPorDefecto + "\n" +
                "(En el mismo equipo usa 127.0.0.1)";
 
            MostrarFase(Fase.Esperando);
        }
 
        private void UnirsePartida()
        {
            MostrarFase(Fase.IngresandoIp);
        }
 
        private void Conectar()
        {
            string destino = ip.Trim();
 
            if (destino.Length == 0)
                return;
 
            GameManager.Instancia.Controller.IniciarComoCliente(
                destino, RedService.PuertoPorDefecto);
 
            textoEstado.text = "Conectando con " + destino + "...";
            MostrarFase(Fase.Esperando);
        }
 
        private void Volver()
        {
            MostrarFase(Fase.Principal);
        }
 
        private void Cancelar()
        {
            GameManager.Instancia.Controller.CancelarRed();
            MostrarFase(Fase.Principal);
        }
 
        // ---------- Teclado para escribir la IP ----------
 
        private void LeerTeclado()
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (ip.Length > 0)
                        ip = ip.Substring(0, ip.Length - 1);
                }
                else if (c == '\n' || c == '\r')
                {
                    Conectar();
                    return;
                }
                else if ((char.IsLetterOrDigit(c) || c == '.' || c == '-') && ip.Length < 40)
                {
                    ip += c;
                }
            }
        }
 
        // ---------- Construcción de la interfaz ----------
 
        private void MostrarFase(Fase nueva)
        {
            fase = nueva;
 
            grupoPrincipal.SetActive(nueva == Fase.Principal);
            grupoIp.SetActive(nueva == Fase.IngresandoIp);
            grupoEspera.SetActive(nueva == Fase.Esperando);
        }
 
        private GameObject CrearGrupo(string nombre)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(panel.transform, false);
 
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
 
            return go;
        }
 
        private TextMeshProUGUI CrearTexto(
            Transform padre, string nombre, float tamano,
            Vector2 posicion, Vector2 dimension, TextAlignmentOptions alineacion)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
 
            TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
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
 
        private void CrearBoton(Transform padre, string etiqueta, Vector2 posicion, UnityAction accion)
        {
            GameObject go = new GameObject(
                "Boton_" + etiqueta, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
 
            go.GetComponent<Image>().color = ColorBoton;
            go.GetComponent<Button>().onClick.AddListener(accion);
 
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = new Vector2(620f, 70f);
 
            TextMeshProUGUI texto = CrearTexto(
                go.transform, "Texto", 28, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
            texto.text = etiqueta;
 
            RectTransform rtTexto = texto.rectTransform;
            rtTexto.anchorMin = Vector2.zero;
            rtTexto.anchorMax = Vector2.one;
            rtTexto.offsetMin = Vector2.zero;
            rtTexto.offsetMax = Vector2.zero;
        }
    }
}