using Application.DTOs.Configuracion;
using Domain.Entidades;

namespace Application.Mappers
{
    public static class ConfiguracionMapper
    {
        public static Configuracion ToEntity(this ConfiguracionCreateDto dto) =>
            new Configuracion
            {
                Fecha = dto.Fecha,
                DuracionCitaMinutos = dto.DuracionCitaMinutos,
                CantidadEstaciones = dto.CantidadEstaciones,
                TurnoId = dto.TurnoId
            };

        public static ConfiguracionDto ToDto(this Configuracion entity) =>
            new ConfiguracionDto
            {
                Id = entity.Id,
                Fecha = entity.Fecha,
                DuracionCitaMinutos = entity.DuracionCitaMinutos,
                CantidadEstaciones = entity.CantidadEstaciones,
                TurnoId = entity.TurnoId
            };
    }
}
