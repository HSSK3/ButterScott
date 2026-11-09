
using ButterScott.Data;
using ButterScott.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers
{
    public class ContaController : Controller
    {
        private readonly ButterScottContext _context;
        private readonly PasswordHasher<Cliente> _passwordHasher;

        public ContaController(ButterScottContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Cliente>();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha)
        {
            email = email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                ModelState.AddModelError(
                    "",
                    "Informe o e-mail e a senha.");

                return View();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.email == email && c.ativo);

            if (cliente == null)
            {
                ModelState.AddModelError(
                    "",
                    "E-mail ou senha incorretos.");

                return View();
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                cliente,
                cliente.senha_hash,
                senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "E-mail ou senha incorretos.");

                return View();
            }

            HttpContext.Session.SetString(
                "ClienteId",
                cliente.id_cliente.ToString());

            HttpContext.Session.SetString(
                "ClienteNome",
                cliente.nome);

            HttpContext.Session.SetString(
                "EhAdmin",
                cliente.eh_admin ? "true" : "false");

            TempData["MensagemConta"] =
                "Login realizado com sucesso! Bem-vindo, " + cliente.nome + ".";

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(
            string nome,
            string email,
            string senha,
            string? telefone)
        {
            nome = nome?.Trim() ?? "";
            email = email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 120)
            {
                ModelState.AddModelError(
                    "nome",
                    "Informe um nome com até 120 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(email) || email.Length > 180)
            {
                ModelState.AddModelError(
                    "email",
                    "Informe um e-mail válido com até 180 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
            {
                ModelState.AddModelError(
                    "senha",
                    "A senha deve ter pelo menos 8 caracteres.");
            }

            if (telefone?.Length > 20)
            {
                ModelState.AddModelError(
                    "telefone",
                    "O telefone deve ter até 20 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            bool emailExiste = await _context.Clientes
                .AnyAsync(c => c.email == email);

            if (emailExiste)
            {
                ModelState.AddModelError(
                    "email",
                    "Este e-mail já está cadastrado.");

                return View();
            }

            var cliente = new Cliente
            {
                nome = nome,
                email = email,
                telefone = string.IsNullOrWhiteSpace(telefone)
                    ? null
                    : telefone.Trim(),
                data_cadastro = DateTime.Now,
                ativo = true
            };

            cliente.senha_hash = _passwordHasher.HashPassword(
                cliente,
                senha);

            _context.Clientes.Add(cliente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "email",
                    "Não foi possível concluir o cadastro. Verifique se o e-mail já está cadastrado.");

                return View();
            }

            TempData["MensagemConta"] =
                "Cadastro realizado com sucesso! Você já pode entrar.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var clienteId = HttpContext.Session.GetString("ClienteId");

            if (!int.TryParse(clienteId, out int id))
            {
                return RedirectToAction(nameof(Login));
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.id_cliente == id);

            if (cliente == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarPerfil(
            string nome,
            string email,
            string? telefone)
        {
            var clienteId = HttpContext.Session.GetString("ClienteId");

            if (!int.TryParse(clienteId, out int id))
            {
                return RedirectToAction(nameof(Login));
            }

            nome = nome?.Trim() ?? "";
            email = email?.Trim() ?? "";
            telefone = telefone?.Trim();

            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 120)
            {
                TempData["ErroPerfil"] =
                    "Informe um nome com até 120 caracteres.";

                return RedirectToAction(nameof(Perfil));
            }

            if (string.IsNullOrWhiteSpace(email) || email.Length > 180)
            {
                TempData["ErroPerfil"] =
                    "Informe um e-mail válido com até 180 caracteres.";

                return RedirectToAction(nameof(Perfil));
            }

            if (telefone?.Length > 20)
            {
                TempData["ErroPerfil"] =
                    "O telefone deve ter até 20 caracteres.";

                return RedirectToAction(nameof(Perfil));
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.id_cliente == id);

            if (cliente == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            bool emailExiste = await _context.Clientes
                .AnyAsync(c =>
                    c.email == email &&
                    c.id_cliente != id);

            if (emailExiste)
            {
                TempData["ErroPerfil"] =
                    "Este e-mail já está sendo utilizado por outra conta.";

                return RedirectToAction(nameof(Perfil));
            }

            cliente.nome = nome;
            cliente.email = email;
            cliente.telefone = string.IsNullOrWhiteSpace(telefone)
                ? null
                : telefone;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErroPerfil"] =
                    "Não foi possível atualizar seus dados. Verifique as informações.";

                return RedirectToAction(nameof(Perfil));
            }

            HttpContext.Session.SetString("ClienteNome", cliente.nome);

            TempData["SucessoPerfil"] =
                "Seus dados foram atualizados com sucesso!";

            return RedirectToAction(nameof(Perfil));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(
            string senhaAtual,
            string novaSenha,
            string confirmarSenha)
        {
            var clienteId = HttpContext.Session.GetString("ClienteId");

            if (!int.TryParse(clienteId, out int id))
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrWhiteSpace(senhaAtual) ||
                string.IsNullOrWhiteSpace(novaSenha) ||
                string.IsNullOrWhiteSpace(confirmarSenha))
            {
                TempData["ErroSenha"] =
                    "Preencha todos os campos de senha.";

                return RedirectToAction(nameof(Perfil));
            }

            if (novaSenha.Length < 8)
            {
                TempData["ErroSenha"] =
                    "A nova senha deve ter pelo menos 8 caracteres.";

                return RedirectToAction(nameof(Perfil));
            }

            if (novaSenha != confirmarSenha)
            {
                TempData["ErroSenha"] =
                    "A confirmação não corresponde à nova senha.";

                return RedirectToAction(nameof(Perfil));
            }

            if (senhaAtual == novaSenha)
            {
                TempData["ErroSenha"] =
                    "A nova senha deve ser diferente da senha atual.";

                return RedirectToAction(nameof(Perfil));
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.id_cliente == id);

            if (cliente == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                cliente,
                cliente.senha_hash,
                senhaAtual);

            if (resultado == PasswordVerificationResult.Failed)
            {
                TempData["ErroSenha"] =
                    "A senha atual está incorreta.";

                return RedirectToAction(nameof(Perfil));
            }

            cliente.senha_hash = _passwordHasher.HashPassword(
                cliente,
                novaSenha);

            await _context.SaveChangesAsync();

            TempData["SucessoSenha"] =
                "Sua senha foi alterada com sucesso!";

            return RedirectToAction(nameof(Perfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("ClienteId");
            HttpContext.Session.Remove("ClienteNome");
            HttpContext.Session.Remove("EhAdmin");

            TempData["MensagemConta"] =
                "Você saiu da sua conta com sucesso.";

            return RedirectToAction(nameof(Login));
        }
    }
}
