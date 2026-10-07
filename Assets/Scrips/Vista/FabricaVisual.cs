using System.Collections.Generic;
using UnityEngine;
using ImperiosEnGuerra.Modelo;
 
namespace ImperiosEnGuerra.Vista
{
    // Construye por código todo el aspecto del juego (sin descargar paquetes):
    // soldados, Centro Urbano, casas, terreno con textura y decoración de recursos.
    // Es solo Vista: no conoce las reglas del juego, solo dibuja.
    public static class FabricaVisual
    {
        private const int Tam = 64;
 
        private static Material materialBase;
 
        private static readonly Dictionary<string, Material> materiales =
            new Dictionary<string, Material>();
 
        // ---------- Materiales ----------
 
        // Usa el material por defecto del pipeline actual (URP) como plantilla.
        private static Material MaterialBase()
        {
            if (materialBase == null)
            {
                GameObject temporal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                materialBase = temporal.GetComponent<Renderer>().sharedMaterial;
                Object.Destroy(temporal);
            }
 
            return materialBase;
        }
 
        public static Material MaterialColor(Color color, bool metalico = false)
        {
            string clave = (metalico ? "m" : "c") + ColorUtility.ToHtmlStringRGBA(color);
 
            Material material;
 
            if (materiales.TryGetValue(clave, out material) && material != null)
                return material;
 
            material = new Material(MaterialBase());
            material.color = color;
 
            if (metalico)
            {
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.85f);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.75f);
            }
 
