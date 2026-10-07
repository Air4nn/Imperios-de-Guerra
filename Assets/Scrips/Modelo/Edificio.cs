namespace ImperiosEnGuerra.Modelo
{
    public class Edificio
    {
        public int Id { get; set; }
        public TipoEdificio Tipo { get; set; }
        public int Vida { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int JugadorId { get; set; }
        public int VidaMaxima { get; set; }

        public Edificio(int id, TipoEdificio tipo, int vida, int x, int y, int jugadorId)
        {
            Id = id; Tipo = tipo; Vida = vida; X = x; Y = y; JugadorId = jugadorId;
            VidaMaxima = vida;
        }

        public bool EstaDestruido() => Vida <= 0;

        public void RecibirDanio(int danio)
        {
            Vida -= danio;
            if (Vida < 0) Vida = 0;
        }
    }
}