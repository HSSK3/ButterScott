using ButterScott.Data;
using ButterScott.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    public class EnderecoController : Controller
    {
        private readonly ButterScottContext _context;

        public EnderecoController(ButterScottContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(bool continuarCompra = false)
        {
            var clienteIdTexto = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteIdTexto, out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para gerenciar seus endereços.";

                return RedirectToAction("Login", "Conta");
            }

            var enderecos = await _context.Enderecos
                .Where(e => e.id_cliente == clienteId)
                .OrderByDescending(e => e.principal)
                .ThenBy(e => e.identificacao)
                .ToListAsync();

            ViewBag.ContinuarCompra = continuarCompra;

            return View(enderecos);
        }

        public IActionResult Criar(bool continuarCompra = false)
        {
            if (!uint.TryParse(
                HttpContext.Session.GetString("ClienteId"),
                out _))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para cadastrar um endereço.";

                return RedirectToAction("Login", "Conta");
            }

            ViewBag.ContinuarCompra = continuarCompra;

            return View(new Endereco());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(
            Endereco endereco,
            bool continuarCompra = false)
        {
            if (!uint.TryParse(
                HttpContext.Session.GetString("ClienteId"),
                out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para cadastrar um endereço.";

                return RedirectToAction("Login", "Conta");
            }

            ModelState.Remove(nameof(Endereco.id_cliente));
            ModelState.Remove(nameof(Endereco.id_endereco));

            if (!ModelState.IsValid)
            {
                ViewBag.ContinuarCompra = continuarCompra;

                return View(endereco);
            }

            endereco.id_cliente = clienteId;

            // O primeiro endereço cadastrado será definido como principal.
            bool primeiroEndereco = !await _context.Enderecos
                .AnyAsync(e => e.id_cliente == clienteId);

            if (primeiroEndereco)
            {
                endereco.principal = true;
            }

            _context.Enderecos.Add(endereco);
            await _context.SaveChangesAsync();

            TempData["MensagemEndereco"] =
                "Endereço cadastrado com sucesso!";

            return RedirectToAction(
                nameof(Index),
                new { continuarCompra });
        }

        public async Task<IActionResult> Editar(uint id)
        {
            if (!uint.TryParse(
                HttpContext.Session.GetString("ClienteId"),
                out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para editar um endereço.";

                return RedirectToAction("Login", "Conta");
            }

            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e =>
                    e.id_endereco == id &&
                    e.id_cliente == clienteId);

            if (endereco == null)
            {
                return NotFound();
            }

            return View(endereco);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarEdicao(
            uint id,
            Endereco endereco)
        {
            if (!uint.TryParse(
                HttpContext.Session.GetString("ClienteId"),
                out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para editar um endereço.";

                return RedirectToAction("Login", "Conta");
            }

            var enderecoExistente = await _context.Enderecos
                .FirstOrDefaultAsync(e =>
                    e.id_endereco == id &&
                    e.id_cliente == clienteId);

            if (enderecoExistente == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Endereco.id_cliente));
            ModelState.Remove(nameof(Endereco.id_endereco));

            if (!ModelState.IsValid)
            {
                endereco.id_endereco = id;
                return View("Editar", endereco);
            }

            enderecoExistente.identificacao = endereco.identificacao;
            enderecoExistente.rua = endereco.rua;
            enderecoExistente.numero = endereco.numero;
            enderecoExistente.complemento = endereco.complemento;
            enderecoExistente.bairro = endereco.bairro;
            enderecoExistente.cidade = endereco.cidade;
            enderecoExistente.estado = endereco.estado;
            enderecoExistente.cep = endereco.cep;

            await _context.SaveChangesAsync();

            TempData["MensagemEndereco"] =
                "Endereço atualizado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(uint id)
        {
            if (!uint.TryParse(
                HttpContext.Session.GetString("ClienteId"),
                out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para gerenciar seus endereços.";

                return RedirectToAction("Login", "Conta");
            }

            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e =>
                    e.id_endereco == id &&
                    e.id_cliente == clienteId);

            if (endereco == null)
            {
                return NotFound();
            }

            if (endereco.principal)
            {
                TempData["MensagemEndereco"] =
                    "Defina outro endereço como principal antes de excluir este.";

                return RedirectToAction(nameof(Index));
            }

            _context.Enderecos.Remove(endereco);
            await _context.SaveChangesAsync();

            TempData["MensagemEndereco"] =
                "Endereço excluído com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DefinirPrincipal(uint id)
        {
            var clienteIdTexto = HttpContext.Session.GetString("ClienteId");

            if (!uint.TryParse(clienteIdTexto, out uint clienteId))
            {
                TempData["MensagemConta"] =
                    "Entre na sua conta para gerenciar seus endereços.";

                return RedirectToAction("Login", "Conta");
            }

            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e =>
                    e.id_endereco == id &&
                    e.id_cliente == clienteId);

            if (endereco == null)
            {
                return NotFound();
            }

            var enderecos = await _context.Enderecos
                .Where(e => e.id_cliente == clienteId)
                .ToListAsync();

            foreach (var item in enderecos)
            {
                item.principal = item.id_endereco == endereco.id_endereco;
            }

            await _context.SaveChangesAsync();

            TempData["MensagemEndereco"] =
                "Endereço principal atualizado com sucesso!";

            return RedirectToAction(nameof(Index));
        }
    }
}