using UnityEngine;
 
namespace ImperiosEnGuerra.Vista
{
    // Aro amarillo bajo una unidad: se muestra solo mientras está seleccionada.
    public class IndicadorSeleccion : MonoBehaviour
    {
        private GameObject aro;
 
        public void Configurar(GameObject aroSeleccion)
        {
            aro = aroSeleccion;
        }
 
        public void Mostrar(bool visible)
        {
            if (aro != null && aro.activeSelf != visible)
                aro.SetActive(visible);
        }
    }
}