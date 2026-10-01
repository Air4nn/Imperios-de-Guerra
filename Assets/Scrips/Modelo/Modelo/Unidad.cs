namespace ImperiosEnGuerra.Modelo
{
    public class Unidad
    {
        public int Id { get; set; }
        public TipoUnidad Tipo { get; set; }
        public int Vida { get; set; }
        public int Ataque { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int JugadorId { get; set; }

        public Unidad(int id, TipoUnidad tipo, int vida, int ataque, int x, int y, int jugadorId)
        {
            Id = id; Tipo = tipo; Vida = vida; Ataque = ataque;
            X = x; Y = y; JugadorId = jugadorId;
        }

        public bool EstaViva() => Vida > 0;

        public void RecibirDanio(int danio)
        {
            Vida -= danio;
            if (Vida < 0) Vida = 0;
        }
    }
}