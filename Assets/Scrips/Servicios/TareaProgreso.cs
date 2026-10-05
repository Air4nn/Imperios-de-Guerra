using System.Threading;
 
namespace ImperiosEnGuerra.Servicios
{
    // Estado de una tarea de fondo (construcción, entrenamiento, recolección).
    // El hilo de fondo actualiza Transcurrido; la vista lo lee en el hilo principal.
    public class TareaProgreso
    {
        private double transcurrido;
 
        public int Id { get; set; }
        public int JugadorId { get; set; }
        public string Descripcion { get; set; }
        public double Total { get; set; }
 
        public double Transcurrido
        {
            get { return Volatile.Read(ref transcurrido); }
            set { Volatile.Write(ref transcurrido, value); }
        }
    }
}