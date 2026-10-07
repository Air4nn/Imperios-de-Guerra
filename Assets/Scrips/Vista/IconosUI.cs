using System.Collections.Generic;
using UnityEngine;
 
namespace ImperiosEnGuerra.Vista
{
    // Carga los íconos PNG de Assets/Resources/Iconos como Sprites.
    // Si un ícono no existe devuelve null y la interfaz funciona igual, sin imagen.
    public static class IconosUI
    {
        private static readonly Dictionary<string, Sprite> cache =
            new Dictionary<string, Sprite>();
 
        public static Sprite Cargar(string nombre)
        {
            Sprite sprite;
 
            if (cache.TryGetValue(nombre, out sprite) && sprite != null)
                return sprite;
 
            Texture2D textura = Resources.Load<Texture2D>("Iconos/" + nombre);
 
            if (textura == null)
                return null;
 
            sprite = Sprite.Create(
                textura,
                new Rect(0f, 0f, textura.width, textura.height),
                new Vector2(0.5f, 0.5f),
                100f);
 
            cache[nombre] = sprite;
            return sprite;
        }
    }
}