
using System.ComponentModel.DataAnnotations;

namespace ButterScott.Models
{
    public class Entrega
    {
        [Key]
        public uint id_entrega { get; set; }

        public uint id_pedido { get; set; }

        public uint id_endereco { get; set; }

        public string status { get; set; } = "Aguardando";

        [StringLength(100)]
        public string? codigo_rastreamento { get; set; }

        public DateTime? data_saida { get; set; }

        public DateTime? data_entrega { get; set; }

        [StringLength(500)]
        public string? observacoes { get; set; }
    }
}
