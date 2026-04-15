using EDUCONTROL.Models;
using Microsoft.EntityFrameworkCore;

namespace EDUCONTROL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Asignatura> Asignaturas { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // NIE unico por alumno
            modelBuilder.Entity<Alumno>()
            .HasIndex(a => a.NIE).IsUnique();
            // NombreUsuario unico
            modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario).IsUnique();
            // Nota: un alumno tiene una sola nota por asignatura
            modelBuilder.Entity<Nota>()
            .HasIndex(n => new { n.AlumnoId, n.AsignaturaId }).IsUnique();
        }
    }
}