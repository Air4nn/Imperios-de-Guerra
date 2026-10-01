using UnityEngine;

namespace ImperiosEnGuerra.Vista
{
    public class UnidadView : MonoBehaviour
    {
        public int unidadId;

        public void ActualizarPosicion(int x, int y)
        {
            transform.position =
                new Vector3(
                    x,
                    0.8f,
                    y
                );
        }

        public void MostrarUnidad()
        {
            gameObject.SetActive(true);
        }

        public void OcultarUnidad()
        {
            gameObject.SetActive(false);
        }
    }
}