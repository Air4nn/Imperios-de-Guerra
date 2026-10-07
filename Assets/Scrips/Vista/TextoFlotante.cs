using TMPro;
using UnityEngine;
 
namespace ImperiosEnGuerra.Vista
{
    // Texto en el mundo 3D (por ejemplo "-25") que sube, mira a la cámara y se desvanece.
    public class TextoFlotante : MonoBehaviour
    {
        private const float Duracion = 1.1f;
        private const float VelocidadSubida = 0.8f;
 
        private TextMeshPro texto;
        private Camera camara;
        private Color colorInicial;
        private float tiempo;
 
        public static void Crear(Vector3 posicion, string contenido, Color color)
        {
            GameObject go = new GameObject("TextoDanio");
            go.transform.position = posicion;
 
            TextoFlotante flotante = go.AddComponent<TextoFlotante>();
            flotante.Iniciar(contenido, color);
        }
 
        private void Iniciar(string contenido, Color color)
        {
            camara = Camera.main;
            colorInicial = color;
 
            texto = gameObject.AddComponent<TextMeshPro>();
            texto.text = contenido;
            texto.fontSize = 4f;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = color;
            texto.rectTransform.sizeDelta = new Vector2(6f, 1.5f);
        }
 
        private void Update()
        {
            tiempo += Time.deltaTime;
 
            transform.position += Vector3.up * VelocidadSubida * Time.deltaTime;
 
            if (camara != null)
                transform.rotation = camara.transform.rotation;
 
            if (texto != null)
            {
                Color c = colorInicial;
                c.a = Mathf.Clamp01(1f - tiempo / Duracion);
                texto.color = c;
            }
 
            if (tiempo >= Duracion)
                Destroy(gameObject);
        }
    }
}