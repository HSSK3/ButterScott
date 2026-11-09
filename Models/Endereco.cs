
using System.ComponentModel.DataAnnotations;

namespace ButterScott.Models
{
    public class Endereco
    {
        [Key]
        public uint id_endereco { get; set; }

        public uint id_cliente { get; set; }

        [StringLength(50)]
        public string? identificacao { get; set; }

        [Required(ErrorMessage = "Informe a rua.")]
        [StringLength(150)]
        public string rua { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o número.")]
        [StringLength(20)]
        public string numero { get; set; } = string.Empty;

        [StringLength(100)]
        public string? complemento { get; set; }

        [Required(ErrorMessage = "Informe o bairro.")]
        [StringLength(100)]
        public string bairro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a cidade.")]
        [StringLength(100)]
        public string cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o estado.")]
        [StringLength(100)]
        public string estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o CEP.")]
        [StringLength(15)]
        public string cep { get; set; } = string.Empty;

        public bool principal { get; set; }
    }
}
