namespace ButterScott.Models
{
    public class Cupom
    {
        public uint id_cupom { get; set; }

        public string codigo { get; set; } = string.Empty;

        public string? descricao { get; set; }

        public string tipo_desconto { get; set; } = "Percentual";

        public decimal valor_desconto { get; set; }

        public decimal valor_minimo_pedido { get; set; }

        public DateTime? data_inicio { get; set; }

        public DateTime? data_fim { get; set; }

        public uint? limite_uso { get; set; }

        public uint usos_realizados { get; set; }

        public bool ativo { get; set; } = true;
    }
}