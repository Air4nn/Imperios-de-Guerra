using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
 
namespace ImperiosEnGuerra.Servicios
{
    // Hilos de la aplicación (todos son tareas del ThreadPool lanzadas con Task.Run):
    //  - Construcción de una casa      (espera SegundosConstruccion)
    //  - Entrenamiento de un soldado   (espera SegundosEntrenamiento)
    //  - Movimiento de una unidad      (un paso cada MillisPorPaso)
    //  - Recolección automática        (un ciclo por segundo)
    // Ninguno toca la API de Unity: lo que cambia la estructura del juego (agregar un
    // edificio o una unidad) se encola con "alHiloPrincipal" y lo ejecuta el hilo principal.
    // Todos se cancelan con Detener() (fin de partida o cierre del juego).
    public class ConcurrenciaService
    {
        public const double SegundosConstruccion = 5;
        public const double SegundosEntrenamiento = 4;
        public const int MillisPorPaso = 400;
 
        private readonly JuegoService juego;
        private readonly Action<Action> alHiloPrincipal;
        private readonly Action<int, string, string> notificar;
 
        private readonly CancellationTokenSource global = new CancellationTokenSource();
 
        private readonly ConcurrentDictionary<int, CancellationTokenSource> movimientos =
            new ConcurrentDictionary<int, CancellationTokenSource>();
 
        private readonly ConcurrentDictionary<int, TareaProgreso> tareas =
            new ConcurrentDictionary<int, TareaProgreso>();
 
        private int siguienteTarea;
 
        public ConcurrenciaService(
            JuegoService juego,
            Action<Action> alHiloPrincipal,
            Action<int, string, string> notificar)
        {
            this.juego = juego;
            this.alHiloPrincipal = alHiloPrincipal;
            this.notificar = notificar;
        }
 
        // Copia segura (instantánea) de las tareas en curso, para mostrarlas en la interfaz.
        public ICollection<TareaProgreso> TareasEnCurso
        {
            get { return tareas.Values; }
        }
 
        // Cancela todos los hilos de fondo.
        public void Detener()
        {
            global.Cancel();
        }
 
        private TareaProgreso NuevaTarea(string descripcion, int jugadorId, double total)
        {
            TareaProgreso tarea = new TareaProgreso
            {
                Id = Interlocked.Increment(ref siguienteTarea),
                Descripcion = descripcion,
                JugadorId = jugadorId,
                Total = total
            };
 
            tareas[tarea.Id] = tarea;
            return tarea;
        }
 
        private async Task Esperar(TareaProgreso tarea, CancellationToken token)
        {
            Stopwatch reloj = Stopwatch.StartNew();
 
            while (reloj.Elapsed.TotalSeconds < tarea.Total)
            {
                await Task.Delay(100, token);
                tarea.Transcurrido = Math.Min(reloj.Elapsed.TotalSeconds, tarea.Total);
            }
        }
 
        // ---------- Construcción ----------
 
        public void ProgramarConstruccion(int jugadorId, int x, int y)
        {
            TareaProgreso tarea = NuevaTarea("Construyendo casa", jugadorId, SegundosConstruccion);
            CancellationToken token = global.Token;
 
            Task.Run(async () =>
            {
                try
                {
                    await Esperar(tarea, token);
 
                    alHiloPrincipal(() =>
                    {
                        juego.CompletarConstruccionCasa(jugadorId, x, y);
                        notificar(jugadorId, "Construir casa",
                            "Casa terminada en (" + x + "," + y + ")");
                    });
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    TareaProgreso quitada;
                    tareas.TryRemove(tarea.Id, out quitada);
                }
            });
        }
 
        // ---------- Entrenamiento ----------
 
        public void ProgramarEntrenamiento(int jugadorId, int x, int y)
        {
            TareaProgreso tarea = NuevaTarea("Entrenando soldado", jugadorId, SegundosEntrenamiento);
            CancellationToken token = global.Token;
 
            Task.Run(async () =>
            {
                try
                {
                    await Esperar(tarea, token);
 
                    alHiloPrincipal(() =>
                    {
                        juego.CompletarEntrenamiento(jugadorId, x, y);
                        notificar(jugadorId, "Entrenar soldado",
                            "Soldado listo en (" + x + "," + y + ")");
                    });
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    TareaProgreso quitada;
                    tareas.TryRemove(tarea.Id, out quitada);
                }
            });
        }
 
        // ---------- Movimiento ----------
 
        // Un hilo por unidad en movimiento; una orden nueva cancela la anterior.
        public void IniciarMovimiento(int jugadorId, int unidadId, int destinoX, int destinoY)
        {
            CancellationTokenSource anterior;
 
            if (movimientos.TryRemove(unidadId, out anterior))
                anterior.Cancel();
 
            CancellationTokenSource propio =
                CancellationTokenSource.CreateLinkedTokenSource(global.Token);
 
            movimientos[unidadId] = propio;
            CancellationToken token = propio.Token;
 
            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(MillisPorPaso, token);
 
                        ResultadoPaso paso =
                            juego.DarPaso(jugadorId, unidadId, destinoX, destinoY);
 
                        if (paso == ResultadoPaso.EnCamino)
                            continue;
 
                        if (paso == ResultadoPaso.Llego)
                            notificar(jugadorId, "Mover",
                                "Unidad #" + unidadId + " llegó a (" + destinoX + "," + destinoY + ")");
                        else if (paso == ResultadoPaso.Bloqueado)
                            notificar(jugadorId, "Mover",
                                "Unidad #" + unidadId + " se detuvo: camino bloqueado");
 
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                }
            });
        }
 
        // ---------- Recolección ----------
 
        public Task RecoleccionAutomatica(int jugadorId, int unidadId, int segundos)
        {
            TareaProgreso tarea = NuevaTarea(
                "Recolectando (unidad #" + unidadId + ")", jugadorId, segundos);
 
            CancellationToken token = global.Token;
 
            return Task.Run(async () =>
            {
                try
                {
                    for (int i = 0; i < segundos; i++)
                    {
                        await Task.Delay(1000, token);
                        juego.RecolectarRecurso(jugadorId, unidadId);
                        tarea.Transcurrido = i + 1;
                    }
 
                    notificar(jugadorId, "Recolectar",
                        "Unidad #" + unidadId + " terminó de recolectar (" + segundos + " s)");
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    TareaProgreso quitada;
                    tareas.TryRemove(tarea.Id, out quitada);
                }
            });
        }
    }
}
 