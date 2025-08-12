using Domain.Entidades;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Servicios
{
    public class CitaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CitaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task GenerarSlotsPorConfiguracionAsync(int configuracionId)
        {
            // Obtiene la configuracion
            var config = await _unitOfWork.Configuraciones.GetByIdAsync(configuracionId);
            if (config == null)
                throw new Exception("Configuracion no encontrada");

            // Obtiene el turno asociado
            var turno = await _unitOfWork.Turnos.GetByIdAsync(config.TurnoId);
            if (turno == null)
                throw new Exception("Turno no encontrado");

            // Obtiene las estaciones disponibles
            var estaciones = await _unitOfWork.Estaciones.GetAllAsync();
            if (!estaciones.Any())
                throw new Exception("No hay estaciones disponibles");

            // Calcula la cantidad de slots segun la duracion y horario del turno
            var inicio = turno.HoraInicio;
            var fin = turno.HoraFin;
            var duracion = TimeSpan.FromMinutes(config.DuracionCitaMinutos);
            var totalSlots = (int)((fin - inicio).TotalMinutes / config.DuracionCitaMinutos);

            // Selecciona solo las estaciones permitidas por la configuracion
            var estacionesParaUsar = estaciones.Take(config.CantidadEstaciones).ToList();

            var citasAGuardar = new List<Cita>();

            for (int i = 0; i < totalSlots; i++)
            {
                var horaSlot = inicio + TimeSpan.FromMinutes(i * config.DuracionCitaMinutos);

                foreach (var estacion in estacionesParaUsar)
                {
                    var cita = new Cita
                    {
                        FechaHora = config.Fecha.Date + horaSlot,
                        EstacionNumero = estacion.Numero,
                        Estado = EstadoCita.Pendiente,
                        TurnoId = config.TurnoId,
                        UsuarioId = 0, // Sin usuario asignado todavia
                        TipoTramite = TipoTramite.Ninguno
                    };
                    citasAGuardar.Add(cita);
                }
            }

            // Guarda en la base de datos
            foreach (var cita in citasAGuardar)
            {
                await _unitOfWork.Citas.AddAsync(cita);
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
