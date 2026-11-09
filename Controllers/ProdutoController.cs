
using ButterScott.Data;
using ButterScott.Filters;
using ButterScott.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    [AdminAuthorize]
    public class ProdutoController : Controller
    {
        private readonly ButterScottContext _context;

        public ProdutoController(ButterScottContext context)
        {
            _context = context;
        }

        // Lista todos os produtos
        public async Task<IActionResult> Index()
        {
            var produtos = await _context.Produtos
                .Join(
                    _context.Categorias,
                    produto => produto.id_categoria,
                    categoria => categoria.id_categoria,
                    (produto, categoria) => new
                    {
                        Produto = produto,
                        CategoriaNome = categoria.nome
                    })
                .OrderBy(p => p.Produto.nome)
                .ToListAsync();

            return View(produtos);
        }

        // Exibe o formulário de cadastro
        public async Task<IActionResult> Criar()
        {
            await CarregarCategorias();
            return View();
        }

        // Recebe o cadastro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Produto produto)
        {
            produto.nome = produto.nome.Trim();
            produto.sabor = produto.sabor.Trim();

            if (string.IsNullOrWhiteSpace(produto.nome) ||
                produto.nome.Length > 120)
            {
                ModelState.AddModelError(
                    "nome",
                    "Informe um nome válido de até 120 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(produto.sabor) ||
                produto.sabor.Length > 80)
            {
                ModelState.AddModelError(
                    "sabor",
                    "Informe um sabor válido de até 80 caracteres.");
            }

            if (produto.preco <= 0)
            {
                ModelState.AddModelError(
                    "preco",
                    "O preço deve ser maior que zero.");
            }

            if (produto.estoque < 0)
            {
                ModelState.AddModelError(
                    "estoque",
                    "O estoque não pode ser negativo.");
            }

            bool categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.id_categoria == produto.id_categoria
                            && c.ativa);

            if (!categoriaExiste)
            {
                ModelState.AddModelError(
                    "id_categoria",
                    "Selecione uma categoria válida e ativa.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarCategorias();
                return View(produto);
            }

            produto.ativo = true;
            produto.data_criacao = DateTime.Now;
            produto.data_atualizacao = DateTime.Now;

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            TempData["SucessoProduto"] =
                "Produto cadastrado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // Exibe o formulário de edição
        public async Task<IActionResult> Editar(uint id)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.id_produto == id);

            if (produto == null)
            {
                return NotFound();
            }

            await CarregarCategorias(produto.id_categoria);

            return View(produto);
        }

        // Recebe as alterações
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(uint id, Produto produto)
        {
            if (id != produto.id_produto)
            {
                return BadRequest();
            }

            var produtoBanco = await _context.Produtos
                .FirstOrDefaultAsync(p => p.id_produto == id);

            if (produtoBanco == null)
            {
                return NotFound();
            }

            produto.nome = produto.nome.Trim();
            produto.sabor = produto.sabor.Trim();

            if (string.IsNullOrWhiteSpace(produto.nome) ||
                produto.nome.Length > 120)
            {
                ModelState.AddModelError(
                    "nome",
                    "Informe um nome válido de até 120 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(produto.sabor) ||
                produto.sabor.Length > 80)
            {
                ModelState.AddModelError(
                    "sabor",
                    "Informe um sabor válido de até 80 caracteres.");
            }

            if (produto.preco <= 0)
            {
                ModelState.AddModelError(
                    "preco",
                    "O preço deve ser maior que zero.");
            }

            bool categoriaValida = await _context.Categorias
                .AnyAsync(c =>
                    c.id_categoria == produto.id_categoria &&
                    (c.ativa ||
                     c.id_categoria == produtoBanco.id_categoria));

            if (!categoriaValida)
            {
                ModelState.AddModelError(
                    "id_categoria",
                    "Selecione uma categoria ativa ou mantenha a categoria atual.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarCategorias(produtoBanco.id_categoria);
                return View(produto);
            }

            produtoBanco.id_categoria = produto.id_categoria;
            produtoBanco.nome = produto.nome;
            produtoBanco.descricao = produto.descricao;
            produtoBanco.sabor = produto.sabor;
            produtoBanco.preco = produto.preco;
            produtoBanco.estoque = produto.estoque;
            produtoBanco.imagem_url = produto.imagem_url;
            produtoBanco.ativo = produto.ativo;

            await _context.SaveChangesAsync();

            TempData["SucessoProduto"] =
                "Produto atualizado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // Ativa ou desativa um produto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(uint id)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.id_produto == id);

            if (produto == null)
            {
                return NotFound();
            }

            produto.ativo = !produto.ativo;

            await _context.SaveChangesAsync();

            TempData["SucessoProduto"] = produto.ativo
                ? "Produto ativado com sucesso!"
                : "Produto desativado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // Carrega categorias ativas e, opcionalmente, a categoria atual
        private async Task CarregarCategorias(uint? categoriaAtualId = null)
        {
            ViewBag.Categorias = await _context.Categorias
                .Where(c => c.ativa ||
                            c.id_categoria == categoriaAtualId)
                .OrderBy(c => c.nome)
                .ToListAsync();
        }
    }
}
