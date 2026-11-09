
namespace ButterScott.Models
{
    public class Cliente
    {
        public uint id_cliente { get; set; }

        public string nome { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public string senha_hash { get; set; } = string.Empty;

        public string? telefone { get; set; }

        public DateTime data_cadastro { get; set; }

        public bool ativo { get; set; } = true;
        
        public bool eh_admin { get; set; } = false;
    }
}
