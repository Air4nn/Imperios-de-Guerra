using UnityEngine;
using ImperiosEnGuerra.Modelo;
using System.Collections.Generic;
 
namespace ImperiosEnGuerra.Vista
{
    public class MapaView : MonoBehaviour
    {
        public float tamanoCelda = 1f;
 
        private Dictionary<int, GameObject> unidadesVisuales =
            new Dictionary<int, GameObject>();
 
        private Dictionary<int, GameObject> edificiosVisuales =
            new Dictionary<int, GameObject>();
 
        private InteraccionMapa interaccion;
 
        private void Start()
        {
            interaccion = FindFirstObjectByType<InteraccionMapa>();
            DibujarMapa();
        }
 
        private void Update()
        {
            ActualizarUnidades();
            ActualizarEdificios();
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
 
                    if (datos.Recurso == TipoRecurso.Oro)
                        r.material.color = Color.yellow;
                    else if (datos.Recurso == TipoRecurso.Madera)
                        r.material.color = new Color(0.4f, 0.25f, 0.1f);
                    else if (datos.Recurso == TipoRecurso.Comida)
                        r.material.color = Color.green;
                    else
                        r.material.color = (x + y) % 2 == 0
                            ? new Color(0.35f, 0.55f, 0.3f)
                            : new Color(0.3f, 0.5f, 0.25f);
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
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule
                );
 
            objeto.name = "Unidad_" + unidad.Id;
 
            objeto.transform.position =
                new Vector3(
                    unidad.X,
                    0.8f,
                    unidad.Y
                );
 
            objeto.transform.localScale =
                new Vector3(
                    0.5f,
                    0.8f,
                    0.5f
                );
 
            Renderer renderer =
                objeto.GetComponent<Renderer>();
 
            renderer.material.color =
                ColorDeJugador(unidad.JugadorId);
 
            unidadesVisuales.Add(
                unidad.Id,
                objeto
            );
        }
 
        private void CrearVisualEdificio(Edificio edificio)
        {
            if (edificiosVisuales.ContainsKey(edificio.Id))
                return;
 
            GameObject objeto =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );
 
            objeto.name =
                "Edificio_" + edificio.Id;
 
            Renderer renderer =
                objeto.GetComponent<Renderer>();
 
            Color color = ColorDeJugador(edificio.JugadorId);
 
            if (edificio.Tipo == TipoEdificio.Casa)
            {
                // Las casas son mas pequenas y claras que el Centro Urbano.
                objeto.transform.position =
                    new Vector3(edificio.X, 0.5f, edificio.Y);
 
                objeto.transform.localScale =
                    new Vector3(0.6f, 0.8f, 0.6f);
 
                renderer.material.color =
                    Color.Lerp(color, Color.white, 0.4f);
            }
            else
            {
                objeto.transform.position =
                    new Vector3(edificio.X, 1f, edificio.Y);
 
                objeto.transform.localScale =
                    new Vector3(0.9f, 1.5f, 0.9f);
 
                renderer.material.color = color;
            }
 
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
 
                objeto.transform.position =
                    new Vector3(
                        unidad.X,
                        0.8f,
                        unidad.Y
                    );
 
                // La unidad seleccionada se ve en amarillo.
                bool seleccionada =
                    interaccion != null &&
                    interaccion.Seleccionada == unidad;
 
                objeto.GetComponent<Renderer>().material.color =
                    seleccionada
                        ? Color.yellow
                        : ColorDeJugador(unidad.JugadorId);
 
                if (!unidad.EstaViva())
                {
                    objeto.SetActive(false);
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
 
                edificiosVisuales[edificio.Id]
                    .SetActive(!edificio.EstaDestruido());
            }
        }
    }
}