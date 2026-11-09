
using ButterScott.Data;
using ButterScott.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly ButterScottContext _context;

        public AdminController(ButterScottContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Entregas()
        {
            var entregas = await _context.Entregas
                .Join(
                    _context.Pedidos,
                    entrega => entrega.id_pedido,
                    pedido => pedido.id_pedido,
                    (entrega, pedido) => new
                    {
                        entrega.id_entrega,
                        entrega.id_pedido,
                        entrega.status,
                        entrega.codigo_rastreamento,
                        entrega.data_saida,
                        entrega.data_entrega,
                        pedido.data_pedido,
                        pedido.valor_total,
                        pedido.id_cliente
                    })
                .Join(
                    _context.Clientes,
                    entrega => entrega.id_cliente,
                    cliente => cliente.id_cliente,
                    (entrega, cliente) => new
                    {
                        entrega.id_entrega,
                        entrega.id_pedido,
                        entrega.status,
                        entrega.codigo_rastreamento,
                        entrega.data_saida,
                        entrega.data_entrega,
                        entrega.data_pedido,
                        entrega.valor_total,
                        cliente.nome,
                        cliente.email
                    })
                .OrderByDescending(e => e.data_pedido)
                .ToListAsync();

            return View(entregas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarEntrega(
            int id_entrega,
            string novo_status)
        {
            var statusPermitidos = new[]
            {
                "Aguardando",
                "Em preparo",
                "Saiu para entrega",
                "Entregue",
                "Cancelada"
            };

            if (!statusPermitidos.Contains(novo_status))
            {
                TempData["Erro"] = "Status de entrega inválido.";
                return RedirectToAction(nameof(Entregas));
            }

            var entrega = await _context.Entregas
                .FirstOrDefaultAsync(e => e.id_entrega == id_entrega);

            if (entrega == null)
            {
                TempData["Erro"] = "Entrega não encontrada.";
                return RedirectToAction(nameof(Entregas));
            }

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.id_pedido == entrega.id_pedido);

            if (pedido == null)
            {
                TempData["Erro"] = "Pedido relacionado não encontrado.";
                return RedirectToAction(nameof(Entregas));
            }

            await using var transacao =
                await _context.Database.BeginTransactionAsync();

            try
            {
                entrega.status = novo_status;

                if (novo_status == "Saiu para entrega")
                {
                    entrega.data_saida ??= DateTime.Now;
                }

                if (novo_status == "Entregue")
                {
                    entrega.data_saida ??= DateTime.Now;
                    entrega.data_entrega ??= DateTime.Now;
                }

                if (novo_status == "Cancelada")
                {
                    pedido.status = "Cancelado";
                }
                else
                {
                    pedido.status = novo_status;
                }

                await _context.SaveChangesAsync();
                await transacao.CommitAsync();

                TempData["Sucesso"] =
                    "Status da entrega atualizado com sucesso.";
            }
            catch
            {
                await transacao.RollbackAsync();

                TempData["Erro"] =
                    "Não foi possível atualizar o status da entrega.";
            }

            return RedirectToAction(nameof(Entregas));
        }
    }
}
