using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EDUCONTROL.Models
{
    public class Nota
    {
        public int Id { get; set; }

        [Required]
        public int AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        [Required]
        public int AsignaturaId { get; set; }
        [ForeignKey("AsignaturaId")]
        public Asignatura? Asignatura { get; set; }

        [Range(0, 10)]
        [Display(Name = "Periodo 1")]
        public decimal? Periodo1 { get; set; }

        [Range(0, 10)]
        [Display(Name = "Periodo 2")]
        public decimal? Periodo2 { get; set; }

        [Range(0, 10)]
        [Display(Name = "Periodo 3")]
        public decimal? Periodo3 { get; set; }

        // Promedio dinámico
        public decimal Promedio
        {
             get
            {
                if (!Periodo1.HasValue && !Periodo2.HasValue && !Periodo3.HasValue) return 0;


                decimal suma = (Periodo1 ?? 0) + (Periodo2 ?? 0) + (Periodo3 ?? 0);


                return Math.Round(suma / 3, 2);
            }
        }

        public string Estado => Promedio >= 6 ? "Aprobado" : "Reprobado";

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public string RegistradoPor { get; set; } = string.Empty;
    }
}