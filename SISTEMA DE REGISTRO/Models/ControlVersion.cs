using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SISTEMA_DE_REGISTRO.Models
{
    [Table("control_versiones")]
    public class ControlVersion : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("version")]
        public string Version { get; set; } = string.Empty;

        [Column("url_descarga")]
        public string UrlDescarga { get; set; } = string.Empty;

        [Column("novedades")]
        public string Novedades { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}