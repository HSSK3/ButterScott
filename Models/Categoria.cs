
namespace ButterScott.Models
{
    public class Categoria
    {
        public uint id_categoria { get; set; }

        public string nome { get; set; } = string.Empty;

        public string? descricao { get; set; }

        public bool ativa { get; set; } = true;
    }
}
