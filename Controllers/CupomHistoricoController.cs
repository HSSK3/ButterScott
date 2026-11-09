
using ButterScott.Data;
using ButterScott.Filters;
using ButterScott.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    [AdminAuthorize]
    public class CupomHistoricoController : Controller
    {
        private readonly ButterScottContext _context;

        public CupomHistoricoController(ButterScottContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var historico = await (
                from utilizacao in _context.CuponsUtilizados
                join cupom in _context.Cupons
                    on utilizacao.id_cupom equals cupom.id_cupom
                join cliente in _context.Clientes
                    on utilizacao.id_cliente equals cliente.id_cliente
                join pedido in _context.Pedidos
                    on utilizacao.id_pedido equals pedido.id_pedido
                orderby utilizacao.data_utilizacao descending
                select new CupomHistoricoViewModel
                {
                    id_cupom = utilizacao.id_cupom,
                    codigo_cupom = cupom.codigo,
                    descricao_cupom = cupom.descricao,
                    id_pedido = utilizacao.id_pedido,
                    id_cliente = utilizacao.id_cliente,
                    nome_cliente = cliente.nome,
                    email_cliente = cliente.email,
                    data_utilizacao = utilizacao.data_utilizacao,
                    valor_desconto_aplicado =
                        utilizacao.valor_desconto_aplicado
                }
            ).ToListAsync();

            return View(historico);
        }
    }
}
