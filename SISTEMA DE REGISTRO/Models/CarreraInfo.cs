using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SISTEMA_DE_REGISTRO.Models
{
    [Table("carreras_info")]
    public class CarreraInfo : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("nombre_carrera")]
        public string NombreCarrera { get; set; } = string.Empty;

        [Column("facultad")]
        public string Facultad { get; set; } = string.Empty;

        [Column("total_ciclos")]
        public int TotalCiclos { get; set; }

        [Column("creado_at")]
        public DateTime CreadoAt { get; set; }
    }
}