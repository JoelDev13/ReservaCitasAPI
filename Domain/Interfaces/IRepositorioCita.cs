using Domain.Entidades;
using ProyectoFinal.Domain.Interfaces;

namespace Domain.Interfaces
{
    public interface IRepositorioCita : IGenericRepository<Cita>
    {
        List<Cita> ObtenerPorFechaYEstacion(DateTime fechaHora, int estacionNumero);
    }
}
