using EDUCONTROL.Models;
using System.ComponentModel.DataAnnotations;

namespace   EDUCONTROL.Models
{
    public class Asignatura
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        [Display(Name = "Nombre de la asignatura")]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(20)]
        [Display(Name = "Grado")]
        public string? Grado { get; set; }
        public bool Activa { get; set; } = true;
        // Relacion
        public ICollection<Nota> Notas { get; set; } = new List<Nota>();
    }
}
