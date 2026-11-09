
using System.Text.Json;
using ButterScott.Models;
using Microsoft.AspNetCore.Http;

namespace ButterScott.Services
{
    public class CarrinhoService
    {
        private const string ChaveCarrinho = "ButterScott.Carrinho";

        private readonly ISession _sessao;

        public CarrinhoService(IHttpContextAccessor accessor)
        {
            _sessao = accessor.HttpContext!.Session;
        }

        public List<ItemCarrinho> ObterItens()
        {
            var json = _sessao.GetString(ChaveCarrinho);

            if (string.IsNullOrEmpty(json))
            {
                return new List<ItemCarrinho>();
            }

            return JsonSerializer.Deserialize<List<ItemCarrinho>>(json)
                   ?? new List<ItemCarrinho>();
        }

        public void SalvarItens(List<ItemCarrinho> itens)
        {
            var json = JsonSerializer.Serialize(itens);
            _sessao.SetString(ChaveCarrinho, json);
        }

        public decimal CalcularTotal()
        {
            return ObterItens().Sum(item => item.Subtotal);
        }
    }
}
