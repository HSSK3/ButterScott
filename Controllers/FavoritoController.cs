
using ButterScott.Data;
using ButterScott.Models;
using ButterScott.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    public class FavoritoController : Controller
    {
        private readonly ButterScottContext _context;
        private readonly CarrinhoService _carrinho;

        public FavoritoController(
            ButterScottContext context,
            CarrinhoService carrinho)
        {
            _context = context;
            _carrinho = carrinho;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clienteId = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteId, out uint idCliente))
            {
                return RedirectToAction("Login", "Conta");
            }

            var favoritos = await _context.Favoritos
                .Where(f => f.id_cliente == idCliente)
                .Join(
                    _context.Produtos,
                    favorito => favorito.id_produto,
                    produto => produto.id_produto,
                    (favorito, produto) => produto)
                .ToListAsync();

            ViewBag.ProdutosNoCarrinho = _carrinho
                .ObterItens()
                .Select(item => item.IdProduto)
                .ToHashSet();

            return View(favoritos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(uint idProduto)
        {
            var clienteId = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteId, out uint idCliente))
            {
                return RedirectToAction("Login", "Conta");
            }

            var produtoExiste = await _context.Produtos
                .AnyAsync(p => p.id_produto == idProduto && p.ativo);

            if (!produtoExiste)
            {
                TempData["ErroFavorito"] = "Produto não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            bool jaExiste = await _context.Favoritos
                .AnyAsync(f =>
                    f.id_cliente == idCliente &&
                    f.id_produto == idProduto);

            if (!jaExiste)
            {
                _context.Favoritos.Add(new Favorito
                {
                    id_cliente = idCliente,
                    id_produto = idProduto,
                    data_adicao = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            TempData["MensagemFavorito"] =
                "Produto adicionado aos favoritos!";

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(uint idProduto)
        {
            var clienteId = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteId, out uint idCliente))
            {
                return RedirectToAction("Login", "Conta");
            }

            var favorito = await _context.Favoritos
                .FirstOrDefaultAsync(f =>
                    f.id_cliente == idCliente &&
                    f.id_produto == idProduto);

            if (favorito != null)
            {
                _context.Favoritos.Remove(favorito);
                await _context.SaveChangesAsync();
            }

            TempData["MensagemFavorito"] =
                "Produto removido dos favoritos.";

            return RedirectToAction(nameof(Index));
        }
    }
}
