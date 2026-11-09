
using ButterScott.Data;
using ButterScott.Filters;
using ButterScott.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    [AdminAuthorize]
    public class CategoriaController : Controller
    {
        private readonly ButterScottContext _context;

        public CategoriaController(ButterScottContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .OrderBy(c => c.nome)
                .ToListAsync();

            return View(categorias);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Categoria categoria)
        {
            categoria.nome = categoria.nome.Trim();
            categoria.descricao = categoria.descricao?.Trim();

            if (string.IsNullOrWhiteSpace(categoria.nome))
            {
                ModelState.AddModelError("nome", "Informe o nome da categoria.");
            }
            else if (categoria.nome.Length > 80)
            {
                ModelState.AddModelError("nome", "O nome deve ter no máximo 80 caracteres.");
            }

            if (categoria.descricao?.Length > 255)
            {
                ModelState.AddModelError("descricao", "A descrição deve ter no máximo 255 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            categoria.ativa = true;

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Categoria criada com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(uint id)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.id_categoria == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(uint id, Categoria categoria)
        {
            if (id != categoria.id_categoria)
            {
                return NotFound();
            }

            categoria.nome = categoria.nome.Trim();
            categoria.descricao = categoria.descricao?.Trim();

            if (string.IsNullOrWhiteSpace(categoria.nome))
            {
                ModelState.AddModelError("nome", "Informe o nome da categoria.");
            }
            else if (categoria.nome.Length > 80)
            {
                ModelState.AddModelError("nome", "O nome deve ter no máximo 80 caracteres.");
            }

            if (categoria.descricao?.Length > 255)
            {
                ModelState.AddModelError("descricao", "A descrição deve ter no máximo 255 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var categoriaBanco = await _context.Categorias
                .FirstOrDefaultAsync(c => c.id_categoria == id);

            if (categoriaBanco == null)
            {
                return NotFound();
            }

            categoriaBanco.nome = categoria.nome;
            categoriaBanco.descricao = categoria.descricao;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Categoria atualizada com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(uint id)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.id_categoria == id);

            if (categoria == null)
            {
                return NotFound();
            }

            categoria.ativa = !categoria.ativa;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = categoria.ativa
                ? "Categoria ativada com sucesso!"
                : "Categoria desativada com sucesso!";

            return RedirectToAction(nameof(Index));
        }
    }
}
