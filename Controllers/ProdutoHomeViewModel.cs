
using ButterScott.Models;

namespace ButterScott.Models
{
    public class ProdutoHomeViewModel
    {
        public Produto Produto { get; set; } = null!;
        public bool EhFavorito { get; set; }
    }
}
