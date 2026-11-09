
using ButterScott.Data;
using ButterScott.Models;
using ButterScott.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    public class CarrinhoController : Controller
    {
        private readonly ButterScottContext _context;
        private readonly CarrinhoService _carrinho;

        private const string ChaveCupom = "ButterScott.Cupom";

        public CarrinhoController(
            ButterScottContext context,
            CarrinhoService carrinho)
        {
            _context = context;
            _carrinho = carrinho;
        }

        public async Task<IActionResult> Index()
        {
            var itens = _carrinho.ObterItens();

            await CarregarDadosCupom(itens);

            return View(itens);
        }

        private async Task CarregarDadosCupom(List<ItemCarrinho> itens)
        {
            var subtotal = await CalcularSubtotalAtual(itens);

            ViewBag.Subtotal = subtotal;
            ViewBag.Cupom = null;
            ViewBag.Desconto = 0m;

            var cupomIdTexto = HttpContext.Session.GetString(ChaveCupom);

            if (!uint.TryParse(cupomIdTexto, out uint cupomId))
                return;

            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(c => c.id_cupom == cupomId);

            if (cupom == null || !ValidarCupom(cupom, subtotal, out _))
            {
                HttpContext.Session.Remove(ChaveCupom);
                return;
            }

            ViewBag.Cupom = cupom;
            ViewBag.Desconto = CalcularDesconto(cupom, subtotal);
        }

        private async Task<decimal> CalcularSubtotalAtual(
            List<ItemCarrinho> itens)
        {
            if (!itens.Any())
                return 0m;

            var idsProdutos = itens
                .Select(i => i.IdProduto)
                .Distinct()
                .ToList();

            var produtos = await _context.Produtos
                .Where(p => idsProdutos.Contains(p.id_produto) && p.ativo)
                .ToListAsync();

            return itens.Sum(item =>
            {
                var produto = produtos.FirstOrDefault(
                    p => p.id_produto == item.IdProduto);

                return produto == null
                    ? 0m
                    : produto.preco * item.Quantidade;
            });
        }

        private bool ValidarCupom(
            Cupom cupom,
            decimal subtotal,
            out string mensagem)
        {
            var agora = DateTime.Now;

            if (!cupom.ativo)
            {
                mensagem = "Este cupom está desativado.";
                return false;
            }

            if (cupom.data_inicio.HasValue &&
                agora < cupom.data_inicio.Value)
            {
                mensagem = "Este cupom ainda não está disponível.";
                return false;
            }

            if (cupom.data_fim.HasValue &&
                agora > cupom.data_fim.Value)
            {
                mensagem = "Este cupom expirou.";
                return false;
            }

            if (cupom.limite_uso.HasValue &&
                cupom.usos_realizados >= cupom.limite_uso.Value)
            {
                mensagem = "Este cupom atingiu o limite de utilizações.";
                return false;
            }

            if (subtotal < cupom.valor_minimo_pedido)
            {
                mensagem =
                    $"O pedido mínimo para este cupom é {cupom.valor_minimo_pedido:C}.";

                return false;
            }

            if (cupom.valor_desconto <= 0m)
            {
                mensagem = "Este cupom possui um desconto inválido.";
                return false;
            }

            if (cupom.tipo_desconto == "Percentual" &&
                cupom.valor_desconto > 100m)
            {
                mensagem = "O percentual de desconto deste cupom é inválido.";
                return false;
            }

            if (cupom.tipo_desconto != "Percentual" &&
                cupom.tipo_desconto != "ValorFixo")
            {
                mensagem = "O tipo de desconto deste cupom é inválido.";
                return false;
            }

            mensagem = string.Empty;
            return true;
        }

        private decimal CalcularDesconto(Cupom cupom, decimal subtotal)
        {
            decimal desconto;

            if (cupom.tipo_desconto == "Percentual")
            {
                desconto = subtotal * cupom.valor_desconto / 100m;
            }
            else
            {
                desconto = cupom.valor_desconto;
            }

            desconto = Math.Min(desconto, subtotal);

            return Math.Round(desconto, 2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AplicarCupom(string codigo)
        {
            var itens = _carrinho.ObterItens();

            if (!itens.Any())
            {
                TempData["MensagemCarrinho"] =
                    "Adicione produtos ao carrinho antes de aplicar um cupom.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(codigo))
            {
                TempData["MensagemCarrinho"] =
                    "Informe o código do cupom.";

                return RedirectToAction(nameof(Index));
            }

            codigo = codigo.Trim().ToUpperInvariant();

            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(c => c.codigo == codigo);

            if (cupom == null)
            {
                TempData["MensagemCarrinho"] =
                    "Cupom não encontrado.";

                return RedirectToAction(nameof(Index));
            }

            var subtotal = await CalcularSubtotalAtual(itens);

            if (!ValidarCupom(cupom, subtotal, out string mensagem))
            {
                TempData["MensagemCarrinho"] = mensagem;
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetString(
                ChaveCupom,
                cupom.id_cupom.ToString());

            TempData["MensagemCarrinho"] =
                $"Cupom {cupom.codigo} aplicado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoverCupom()
        {
            HttpContext.Session.Remove(ChaveCupom);

            TempData["MensagemCarrinho"] =
                "Cupom removido do carrinho.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(
            uint idEndereco,
            string formaPagamento)
        {
            var clienteIdTexto = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteIdTexto, out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para finalizar o pedido.";

                return RedirectToAction("Login", "Conta");
            }

            if (formaPagamento != "PIX" &&
                formaPagamento != "Cartao" &&
                formaPagamento != "Boleto" &&
                formaPagamento != "NaEntrega")
            {
                TempData["MensagemCarrinho"] =
                    "Selecione uma forma de pagamento válida.";

                return RedirectToAction(nameof(Confirmar));
            }

            string statusPagamento = formaPagamento switch
            {
                "PIX" => "Aguardando Pagamento",
                "Cartao" => "Pago (Simulação)",
                "Boleto" => "Aguardando Pagamento",
                "NaEntrega" => "Pagamento na Entrega",
                _ => "Aguardando Pagamento"
            };

            var itensCarrinho = _carrinho.ObterItens();

            if (!itensCarrinho.Any())
            {
                TempData["MensagemCarrinho"] = "Seu carrinho está vazio.";
                return RedirectToAction("Index");
            }

            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e =>
                    e.id_endereco == idEndereco &&
                    e.id_cliente == clienteId);

            if (endereco == null)
            {
                TempData["MensagemCarrinho"] =
                    "Selecione um endereço de entrega válido.";

                return RedirectToAction(nameof(Confirmar));
            }

            var idsProdutos = itensCarrinho
                .Select(i => i.IdProduto)
                .Distinct()
                .ToList();

            var produtos = await _context.Produtos
                .Where(p => idsProdutos.Contains(p.id_produto) && p.ativo)
                .ToListAsync();

            if (produtos.Count != idsProdutos.Count)
            {
                TempData["MensagemCarrinho"] =
                    "Um ou mais produtos não estão mais disponíveis. Confira seu carrinho.";

                return RedirectToAction("Index");
            }

            foreach (var item in itensCarrinho)
            {
                var produto = produtos.First(p => p.id_produto == item.IdProduto);

                if (item.Quantidade <= 0 ||
                    (uint)item.Quantidade > produto.estoque)
                {
                    TempData["MensagemCarrinho"] =
                        $"Estoque insuficiente para {produto.nome}. Confira a quantidade disponível.";

                    return RedirectToAction("Index");
                }
            }

            var subtotal = itensCarrinho.Sum(item =>
            {
                var produto = produtos.First(p => p.id_produto == item.IdProduto);
                return produto.preco * item.Quantidade;
            });

            Cupom? cupomAplicado = null;
            decimal desconto = 0m;

            var cupomIdTexto = HttpContext.Session.GetString(ChaveCupom);

            if (uint.TryParse(cupomIdTexto, out uint cupomId))
            {
                cupomAplicado = await _context.Cupons
                    .FirstOrDefaultAsync(c => c.id_cupom == cupomId);

                if (cupomAplicado == null)
                {
                    HttpContext.Session.Remove(ChaveCupom);

                    TempData["MensagemCarrinho"] =
                        "O cupom aplicado não está mais disponível.";

                    return RedirectToAction("Index");
                }

                if (!ValidarCupom(cupomAplicado, subtotal, out string mensagemCupom))
                {
                    HttpContext.Session.Remove(ChaveCupom);

                    TempData["MensagemCarrinho"] = mensagemCupom;

                    return RedirectToAction("Index");
                }

                desconto = CalcularDesconto(cupomAplicado, subtotal);
            }

            var total = subtotal - desconto;

            await using var transacao =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var pedido = new Pedido
                {
                    id_cliente = clienteId,
                    data_pedido = DateTime.Now,
                    status = "Pendente",
                    forma_pagamento = formaPagamento,
                    status_pagamento = statusPagamento,
                    valor_subtotal = subtotal,
                    valor_entrega = 0m,
                    valor_desconto = desconto,
                    valor_total = total,
                    observacoes = null
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                var entrega = new Entrega
                {
                    id_pedido = pedido.id_pedido,
                    id_endereco = endereco.id_endereco,
                    status = "Aguardando"
                };

                _context.Entregas.Add(entrega);

                foreach (var item in itensCarrinho)
                {
                    var produto = produtos.First(p => p.id_produto == item.IdProduto);

                    _context.ItensPedido.Add(new ItemPedido
                    {
                        id_pedido = pedido.id_pedido,
                        id_produto = produto.id_produto,
                        quantidade = (uint)item.Quantidade,
                        preco_unitario = produto.preco,
                        subtotal = produto.preco * item.Quantidade
                    });

                    produto.estoque -= (uint)item.Quantidade;
                }

                if (cupomAplicado != null)
                {
                    cupomAplicado.usos_realizados++;

                    _context.CuponsUtilizados.Add(new CupomUtilizado
                    {
                        id_cupom = cupomAplicado.id_cupom,
                        id_pedido = pedido.id_pedido,
                        id_cliente = clienteId,
                        data_utilizacao = DateTime.Now,
                        valor_desconto_aplicado = desconto
                    });
                }

                await _context.SaveChangesAsync();

                await transacao.CommitAsync();

                _carrinho.SalvarItens(new List<ItemCarrinho>());

                HttpContext.Session.Remove(ChaveCupom);

                return RedirectToAction(
                    "Resultado",
                    "Pedido",
                    new { id = pedido.id_pedido });
            }
            catch (Exception)
            {
                await transacao.RollbackAsync();

                TempData["MensagemCarrinho"] =
                    "Não foi possível registrar o pedido. Tente novamente.";

                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Confirmar()
        {
            var clienteIdTexto = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteIdTexto, out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para finalizar o pedido.";

                return RedirectToAction("Login", "Conta");
            }

            var itens = _carrinho.ObterItens();

            if (!itens.Any())
            {
                TempData["MensagemCarrinho"] =
                    "Adicione pelo menos um cupcake antes de continuar.";

                return RedirectToAction("Index");
            }

            var enderecos = await _context.Enderecos
                .Where(e => e.id_cliente == clienteId)
                .OrderByDescending(e => e.principal)
                .ThenBy(e => e.identificacao)
                .ToListAsync();

            ViewBag.Enderecos = enderecos;

            await CarregarDadosCupom(itens);

            return View("~/Views/Pedido/Confirmar.cshtml", itens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(uint id, string? origem)
        {
            bool veioDosFavoritos = origem == "Favoritos";
            bool veioDoExplorar = origem == "Explorar";

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.id_produto == id && p.ativo);

            if (produto == null)
                return NotFound();

            if (produto.estoque == 0)
            {
                TempData["MensagemCarrinho"] =
                    "Este produto está sem estoque no momento.";

                if (veioDosFavoritos)
                    return RedirectToAction("Index", "Favorito");

                if (veioDoExplorar)
                    return RedirectToAction("Index", "Explorar");

                return RedirectToAction("Detalhes", "Home", new { id });
            }

            var itens = _carrinho.ObterItens();
            var item = itens.FirstOrDefault(i => i.IdProduto == id);

            if (item != null)
            {
                if ((uint)item.Quantidade >= produto.estoque)
                {
                    TempData["MensagemCarrinho"] =
                        "Você já atingiu o estoque disponível deste produto.";

                    if (veioDosFavoritos)
                        return RedirectToAction("Index", "Favorito");

                    if (veioDoExplorar)
                        return RedirectToAction("Index", "Explorar");

                    return RedirectToAction("Detalhes", "Home", new { id });
                }

                item.Quantidade++;
            }
            else
            {
                itens.Add(new ItemCarrinho
                {
                    IdProduto = produto.id_produto,
                    Nome = produto.nome,
                    ImagemUrl = produto.imagem_url,
                    Preco = produto.preco,
                    Quantidade = 1
                });
            }

            _carrinho.SalvarItens(itens);

            TempData["MensagemCarrinho"] =
                "Produto adicionado ao carrinho!";

            if (veioDosFavoritos)
                return RedirectToAction("Index", "Favorito");

            if (veioDoExplorar)
                return RedirectToAction("Index", "Explorar");

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remover(uint id)
        {
            var itens = _carrinho.ObterItens();

            itens.RemoveAll(item => item.IdProduto == id);

            _carrinho.SalvarItens(itens);

            TempData["MensagemCarrinho"] =
                "Produto removido do carrinho.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarQuantidade(
            uint id,
            int quantidade)
        {
            var itens = _carrinho.ObterItens();
            var item = itens.FirstOrDefault(i => i.IdProduto == id);

            if (item == null)
                return RedirectToAction("Index");

            if (quantidade <= 0)
            {
                itens.Remove(item);
            }
            else
            {
                var produto = await _context.Produtos
                    .FirstOrDefaultAsync(p => p.id_produto == id && p.ativo);

                if (produto == null)
                {
                    itens.Remove(item);
                }
                else if ((uint)quantidade > produto.estoque)
                {
                    TempData["MensagemCarrinho"] =
                        "A quantidade solicitada ultrapassa o estoque disponível.";

                    return RedirectToAction("Index");
                }
                else
                {
                    item.Quantidade = quantidade;
                }
            }

            _carrinho.SalvarItens(itens);

            return RedirectToAction("Index");
        }
    }
}
