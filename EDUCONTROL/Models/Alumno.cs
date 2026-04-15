using EDUCONTROL.Models;
using System.ComponentModel.DataAnnotations;

namespace EDUCONTROL.Models
{
    public class Alumno
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        [Display(Name = "NIE")]
        public string NIE { get; set; } = string.Empty;
        [Required]
        [MaxLength(150)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        [Display(Name = "Grado")]
        public string Grado { get; set; } = string.Empty;
        [Display(Name = "Seccion")]
        public string Seccion { get; set; } = "A";
        [Display(Name = "Anio academico")]
        public int AnioAcademico { get; set; } = DateTime.Now.Year;
        public string Estado { get; set; } = "Activo";
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        // Relaciones
        public ICollection<Nota> Notas { get; set; } = new List<Nota>();
        public ICollection<Asistencia> Asistencias { get; set; } = new
       List<Asistencia>();
    }
}