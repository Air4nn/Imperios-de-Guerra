using UnityEngine;
using ImperiosEnGuerra.Controlador;
 
namespace ImperiosEnGuerra.Vista
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instancia { get; private set; }
 
        public JuegoController Controller { get; private set; }
 
        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }
 
            Instancia = this;
 
            Controller = new JuegoController();
 
            // El Controlador crea las unidades iniciales y guarda configuracion.txt.
            Controller.IniciarPartida();
        }
 
        private void Update()
        {
            // Ejecuta en el hilo principal lo que los hilos de fondo dejaron en la cola.
            if (Controller != null)
                Controller.ProcesarPendientes();
        }
 
        private void OnDestroy()
        {
            if (Instancia == this)
            {
                // Finaliza correctamente los hilos de fondo.
                Controller.Detener();
                Instancia = null;
            }
        }
    }
}