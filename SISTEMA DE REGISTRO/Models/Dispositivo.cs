using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SISTEMA_DE_REGISTRO.Models
{
    [Table("dispositivos")]
    public class Dispositivo : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("tipo_dispositivo")]
        public string TipoDispositivo { get; set; } = string.Empty;

        [Column("marca")]
        public string Marca { get; set; } = string.Empty;

        [Column("modelo")]
        public string Modelo { get; set; } = string.Empty;

        [Column("numero_serie")]
        public string NumeroSerie { get; set; } = string.Empty;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Column("codigo_qr")]
        public string CodigoQr { get; set; } = string.Empty;

        [Column("mac_direccion")]
        public string MacDirección { get; set; } = string.Empty;

        [Column("estudiante_id")]
        public string EstudianteId { get; set; } = string.Empty;

        [Column("foto_url")]
        public string? FotoUrl { get; set; }
    }
}