using System.Threading;
using System.Threading.Tasks;

namespace ImperiosEnGuerra.Servicios
{
    public class ConcurrenciaService
    {
        private readonly JuegoService juego;

        public ConcurrenciaService(JuegoService juego) { this.juego = juego; }

        public Task RecoleccionAutomatica(int jugadorId, int unidadId, int segundos)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < segundos; i++)
                {
                    Thread.Sleep(1000);
                    juego.RecolectarRecurso(jugadorId, unidadId);
                }
            });
        }
    }
}