namespace ImperiosEnGuerra.Modelo
{
    public class Celda
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Ocupada { get; set; }
        public TipoRecurso? Recurso { get; set; }
        public int CantidadRecurso { get; set; }

        public Celda(int x, int y)
        {
            X = x; Y = y; Ocupada = false;
            Recurso = null; CantidadRecurso = 0;
        }
    }
}