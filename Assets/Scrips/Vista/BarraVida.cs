using System;
using UnityEngine;
 
namespace ImperiosEnGuerra.Vista
{
    // Barra de vida en el mundo 3D sobre una unidad o un edificio.
    // Solo lee la vida (mediante una función) y la dibuja: no modifica el Modelo.
    // También detecta cuando la vida baja y muestra el daño recibido (-25, etc.),
    // venga el golpe de este equipo o del oponente en red.
    public class BarraVida : MonoBehaviour
    {
        private Func<int> obtenerVida;
        private int vidaMaxima;
        private bool soloSiHayDanio;
        private float ancho;
        private Color colorDanio;
 
        private GameObject visual;
        private Transform relleno;
        private Renderer rendererRelleno;
 
        private int vidaAnterior;
        private int bandaColor = -1;
        private Camera camara;
 
        public static BarraVida Crear(
            Transform padre, float altura, float ancho, int vidaMaxima,
            Func<int> obtenerVida, bool soloSiHayDanio, Color colorDanio)
        {
            GameObject go = new GameObject("BarraVida");
            go.transform.SetParent(padre, false);
            go.transform.localPosition = new Vector3(0f, altura, 0f);
 
            BarraVida barra = go.AddComponent<BarraVida>();
            barra.Configurar(ancho, vidaMaxima, obtenerVida, soloSiHayDanio, colorDanio);
 
            return barra;
        }
 
        private void Configurar(
            float anchoBarra, int maxima, Func<int> funcionVida,
            bool ocultarSiSana, Color colorDelDanio)
        {
            ancho = anchoBarra;
            vidaMaxima = Mathf.Max(1, maxima);
            obtenerVida = funcionVida;
            soloSiHayDanio = ocultarSiSana;
            colorDanio = colorDelDanio;
            vidaAnterior = funcionVida();
 
            visual = new GameObject("Visual");
            visual.transform.SetParent(transform, false);
 
            GameObject fondo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(fondo.GetComponent<Collider>());
            fondo.transform.SetParent(visual.transform, false);
            fondo.transform.localScale = new Vector3(ancho + 0.05f, 0.13f, 0.02f);
            fondo.GetComponent<Renderer>().sharedMaterial =
                FabricaVisual.MaterialColor(new Color(0.07f, 0.07f, 0.07f));
 
            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(fill.GetComponent<Collider>());
            fill.transform.SetParent(visual.transform, false);
            relleno = fill.transform;
            rendererRelleno = fill.GetComponent<Renderer>();
 
            Actualizar();
        }
 
        private void LateUpdate()
        {
            Actualizar();
 
            // La barra siempre mira a la cámara.
            if (camara == null)
                camara = Camera.main;
 
            if (camara != null)
                transform.rotation = camara.transform.rotation;
        }
 
        // Pública para que la Vista pueda forzar una última lectura antes de ocultar el objeto.
        public void Actualizar()
        {
            if (obtenerVida == null)
                return;
 
            int vida = obtenerVida();
 
            if (vida < vidaAnterior)
            {
                TextoFlotante.Crear(
                    transform.position + new Vector3(0f, 0.25f, 0f),
                    "-" + (vidaAnterior - vida),
                    colorDanio);
            }
 
            vidaAnterior = vida;
 
            float fraccion = Mathf.Clamp01((float)vida / vidaMaxima);
            bool mostrar = vida > 0 && (!soloSiHayDanio || fraccion < 1f);
 
            if (visual.activeSelf != mostrar)
                visual.SetActive(mostrar);
 
            if (!mostrar)
                return;
 
            float anchoRelleno = Mathf.Max(0.001f, ancho * fraccion);
 
            relleno.localScale = new Vector3(anchoRelleno, 0.09f, 0.02f);
            relleno.localPosition = new Vector3(-ancho / 2f + anchoRelleno / 2f, 0f, -0.02f);
 
            int banda = fraccion > 0.6f ? 0 : (fraccion > 0.3f ? 1 : 2);
 
            if (banda != bandaColor)
            {
                bandaColor = banda;
 
                Color color =
                    banda == 0 ? new Color(0.20f, 0.85f, 0.25f) :
                    banda == 1 ? new Color(0.95f, 0.80f, 0.15f) :
                                 new Color(0.90f, 0.15f, 0.12f);
 
                rendererRelleno.sharedMaterial = FabricaVisual.MaterialColor(color);
            }
        }
    }
}