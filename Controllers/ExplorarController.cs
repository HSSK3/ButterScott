
using ButterScott.Data;
using ButterScott.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    public class ExplorarController : Controller
    {
        private readonly ButterScottContext _context;
        private readonly CarrinhoService _carrinho;

        public ExplorarController(
            ButterScottContext context,
            CarrinhoService carrinho)
        {
            _context = context;
            _carrinho = carrinho;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? pesquisa,
            uint? categoriaId)
        {
            var consulta = _context.Produtos
                .Where(p => p.ativo);

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                consulta = consulta.Where(p =>
                    p.nome.Contains(pesquisa) ||
                    (p.descricao != null &&
                     p.descricao.Contains(pesquisa)) ||
                    p.sabor.Contains(pesquisa));
            }

            if (categoriaId.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.id_categoria == categoriaId.Value);
            }

            var produtos = await consulta
                .OrderBy(p => p.nome)
                .ToListAsync();

            var categorias = await _context.Categorias
                .Where(c => c.ativa)
                .OrderBy(c => c.nome)
                .ToListAsync();

            var produtosNoCarrinho = _carrinho.ObterItens()
                .Select(item => item.IdProduto)
                .ToHashSet();

            ViewBag.Categorias = categorias;
            ViewBag.Pesquisa = pesquisa;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.ProdutosNoCarrinho = produtosNoCarrinho;

            return View(produtos);
        }
    }
}
