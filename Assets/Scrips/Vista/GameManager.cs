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
 
            // Necesario para que cada instancia siga recibiendo mensajes de red
            // aunque su ventana no tenga el foco (dos instancias en un mismo equipo).
            Application.runInBackground = true;
 
            Controller = new JuegoController();
 
            // El Controlador crea las unidades iniciales y guarda configuracion.txt.
            Controller.IniciarPartida();
        }
 
        private void Update()
        {
            // Ejecuta en el hilo principal lo que los hilos de fondo dejaron en la cola
            // y aplica los mensajes recibidos por red.
            if (Controller != null)
                Controller.ProcesarPendientes();
        }
 
        private void OnDestroy()
        {
            if (Instancia == this)
            {
                // Finaliza correctamente los hilos de fondo y cierra los sockets.
                Controller.Detener();
                Instancia = null;
            }
        }
    }
}