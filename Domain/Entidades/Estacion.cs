using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entidades
{
    public class Estacion : IEntity
    {
        public int Id { get; set; }

        [Required]
        public int Numero { get; set; }

        public string Descripcion { get; set; }
    }
}
