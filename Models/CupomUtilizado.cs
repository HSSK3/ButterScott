namespace ButterScott.Models
{
    public class CupomUtilizado
    {
        public uint id_cupom { get; set; }

        public uint id_pedido { get; set; }

        public uint id_cliente { get; set; }

        public DateTime data_utilizacao { get; set; }

        public decimal valor_desconto_aplicado { get; set; }
    }
}