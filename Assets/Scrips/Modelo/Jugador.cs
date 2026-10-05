using System.Collections.Generic;

namespace ImperiosEnGuerra.Modelo
{
    public class Jugador
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public Dictionary<TipoRecurso, int> Recursos { get; set; }
        public List<Unidad> Unidades { get; set; }
        public List<Edificio> Edificios { get; set; }
        public int EnEntrenamiento { get; set; }
        public int ContadorUnidades { get; set; }
        public int ContadorEdificios { get; set; }

        public Jugador(int id, string nombre)
        {
            Id = id; Nombre = nombre;
            Recursos = new Dictionary<TipoRecurso, int>
            {
                [TipoRecurso.Oro] = 500,
                [TipoRecurso.Madera] = 500,
                [TipoRecurso.Comida] = 500
            };
            Unidades = new List<Unidad>();
            Edificios = new List<Edificio>();
        }

        public void AgregarRecurso(TipoRecurso tipo, int cantidad)
        {
            if (!Recursos.ContainsKey(tipo)) Recursos[tipo] = 0;
            Recursos[tipo] += cantidad;
        }

        public bool TieneRecursos(TipoRecurso tipo, int cantidad) =>
            Recursos.ContainsKey(tipo) && Recursos[tipo] >= cantidad;

        public bool GastarRecurso(TipoRecurso tipo, int cantidad)
        {
            if (!TieneRecursos(tipo, cantidad)) return false;
            Recursos[tipo] -= cantidad;
            return true;
        }

            public void AgregarUnidad(Unidad unidad)
        {
            Unidades.Add(unidad);
        }

        public void AgregarEdificio(Edificio edificio)
        {
            Edificios.Add(edificio);
        }

        public int ObtenerRecurso(TipoRecurso tipo)
        {
            if (Recursos.ContainsKey(tipo))
                return Recursos[tipo];

            return 0;
        }
    }
}