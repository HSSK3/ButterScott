
namespace ButterScott.Models
{
    public class ItemCarrinho
    {
        public uint IdProduto { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? ImagemUrl { get; set; }

        public decimal Preco { get; set; }

        public int Quantidade { get; set; } = 1;

        public decimal Subtotal => Preco * Quantidade;
    }
}
