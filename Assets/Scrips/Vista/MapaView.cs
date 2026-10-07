using UnityEngine;
using ImperiosEnGuerra.Modelo;
using System.Collections.Generic;
 
namespace ImperiosEnGuerra.Vista
{
    public class MapaView : MonoBehaviour
    {
        // Altura de la cara superior de las celdas: ahí se apoyan unidades y edificios.
        private const float AlturaSuelo = 0.075f;
 
        public float tamanoCelda = 1f;
 
        private class RecursoVisual
        {
            public Celda Celda;
            public GameObject Objeto;
        }
 
        private Dictionary<int, GameObject> unidadesVisuales =
            new Dictionary<int, GameObject>();
 
        private Dictionary<int, GameObject> edificiosVisuales =
            new Dictionary<int, GameObject>();
 
        private readonly List<RecursoVisual> recursosVisuales = new List<RecursoVisual>();
 
        private InteraccionMapa interaccion;
 
        private void Start()
        {
            interaccion = FindAnyObjectByType<InteraccionMapa>();
            DibujarMapa();
        }
 
        private void Update()
        {
            ActualizarUnidades();
            ActualizarEdificios();
            ActualizarRecursos();
        }
 
        private Color ColorDeJugador(int jugadorId)
        {
            return jugadorId == 1
                ? new Color(0.2f, 0.4f, 1f)
                : new Color(1f, 0.2f, 0.2f);
        }
 
        private void DibujarMapa()
        {
            Partida partida =
                GameManager.Instancia.Controller.Partida;
 
            for (int x = 0; x < partida.Mapa.Filas; x++)
            {
                for (int y = 0; y < partida.Mapa.Columnas; y++)
                {
                    GameObject celda =
                        GameObject.CreatePrimitive(
                            PrimitiveType.Cube
                        );
 
                    celda.name = "Celda_" + x + "_" + y;
 
                    celda.transform.position =
                        new Vector3(
                            x * tamanoCelda,
                            0,
                            y * tamanoCelda
                        );
 
                    celda.transform.localScale =
                        new Vector3(
                            tamanoCelda,
                            0.15f,
                            tamanoCelda
                        );
 
                    Renderer r = celda.GetComponent<Renderer>();
                    Celda datos = partida.Mapa.Celdas[x, y];
 
                    if (datos.Recurso != null)
                    {
                        // Celda con textura propia del recurso y decoración en 3D encima.
                        r.sharedMaterial = FabricaVisual.MaterialRecurso(datos.Recurso.Value);
 
                        GameObject decoracion =
                            FabricaVisual.CrearDecoracionRecurso(datos.Recurso.Value);
 
                        decoracion.name = "Recurso_" + x + "_" + y;
 
                        decoracion.transform.position =
                            new Vector3(
                                x * tamanoCelda,
                                AlturaSuelo,
                                y * tamanoCelda
                            );
 
                        recursosVisuales.Add(new RecursoVisual
                        {
                            Celda = datos,
                            Objeto = decoracion
                        });
                    }
                    else
                    {
                        r.sharedMaterial = FabricaVisual.MaterialTerreno(x, y);
                    }
                }
            }
 
            DibujarUnidades(partida);
            DibujarEdificios(partida);
        }
 
        private void DibujarUnidades(Partida partida)
        {
            foreach (Jugador jugador in
                     new Jugador[]
                     {
                         partida.Jugador1,
                         partida.Jugador2
                     })
            {
                foreach (Unidad unidad in jugador.Unidades)
                {
                    CrearVisualUnidad(unidad);
                }
            }
        }
 
        private void DibujarEdificios(Partida partida)
        {
            foreach (Jugador jugador in
                     new Jugador[]
                     {
                         partida.Jugador1,
                         partida.Jugador2
                     })
            {
                foreach (Edificio edificio in jugador.Edificios)
                {
                    CrearVisualEdificio(edificio);
                }
            }
        }
 
        private void CrearVisualUnidad(Unidad unidad)
        {
            if (unidadesVisuales.ContainsKey(unidad.Id))
                return;
 
            GameObject objeto =
                FabricaVisual.CrearSoldado(ColorDeJugador(unidad.JugadorId));
 
            objeto.name = "Unidad_" + unidad.Id;
 
            objeto.transform.position =
                new Vector3(
                    unidad.X,
                    AlturaSuelo,
                    unidad.Y
                );
 
            // Al aparecer, mira hacia el campamento enemigo.
            float sentido = unidad.JugadorId == 1 ? 1f : -1f;
            objeto.transform.rotation =
                Quaternion.LookRotation(new Vector3(sentido, 0f, sentido));
 
            // Barra de vida: solo se ve cuando la unidad está herida.
            BarraVida.Crear(
                objeto.transform, 1.12f, 0.7f,
                unidad.VidaMaxima, () => unidad.Vida,
                true, new Color(1f, 0.35f, 0.25f));
 
            unidadesVisuales.Add(
                unidad.Id,
                objeto
            );
        }
 
