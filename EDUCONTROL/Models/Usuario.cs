using System.ComponentModel.DataAnnotations;
namespace EDUCONTROL.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        [Display(Name = "Contrasena")]
        public string Contrasena { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Rol")]
        // Valores permitidos: Director | Secretaria | Profesor
        public string Rol { get; set; } = "Profesor";
        [Required]
        [MaxLength(150)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;
        // Solo obligatorio si Rol == "Profesor"
        [Display(Name = "Grado asignado")]
        public string? GradoAsignado { get; set; }
        public string? SeccionAsignada { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
    }
}
