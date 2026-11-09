
using System.ComponentModel.DataAnnotations;

namespace ButterScott.Models
{
    public class ItemPedido
    {
        [Key]
        public uint id_item { get; set; }

        public uint id_pedido { get; set; }

        public uint id_produto { get; set; }

        public uint quantidade { get; set; }

        public decimal preco_unitario { get; set; }

        public decimal subtotal { get; set; }
    }
}