            materiales[clave] = material;
            return material;
        }
 
        private static Material MaterialTextura(string clave, System.Func<Texture2D> crear)
        {
            Material material;
 
            if (materiales.TryGetValue(clave, out material) && material != null)
                return material;
 
            material = new Material(MaterialBase());
            material.color = Color.white;
            material.mainTexture = crear();
 
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.05f);
 
            materiales[clave] = material;
            return material;
        }
 
        private static Color Oscuro(Color color, float factor)
        {
            return new Color(color.r * factor, color.g * factor, color.b * factor, 1f);
        }
 
        // ---------- Terreno con textura ----------
 
        public static Material MaterialTerreno(int x, int y)
        {
            if ((x + y) % 2 == 0)
                return MaterialTextura("pastoA",
                    () => TexturaPasto(11, new Color(0.34f, 0.58f, 0.30f)));
 
            return MaterialTextura("pastoB",
                () => TexturaPasto(23, new Color(0.30f, 0.52f, 0.26f)));
        }
 
        public static Material MaterialRecurso(TipoRecurso tipo)
        {
            switch (tipo)
            {
                case TipoRecurso.Oro:
                    return MaterialTextura("terrenoOro", TexturaOro);
                case TipoRecurso.Madera:
                    return MaterialTextura("terrenoMadera", TexturaBosque);
                default:
                    return MaterialTextura("terrenoComida", TexturaArbustos);
            }
        }
 
        private static Texture2D NuevaTextura(Color[] pixeles)
        {
            Texture2D textura = new Texture2D(Tam, Tam, TextureFormat.RGBA32, true);
            textura.SetPixels(pixeles);
            textura.wrapMode = TextureWrapMode.Clamp;
            textura.filterMode = FilterMode.Bilinear;
            textura.Apply();
            return textura;
        }
 
        private static Color[] Rellenar(System.Random azar, Color baseColor, float ruido)
        {
            Color[] pixeles = new Color[Tam * Tam];
 
            for (int i = 0; i < pixeles.Length; i++)
            {
                float v = 1f + ((float)azar.NextDouble() - 0.5f) * 2f * ruido;
                pixeles[i] = new Color(baseColor.r * v, baseColor.g * v, baseColor.b * v, 1f);
            }
 
            return pixeles;
        }
 
        private static void Circulo(Color[] pixeles, int cx, int cy, int radio, Color color)
        {
            for (int dy = -radio; dy <= radio; dy++)
            {
                for (int dx = -radio; dx <= radio; dx++)
                {
                    if (dx * dx + dy * dy > radio * radio) continue;
 
                    int ix = cx + dx;
                    int iy = cy + dy;
 
                    if (ix < 0 || iy < 0 || ix >= Tam || iy >= Tam) continue;
 
                    pixeles[iy * Tam + ix] = color;
                }
            }
        }
 
        private static Texture2D TexturaPasto(int semilla, Color baseColor)
        {
            System.Random azar = new System.Random(semilla);
            Color[] pixeles = Rellenar(azar, baseColor, 0.06f);
 
            Color brizna = new Color(baseColor.r * 1.2f, baseColor.g * 1.2f, baseColor.b * 1.1f, 1f);
 
            for (int i = 0; i < 26; i++)
            {
                int x = azar.Next(2, Tam - 2);
                int y = azar.Next(2, Tam - 6);
 
                for (int k = 0; k < 4; k++)
                    pixeles[(y + k) * Tam + x] = brizna;
            }
 
            return NuevaTextura(pixeles);
        }
 
        private static Texture2D TexturaOro()
        {
            System.Random azar = new System.Random(5);
            Color[] pixeles = Rellenar(azar, new Color(0.43f, 0.37f, 0.23f), 0.08f);
 
            for (int i = 0; i < 8; i++)
            {
                int x = azar.Next(6, Tam - 6);
                int y = azar.Next(6, Tam - 6);
                int r = azar.Next(3, 6);
 
                Circulo(pixeles, x, y, r, new Color(0.95f, 0.76f, 0.12f));
                Circulo(pixeles, x - 1, y + 1, 1, new Color(1f, 0.95f, 0.6f));
            }
 
            return NuevaTextura(pixeles);
        }
 
        private static Texture2D TexturaBosque()
        {
            System.Random azar = new System.Random(8);
            Color[] pixeles = Rellenar(azar, new Color(0.27f, 0.44f, 0.21f), 0.07f);
 
            for (int i = 0; i < 9; i++)
            {
                int x = azar.Next(6, Tam - 6);
                int y = azar.Next(6, Tam - 6);
                int r = azar.Next(6, 10);
 
                Circulo(pixeles, x, y, r, new Color(0.09f, 0.33f, 0.12f));
                Circulo(pixeles, x - 1, y + 1, r - 3, new Color(0.15f, 0.44f, 0.17f));
            }
 
            return NuevaTextura(pixeles);
        }
 
        private static Texture2D TexturaArbustos()
        {
            System.Random azar = new System.Random(13);
            Color[] pixeles = Rellenar(azar, new Color(0.36f, 0.57f, 0.31f), 0.06f);
 
            for (int i = 0; i < 6; i++)
            {
                int x = azar.Next(8, Tam - 8);
                int y = azar.Next(8, Tam - 8);
                int r = azar.Next(6, 9);
 
                Circulo(pixeles, x, y, r, new Color(0.18f, 0.47f, 0.17f));
 
                for (int b = 0; b < 3; b++)
                {
                    int bx = x + azar.Next(-r + 2, r - 1);
                    int by = y + azar.Next(-r + 2, r - 1);
                    Circulo(pixeles, bx, by, 1, new Color(0.85f, 0.10f, 0.10f));
                }
            }
 
            return NuevaTextura(pixeles);
        }
 
        // ---------- Piezas ----------
 
        private static GameObject Parte(
            Transform padre, PrimitiveType tipo, Vector3 posicion, Vector3 escala,
            Material material, Vector3 rotacion = default(Vector3))
        {
            GameObject pieza = GameObject.CreatePrimitive(tipo);
 
            // Las piezas solo se dibujan; el clic lo recibe el collider único de la raíz.
            Object.Destroy(pieza.GetComponent<Collider>());
 
            pieza.transform.SetParent(padre, false);
            pieza.transform.localPosition = posicion;
            pieza.transform.localEulerAngles = rotacion;
            pieza.transform.localScale = escala;
            pieza.GetComponent<Renderer>().sharedMaterial = material;
 
            return pieza;
        }
 
        private static void AgregarCollider(GameObject raiz, Vector3 centro, Vector3 tamano)
        {
            BoxCollider caja = raiz.AddComponent<BoxCollider>();
            caja.center = centro;
            caja.size = tamano;
        }
 
        // ---------- Unidades ----------
 
        public static GameObject CrearSoldado(Color equipo)
        {
            GameObject raiz = new GameObject("Soldado");
            Transform t = raiz.transform;
 
            Material mEquipo = MaterialColor(equipo);
            Material mClaro = MaterialColor(Color.Lerp(equipo, Color.white, 0.3f));
            Material mPiel = MaterialColor(new Color(0.95f, 0.78f, 0.62f));
            Material mAcero = MaterialColor(new Color(0.62f, 0.65f, 0.70f), true);
            Material mHoja = MaterialColor(new Color(0.88f, 0.90f, 0.94f), true);
            Material mCuero = MaterialColor(new Color(0.30f, 0.20f, 0.12f));
 
            // Piernas y cuerpo
            Parte(t, PrimitiveType.Cube, new Vector3(-0.08f, 0.07f, 0f), new Vector3(0.11f, 0.14f, 0.13f), mCuero);
            Parte(t, PrimitiveType.Cube, new Vector3(0.08f, 0.07f, 0f), new Vector3(0.11f, 0.14f, 0.13f), mCuero);
            Parte(t, PrimitiveType.Capsule, new Vector3(0f, 0.38f, 0f), new Vector3(0.34f, 0.24f, 0.34f), mEquipo);
 
            // Cabeza, casco y cimera
            Parte(t, PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0f), new Vector3(0.24f, 0.24f, 0.24f), mPiel);
            Parte(t, PrimitiveType.Sphere, new Vector3(0f, 0.77f, 0f), new Vector3(0.27f, 0.16f, 0.27f), mAcero);
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.89f, 0f), new Vector3(0.03f, 0.10f, 0.18f), mEquipo);
 
            // Escudo (izquierda) y espada (derecha)
            Parte(t, PrimitiveType.Cube, new Vector3(-0.23f, 0.40f, 0f), new Vector3(0.05f, 0.30f, 0.24f), mClaro);
            Parte(t, PrimitiveType.Sphere, new Vector3(-0.26f, 0.40f, 0f), new Vector3(0.08f, 0.08f, 0.08f), mAcero);
            Parte(t, PrimitiveType.Cube, new Vector3(0.24f, 0.50f, 0f), new Vector3(0.04f, 0.50f, 0.06f), mHoja,
                new Vector3(0f, 0f, -12f));
            Parte(t, PrimitiveType.Cube, new Vector3(0.22f, 0.27f, 0f), new Vector3(0.15f, 0.04f, 0.08f), mCuero);
 
            // Aro amarillo bajo los pies: solo se ve cuando la unidad está seleccionada.
            GameObject aro = Parte(t, PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0f),
                new Vector3(0.80f, 0.01f, 0.80f), MaterialColor(new Color(1f, 0.92f, 0.2f)));
            aro.SetActive(false);
 
            raiz.AddComponent<IndicadorSeleccion>().Configurar(aro);
 
            AgregarCollider(raiz, new Vector3(0f, 0.4f, 0f), new Vector3(0.5f, 0.8f, 0.5f));
 
            return raiz;
        }
 
        // ---------- Edificios ----------
 
        public static GameObject CrearCentroUrbano(Color equipo)
        {
            GameObject raiz = new GameObject("CentroUrbano");
            Transform t = raiz.transform;
 
            Material mPiedra = MaterialColor(new Color(0.62f, 0.60f, 0.57f));
            Material mPiedraOsc = MaterialColor(new Color(0.50f, 0.48f, 0.46f));
            Material mEquipo = MaterialColor(equipo);
            Material mTecho = MaterialColor(Oscuro(equipo, 0.6f));
            Material mMadera = MaterialColor(new Color(0.35f, 0.22f, 0.12f));
 
            // Base de piedra y torreón del color del equipo
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.20f, 0f), new Vector3(0.92f, 0.40f, 0.92f), mPiedra);
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.65f, 0f), new Vector3(0.62f, 0.50f, 0.62f), mEquipo);
 
            // Techo a dos aguas: un cubo girado 45 grados forma el prisma del tejado.
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.90f, 0f), new Vector3(0.50f, 0.50f, 0.80f), mTecho,
                new Vector3(0f, 0f, 45f));
 
            // Cuatro torres en las esquinas con su remate
            float[] esquinas = { -0.40f, 0.40f };
 
            foreach (float ex in esquinas)
            {
                foreach (float ez in esquinas)
                {
                    Parte(t, PrimitiveType.Cylinder, new Vector3(ex, 0.75f, ez),
                        new Vector3(0.20f, 0.35f, 0.20f), mPiedraOsc);
                    Parte(t, PrimitiveType.Sphere, new Vector3(ex, 1.12f, ez),
                        new Vector3(0.26f, 0.26f, 0.26f), mTecho);
                }
            }
 
            // Puerta al frente (hacia la cámara) y bandera
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.17f, -0.47f), new Vector3(0.20f, 0.26f, 0.02f), mMadera);
            Parte(t, PrimitiveType.Cylinder, new Vector3(0f, 1.50f, 0f), new Vector3(0.03f, 0.25f, 0.03f), mMadera);
            Parte(t, PrimitiveType.Cube, new Vector3(0.16f, 1.62f, 0f), new Vector3(0.30f, 0.18f, 0.02f), mEquipo);
 
            AgregarCollider(raiz, new Vector3(0f, 0.6f, 0f), new Vector3(0.9f, 1.2f, 0.9f));
 
            return raiz;
        }
 
        public static GameObject CrearCasa(Color equipo)
        {
            GameObject raiz = new GameObject("Casa");
            Transform t = raiz.transform;
 
            Material mPared = MaterialColor(Color.Lerp(equipo, Color.white, 0.55f));
            Material mTecho = MaterialColor(new Color(0.50f, 0.25f, 0.15f));
            Material mMadera = MaterialColor(new Color(0.30f, 0.19f, 0.10f));
            Material mPiedra = MaterialColor(new Color(0.55f, 0.55f, 0.58f));
 
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.20f, 0f), new Vector3(0.58f, 0.40f, 0.58f), mPared);
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.40f, 0f), new Vector3(0.46f, 0.46f, 0.70f), mTecho,
                new Vector3(0f, 0f, 45f));
            Parte(t, PrimitiveType.Cube, new Vector3(0f, 0.11f, -0.295f), new Vector3(0.14f, 0.22f, 0.02f), mMadera);
            Parte(t, PrimitiveType.Cube, new Vector3(0.15f, 0.62f, 0.12f), new Vector3(0.08f, 0.20f, 0.08f), mPiedra);
 
            AgregarCollider(raiz, new Vector3(0f, 0.35f, 0f), new Vector3(0.7f, 0.7f, 0.7f));
 
            return raiz;
        }
 
        // ---------- Decoración de las celdas con recursos ----------
 
        public static GameObject CrearDecoracionRecurso(TipoRecurso tipo)
        {
            GameObject raiz = new GameObject("Recurso_" + tipo);
            Transform t = raiz.transform;
 
            switch (tipo)
            {
                case TipoRecurso.Oro:
                {
                    Material oro = MaterialColor(new Color(1f, 0.80f, 0.10f), true);
 
                    Parte(t, PrimitiveType.Cube, new Vector3(-0.20f, 0.12f, -0.10f), Vector3.one * 0.16f, oro, new Vector3(35f, 25f, 15f));
                    Parte(t, PrimitiveType.Cube, new Vector3(0.15f, 0.10f, 0.18f), Vector3.one * 0.14f, oro, new Vector3(20f, 60f, 35f));
                    Parte(t, PrimitiveType.Cube, new Vector3(0.00f, 0.15f, 0.00f), Vector3.one * 0.22f, oro, new Vector3(40f, 15f, 25f));
                    Parte(t, PrimitiveType.Cube, new Vector3(0.22f, 0.08f, -0.20f), Vector3.one * 0.12f, oro, new Vector3(10f, 45f, 30f));
                    break;
                }
 
                case TipoRecurso.Madera:
                {
                    Material tronco = MaterialColor(new Color(0.35f, 0.22f, 0.12f));
                    Material copa = MaterialColor(new Color(0.12f, 0.45f, 0.15f));
                    Material copaClara = MaterialColor(new Color(0.20f, 0.55f, 0.20f));
 
                    Vector3[] arboles =
                    {
                        new Vector3(-0.22f, 0f, -0.15f),
                        new Vector3(0.20f, 0f, 0.10f),
                        new Vector3(-0.05f, 0f, 0.26f)
                    };
 
                    foreach (Vector3 p in arboles)
                    {
                        Parte(t, PrimitiveType.Cylinder, p + new Vector3(0f, 0.18f, 0f),
                            new Vector3(0.08f, 0.18f, 0.08f), tronco);
                        Parte(t, PrimitiveType.Sphere, p + new Vector3(0f, 0.50f, 0f),
                            new Vector3(0.36f, 0.34f, 0.36f), copa);
                        Parte(t, PrimitiveType.Sphere, p + new Vector3(0f, 0.68f, 0f),
                            new Vector3(0.26f, 0.24f, 0.26f), copaClara);
                    }
 
                    break;
                }
 
                default:
                {
                    Material arbusto = MaterialColor(new Color(0.16f, 0.50f, 0.18f));
                    Material baya = MaterialColor(new Color(0.85f, 0.10f, 0.10f));
 
                    Vector3[] arbustos =
                    {
                        new Vector3(-0.20f, 0.14f, -0.10f),
                        new Vector3(0.18f, 0.13f, 0.15f),
                        new Vector3(0.00f, 0.14f, 0.26f)
                    };
 
                    foreach (Vector3 p in arbustos)
                    {
                        Parte(t, PrimitiveType.Sphere, p, Vector3.one * 0.30f, arbusto);
                        Parte(t, PrimitiveType.Sphere, p + new Vector3(0.08f, 0.10f, -0.08f), Vector3.one * 0.07f, baya);
                        Parte(t, PrimitiveType.Sphere, p + new Vector3(-0.07f, 0.08f, -0.10f), Vector3.one * 0.07f, baya);
                        Parte(t, PrimitiveType.Sphere, p + new Vector3(0.00f, 0.14f, 0.06f), Vector3.one * 0.07f, baya);
                    }
 
                    break;
                }
            }
 
            return raiz;
        }
    }
}