using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SISTEMA_DE_REGISTRO.Models
{
    [Table("registros_accesos")]
    public class RegistroAcceso : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("creado_at")]
        public DateTime CreadoAt { get; set; }

        [Column("codigo_estudiante")]
        public string CodigoEstudiante { get; set; } = string.Empty;

        [Column("equipo")]
        public string Equipo { get; set; } = string.Empty;

        [Column("tipo_evento")]
        public string TipoEvento { get; set; } = string.Empty;

        [Column("escaneado_por")]
        public string EscaneadoPor { get; set; } = string.Empty;

        [Column("numero_serie")]
        public string NumeroSerie { get; set; } = string.Empty;

        [Column("estudiante_id")]
        public string EstudianteId { get; set; } = string.Empty;

        [Column("dispositivo_id")]
        public string? DispositivoId { get; set; }

        // Propiedades de compatibilidad para ControlAccesoPage y MisRegistrosAccesoPage
        [JsonIgnore]
        public DateTime FechaRegistro
        {
            get => CreadoAt;
            set => CreadoAt = value;
        }

        [JsonIgnore]
        public DateTime CreatedAt
        {
            get => CreadoAt;
            set => CreadoAt = value;
        }
    }
}