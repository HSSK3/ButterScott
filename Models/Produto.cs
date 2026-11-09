
namespace ButterScott.Models
{
    public class Produto
    {
        public uint id_produto { get; set; }

        public uint id_categoria { get; set; }

        public string nome { get; set; } = string.Empty;

        public string? descricao { get; set; }

        public string sabor { get; set; } = string.Empty;

        public decimal preco { get; set; }

        public uint estoque { get; set; }

        public string? imagem_url { get; set; }

        public bool ativo { get; set; } = true;

        public DateTime data_criacao { get; set; }

        public DateTime data_atualizacao { get; set; }
    }
}
