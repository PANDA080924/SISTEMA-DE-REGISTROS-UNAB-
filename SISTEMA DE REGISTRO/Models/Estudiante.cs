using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SISTEMA_DE_REGISTRO.Models
{
    [Table("estudiantes")]
    public class Estudiante : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("correo_institucional")]
        public string CorreoInstitucional { get; set; } = string.Empty;

        [Column("codigo_estudiante")]
        public string CodigoEstudiante { get; set; } = string.Empty;

        [Column("carrera_profesional")]
        public string CarreraProfesional { get; set; } = string.Empty;

        [Column("ciclo_academico")]
        public string CicloAcademico { get; set; } = string.Empty;

        [Column("facultad")]
        public string Facultad { get; set; } = string.Empty;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Column("estado_activo")]
        public bool EstadoActivo { get; set; } = true;

        [Column("contrasena")] // <-- Nueva columna para la contraseña/PIN
        public string Contrasena { get; set; } = string.Empty;
    }
}