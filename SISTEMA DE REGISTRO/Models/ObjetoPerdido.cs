using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SISTEMA_DE_REGISTRO.Models
{
    [Table("objetos_perdidos")]
    public class ObjetoPerdido : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("estudiante_id")]
        public string EstudianteId { get; set; } = string.Empty;

        [Column("dispositivo_id")]
        public string DispositivoId { get; set; } = string.Empty;

        [Column("descripción_hallazgo")]
        public string DescripcionHallazgo { get; set; } = string.Empty;

        [Column("estado_reportaje")]
        public string EstadoReportaje { get; set; } = string.Empty;

        [Column("fecha_reportaje")]
        public DateTime FechaReportaje { get; set; } = DateTime.UtcNow;
    }
}