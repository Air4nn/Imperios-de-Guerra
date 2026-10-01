namespace ImperiosEnGuerra.Modelo
{
    public class Recurso
    {
        public TipoRecurso Tipo { get; set; }
        public int Cantidad { get; set; }

        public Recurso(TipoRecurso tipo, int cantidad)
        {
            Tipo = tipo;
            Cantidad = cantidad;
        }
    }
}