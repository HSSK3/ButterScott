
using System.ComponentModel.DataAnnotations;

namespace ButterScott.Models
{
    public class Pedido
    {
        [Key]
        public uint id_pedido { get; set; }

        public uint id_cliente { get; set; }

        public DateTime data_pedido { get; set; }

        public string status { get; set; } = "Pendente";

        public decimal valor_subtotal { get; set; }

        public decimal valor_entrega { get; set; }

        public decimal valor_desconto { get; set; }

        public decimal valor_total { get; set; }

        public string? observacoes { get; set; }

        public string forma_pagamento { get; set; } = string.Empty;

        public string status_pagamento { get; set; } = "Aguardando Pagamento";
    }
}
