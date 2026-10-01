using UnityEngine;

namespace ImperiosEnGuerra.Vista
{
    public class EdificioView : MonoBehaviour
    {
        public int edificioId;

        public void ActualizarPosicion(int x, int y)
        {
            transform.position =
                new Vector3(
                    x,
                    1f,
                    y
                );
        }

        public void MostrarEdificio()
        {
            gameObject.SetActive(true);
        }

        public void OcultarEdificio()
        {
            gameObject.SetActive(false);
        }
    }
}