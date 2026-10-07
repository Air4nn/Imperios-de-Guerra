using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ImperiosEnGuerra.Modelo;
 
namespace ImperiosEnGuerra.Vista
{
    // Barra superior derecha: recursos, unidades y edificios de cada jugador con íconos.
    // Se agrega al objeto Canvas; construye su propia interfaz al iniciar.
    public class BarraRecursos : MonoBehaviour
    {
        private const int Items = 5;
 
        private static readonly string[] NombresIconos = { "oro", "madera", "comida", "soldado", "casa" };
 
        private readonly TextMeshProUGUI[,] valores = new TextMeshProUGUI[2, Items];
        private readonly int[,] ultimos = new int[2, Items];
 
        private void Start()
        {
            GameObject barra = new GameObject(
                "BarraRecursos_UI", typeof(RectTransform), typeof(Image));
            barra.transform.SetParent(transform, false);
            barra.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.88f);
 
            RectTransform rt = barra.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -12f);
            rt.sizeDelta = new Vector2(1180f, 64f);
 
            CrearFila(barra.transform, 0, new Color(0.30f, 0.50f, 1f));
            CrearFila(barra.transform, 1, new Color(1f, 0.30f, 0.30f));
        }
 
        private void CrearFila(Transform padre, int jugador, Color colorEquipo)
        {
            float x0 = jugador == 0 ? 8f : 598f;
 
            // Marca de color del equipo
            GameObject marca = new GameObject(
                "Equipo" + (jugador + 1), typeof(RectTransform), typeof(Image));
            marca.transform.SetParent(padre, false);
            marca.GetComponent<Image>().color = colorEquipo;
            marca.GetComponent<Image>().raycastTarget = false;
 
            RectTransform rtMarca = marca.GetComponent<RectTransform>();
            rtMarca.anchorMin = new Vector2(0f, 0.5f);
            rtMarca.anchorMax = new Vector2(0f, 0.5f);
            rtMarca.pivot = new Vector2(0f, 0.5f);
            rtMarca.anchoredPosition = new Vector2(x0, 0f);
            rtMarca.sizeDelta = new Vector2(10f, 44f);
 
            float x = x0 + 20f;
 
            for (int i = 0; i < Items; i++)
            {
                CrearIcono(padre, IconosUI.Cargar(NombresIconos[i]), new Vector2(x, 0f));
                valores[jugador, i] = CrearValor(padre, new Vector2(x + 38f, 0f));
                x += 104f;
            }
        }
 
        private void CrearIcono(Transform padre, Sprite sprite, Vector2 posicion)
        {
            GameObject go = new GameObject("Icono", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
 
            Image imagen = go.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.enabled = sprite != null;
            imagen.preserveAspect = true;
            imagen.raycastTarget = false;
 
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = new Vector2(34f, 34f);
        }
 
        private TextMeshProUGUI CrearValor(Transform padre, Vector2 posicion)
        {
            GameObject go = new GameObject("Valor", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
 
            TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
            texto.fontSize = 26;
            texto.color = Color.white;
            texto.alignment = TextAlignmentOptions.MidlineLeft;
            texto.raycastTarget = false;
            texto.text = "0";
 
            RectTransform rt = texto.rectTransform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = new Vector2(60f, 40f);
 
            return texto;
        }
 
        private void Update()
        {
            if (GameManager.Instancia == null)
                return;
 
            Partida partida = GameManager.Instancia.Controller.Partida;
 
            Actualizar(0, partida.Jugador1);
            Actualizar(1, partida.Jugador2);
        }
 
        private void Actualizar(int fila, Jugador jugador)
        {
            int[] datos =
            {
                jugador.ObtenerRecurso(TipoRecurso.Oro),
                jugador.ObtenerRecurso(TipoRecurso.Madera),
                jugador.ObtenerRecurso(TipoRecurso.Comida),
                ContarUnidades(jugador),
                ContarEdificios(jugador)
            };
 
            for (int i = 0; i < Items; i++)
            {
                if (valores[fila, i] == null || datos[i] == ultimos[fila, i])
                    continue;
 
                ultimos[fila, i] = datos[i];
                valores[fila, i].text = datos[i].ToString();
            }
        }
 
        private int ContarUnidades(Jugador jugador)
        {
            int total = 0;
 
            foreach (Unidad unidad in jugador.Unidades)
            {
                if (unidad.EstaViva())
                    total++;
            }
 
            return total;
        }
 
        private int ContarEdificios(Jugador jugador)
        {
            int total = 0;
 
            foreach (Edificio edificio in jugador.Edificios)
            {
                if (!edificio.EstaDestruido())
                    total++;
            }
 
            return total;
        }
    }
}