        private void CrearVisualEdificio(Edificio edificio)
        {
            if (edificiosVisuales.ContainsKey(edificio.Id))
                return;
 
            Color color = ColorDeJugador(edificio.JugadorId);
            bool esCasa = edificio.Tipo == TipoEdificio.Casa;
 
            GameObject objeto =
                esCasa
                    ? FabricaVisual.CrearCasa(color)
                    : FabricaVisual.CrearCentroUrbano(color);
 
            objeto.name =
                "Edificio_" + edificio.Id;
 
            objeto.transform.position =
                new Vector3(
                    edificio.X,
                    AlturaSuelo,
                    edificio.Y
                );
 
            // Barra de vida siempre visible sobre los edificios.
            BarraVida.Crear(
                objeto.transform,
                esCasa ? 0.98f : 1.95f,
                esCasa ? 0.7f : 1.0f,
                edificio.VidaMaxima, () => edificio.Vida,
                false, new Color(1f, 0.70f, 0.20f));
 
            edificiosVisuales.Add(
                edificio.Id,
                objeto
            );
        }
 
        private void ActualizarUnidades()
        {
            if (GameManager.Instancia == null)
                return;
 
            Partida partida =
                GameManager.Instancia.Controller.Partida;
 
            ActualizarJugador(partida.Jugador1);
            ActualizarJugador(partida.Jugador2);
        }
 
        private void ActualizarJugador(Jugador jugador)
        {
            foreach (Unidad unidad in jugador.Unidades)
            {
                if (!unidadesVisuales.ContainsKey(unidad.Id))
                {
                    CrearVisualUnidad(unidad);
                }
 
                GameObject objeto =
                    unidadesVisuales[unidad.Id];
 
                // El hilo de movimiento avanza una celda cada 0,4 s;
                // la vista se desliza suavemente hacia esa celda y se orienta hacia ella.
                Vector3 destino =
                    new Vector3(
                        unidad.X,
                        AlturaSuelo,
                        unidad.Y
                    );
 
                Vector3 actual = objeto.transform.position;
                Vector3 direccion = destino - actual;
                direccion.y = 0f;
 
                if (direccion.sqrMagnitude > 0.0004f)
                {
                    objeto.transform.position =
                        Vector3.MoveTowards(
                            actual,
                            destino,
                            5f * Time.deltaTime
                        );
 
                    objeto.transform.rotation =
                        Quaternion.Slerp(
                            objeto.transform.rotation,
                            Quaternion.LookRotation(direccion),
                            10f * Time.deltaTime
                        );
                }
 
                // La unidad seleccionada muestra un aro amarillo bajo los pies.
                bool seleccionada =
                    interaccion != null &&
                    interaccion.Seleccionada == unidad;
 
                IndicadorSeleccion indicador =
                    objeto.GetComponent<IndicadorSeleccion>();
 
                if (indicador != null)
                    indicador.Mostrar(seleccionada);
 
                if (!unidad.EstaViva() && objeto.activeSelf)
                {
                    OcultarConUltimoGolpe(objeto);
                }
            }
        }
 
        // Crea los edificios nuevos (casas) y oculta los destruidos.
        private void ActualizarEdificios()
        {
            if (GameManager.Instancia == null)
                return;
 
            Partida partida =
                GameManager.Instancia.Controller.Partida;
 
            ActualizarEdificiosDe(partida.Jugador1);
            ActualizarEdificiosDe(partida.Jugador2);
        }
 
        private void ActualizarEdificiosDe(Jugador jugador)
        {
            foreach (Edificio edificio in jugador.Edificios)
            {
                CrearVisualEdificio(edificio);
 
                GameObject objeto = edificiosVisuales[edificio.Id];
 
                if (edificio.EstaDestruido() && objeto.activeSelf)
                    OcultarConUltimoGolpe(objeto);
            }
        }
 
        // Antes de ocultar un objeto destruido, su barra lee la vida una última vez
        // para mostrar el daño del golpe final.
        private void OcultarConUltimoGolpe(GameObject objeto)
        {
            BarraVida barra = objeto.GetComponentInChildren<BarraVida>(true);
 
            if (barra != null)
                barra.Actualizar();
 
            objeto.SetActive(false);
        }
 
        // Oculta la decoración de un recurso cuando se agota.
        private void ActualizarRecursos()
        {
            foreach (RecursoVisual recurso in recursosVisuales)
            {
                if (recurso.Objeto.activeSelf && recurso.Celda.CantidadRecurso <= 0)
                    recurso.Objeto.SetActive(false);
            }
        }
    }
}