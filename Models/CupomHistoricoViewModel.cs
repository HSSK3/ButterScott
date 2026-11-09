namespace ButterScott.Models
{
    public class CupomHistoricoViewModel
    {
        public uint id_cupom { get; set; }

        public string codigo_cupom { get; set; } = string.Empty;

        public string? descricao_cupom { get; set; }

        public uint id_pedido { get; set; }

        public uint id_cliente { get; set; }

        public string nome_cliente { get; set; } = string.Empty;

        public string email_cliente { get; set; } = string.Empty;

        public DateTime data_utilizacao { get; set; }

        public decimal valor_desconto_aplicado { get; set; }
    }
}