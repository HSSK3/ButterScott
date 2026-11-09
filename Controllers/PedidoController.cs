
using ButterScott.Data;
using ButterScott.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    public class PedidoController : Controller
    {
        private readonly ButterScottContext _context;

        public PedidoController(ButterScottContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MeusPedidos()
        {
            var clienteIdTexto = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteIdTexto, out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para consultar seus pedidos.";

                return RedirectToAction("Login", "Conta");
            }

            var pedidos = await _context.Pedidos
                .Where(p => p.id_cliente == clienteId)
                .OrderByDescending(p => p.data_pedido)
                .ToListAsync();

            return View(pedidos);
        }

        public async Task<IActionResult> Detalhes(uint id)
        {
            var clienteIdTexto = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteIdTexto, out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para consultar seus pedidos.";

                return RedirectToAction("Login", "Conta");
            }

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p =>
                    p.id_pedido == id &&
                    p.id_cliente == clienteId);

            if (pedido == null)
            {
                return NotFound();
            }

            var itens = await _context.ItensPedido
                .Where(i => i.id_pedido == pedido.id_pedido)
                .Join(
                    _context.Produtos,
                    item => item.id_produto,
                    produto => produto.id_produto,
                    (item, produto) => new
                    {
                        item.id_produto,
                        produto.nome,
                        item.quantidade,
                        item.preco_unitario,
                        item.subtotal
                    })
                .ToListAsync();

            var entrega = await _context.Entregas
                .Where(e => e.id_pedido == pedido.id_pedido)
                .Join(
                    _context.Enderecos,
                    e => e.id_endereco,
                    endereco => endereco.id_endereco,
                    (e, endereco) => new
                    {
                        e.status,
                        e.codigo_rastreamento,
                        e.data_saida,
                        e.data_entrega,
                        endereco.identificacao,
                        endereco.rua,
                        endereco.numero,
                        endereco.complemento,
                        endereco.bairro,
                        endereco.cidade,
                        endereco.estado,
                        endereco.cep
                    })
                .FirstOrDefaultAsync();

            ViewBag.Entrega = entrega;
            ViewBag.Pedido = pedido;

            return View(itens);
        }

        public async Task<IActionResult> Resultado(uint id)
        {
            var clienteIdTexto = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteIdTexto, out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para consultar seu pedido.";

                return RedirectToAction("Login", "Conta");
            }

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p =>
                    p.id_pedido == id &&
                    p.id_cliente == clienteId);

            if (pedido == null)
            {
                return NotFound();
            }

            if (pedido.forma_pagamento == "PIX")
            {
                ViewBag.CodigoPix =
                    $"BUTTERSCOTT-PIX-{pedido.id_pedido:D8}";
            }

            if (pedido.forma_pagamento == "Boleto")
            {
                ViewBag.CodigoBoleto =
                    $"BUTTERSCOTT-BOLETO-{pedido.id_pedido:D8}-SIMULACAO";

                ViewBag.VencimentoBoleto =
                    DateTime.Today.AddDays(3);
            }

            return View(pedido);
        }
    }
}
