
using ButterScott.Data;
using ButterScott.Filters;
using ButterScott.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    [AdminAuthorize]
    public class CupomController : Controller
    {
        private readonly ButterScottContext _context;

        public CupomController(ButterScottContext context)
        {
            _context = context;
        }

        // Lista todos os cupons
        public async Task<IActionResult> Index()
        {
            var cupons = await _context.Cupons
                .OrderBy(c => c.codigo)
                .ToListAsync();

            return View(cupons);
        }

        // Exibe o formulário de cadastro
        public IActionResult Criar()
        {
            return View();
        }

        // Recebe o cadastro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Cupom cupom)
        {
            cupom.codigo = (cupom.codigo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(cupom.codigo) ||
                cupom.codigo.Length > 40)
            {
                ModelState.AddModelError(
                    "codigo",
                    "Informe um código válido de até 40 caracteres.");
            }

            bool codigoExiste = await _context.Cupons
                .AnyAsync(c => c.codigo == cupom.codigo);

            if (codigoExiste)
            {
                ModelState.AddModelError(
                    "codigo",
                    "Já existe um cupom com este código.");
            }

            ValidarCupom(cupom);

            if (!ModelState.IsValid)
                return View(cupom);

            cupom.usos_realizados = 0;
            cupom.ativo = true;

            _context.Cupons.Add(cupom);
            await _context.SaveChangesAsync();

            TempData["SucessoCupom"] =
                "Cupom cadastrado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // Exibe o formulário de edição
        public async Task<IActionResult> Editar(uint id)
        {
            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(c => c.id_cupom == id);

            if (cupom == null)
                return NotFound();

            return View(cupom);
        }

        // Recebe as alterações
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(uint id, Cupom cupom)
        {
            if (id != cupom.id_cupom)
                return BadRequest();

            var cupomBanco = await _context.Cupons
                .FirstOrDefaultAsync(c => c.id_cupom == id);

            if (cupomBanco == null)
                return NotFound();

            cupom.codigo = (cupom.codigo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(cupom.codigo) ||
                cupom.codigo.Length > 40)
            {
                ModelState.AddModelError(
                    "codigo",
                    "Informe um código válido de até 40 caracteres.");
            }

            bool codigoExiste = await _context.Cupons
                .AnyAsync(c =>
                    c.codigo == cupom.codigo &&
                    c.id_cupom != id);

            if (codigoExiste)
            {
                ModelState.AddModelError(
                    "codigo",
                    "Já existe outro cupom com este código.");
            }

            ValidarCupom(cupom);

            if (!ModelState.IsValid)
                return View(cupom);

            cupomBanco.codigo = cupom.codigo;
            cupomBanco.descricao = cupom.descricao;
            cupomBanco.tipo_desconto = cupom.tipo_desconto;
            cupomBanco.valor_desconto = cupom.valor_desconto;
            cupomBanco.valor_minimo_pedido = cupom.valor_minimo_pedido;
            cupomBanco.data_inicio = cupom.data_inicio;
            cupomBanco.data_fim = cupom.data_fim;
            cupomBanco.limite_uso = cupom.limite_uso;

            await _context.SaveChangesAsync();

            TempData["SucessoCupom"] =
                "Cupom atualizado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // Ativa ou desativa um cupom
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(uint id)
        {
            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(c => c.id_cupom == id);

            if (cupom == null)
                return NotFound();

            cupom.ativo = !cupom.ativo;

            await _context.SaveChangesAsync();

            TempData["SucessoCupom"] = cupom.ativo
                ? "Cupom ativado com sucesso!"
                : "Cupom desativado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // Valida os dados do cupom
        private void ValidarCupom(Cupom cupom)
        {
            if (cupom.tipo_desconto != "Percentual" &&
                cupom.tipo_desconto != "ValorFixo")
            {
                ModelState.AddModelError(
                    "tipo_desconto",
                    "Selecione um tipo de desconto válido.");
            }

            if (cupom.valor_desconto <= 0)
            {
                ModelState.AddModelError(
                    "valor_desconto",
                    "O valor do desconto deve ser maior que zero.");
            }

            if (cupom.tipo_desconto == "Percentual" &&
                cupom.valor_desconto > 100)
            {
                ModelState.AddModelError(
                    "valor_desconto",
                    "O desconto percentual não pode ultrapassar 100%.");
            }

            if (cupom.valor_minimo_pedido < 0)
            {
                ModelState.AddModelError(
                    "valor_minimo_pedido",
                    "O pedido mínimo não pode ser negativo.");
            }

            if (cupom.data_inicio.HasValue &&
                cupom.data_fim.HasValue &&
                cupom.data_fim <= cupom.data_inicio)
            {
                ModelState.AddModelError(
                    "data_fim",
                    "A data final deve ser posterior à data inicial.");
            }
        }
    }
}
