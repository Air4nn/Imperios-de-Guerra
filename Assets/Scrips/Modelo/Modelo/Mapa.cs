namespace ImperiosEnGuerra.Modelo
{
    public class Mapa
    {
        public int Filas { get; set; }
        public int Columnas { get; set; }
        public Celda[,] Celdas { get; set; }

        public Mapa(int filas, int columnas)
        {
            Filas = filas; Columnas = columnas;
            Celdas = new Celda[filas, columnas];

            for (int x = 0; x < filas; x++)
                for (int y = 0; y < columnas; y++)
                    Celdas[x, y] = new Celda(x, y);
        }

        public bool EstaDentro(int x, int y)
        {
            return x >= 0 && x < Filas && y >= 0 && y < Columnas;
        }

        public bool EstaLibre(int x, int y)
        {
            return EstaDentro(x, y) && !Celdas[x, y].Ocupada;
        }

        public bool Ocupar(int x, int y)
        {
            if (!EstaLibre(x, y)) return false;
            Celdas[x, y].Ocupada = true;
            return true;
        }

        public void Liberar(int x, int y)
        {
            if (EstaDentro(x, y)) Celdas[x, y].Ocupada = false;
        }

        public void AgregarRecurso(int x, int y, TipoRecurso tipo, int cantidad)
        {
            if (!EstaDentro(x, y)) return;
            Celdas[x, y].Recurso = tipo;
            Celdas[x, y].CantidadRecurso = cantidad;
        }
    }
}