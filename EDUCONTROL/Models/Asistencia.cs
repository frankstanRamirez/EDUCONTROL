using EDUCONTROL.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDUCONTROL.Models
{
    public class Asistencia
    {
        public int Id { get; set; }
        [Required]
        public int AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }
        [Required]
        public DateTime Fecha { get; set; } = DateTime.Today;
        // Valores: Presente | Ausente | Tardanza
        [Required]
        public string Estado { get; set; } = "Presente";
        [MaxLength(200)]
        public string? Observacion { get; set; }
        public string RegistradoPor { get; set; } = string.Empty;
    }
}
