
using System.Diagnostics;
using ButterScott.Data;
using ButterScott.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ButterScottContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        ButterScottContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var produtos = await _context.Produtos
            .Where(p => p.ativo)
            .ToListAsync();

        var clienteId = HttpContext.Session.GetString("ClienteId");

        uint.TryParse(clienteId, out uint idCliente);

        var produtosFavoritos = string.IsNullOrEmpty(clienteId)
            ? new HashSet<uint>()
            : (await _context.Favoritos
                .Where(f => f.id_cliente == idCliente)
                .Select(f => f.id_produto)
                .ToListAsync())
                .ToHashSet();

        var viewModel = produtos.Select(produto =>
            new ProdutoHomeViewModel
            {
                Produto = produto,
                EhFavorito = produtosFavoritos.Contains(produto.id_produto)
            }).ToList();

        return View(viewModel);
    }

    public async Task<IActionResult> Detalhes(uint id)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.id_produto == id && p.ativo);

        if (produto == null)
        {
            return NotFound();
        }

        return View(produto);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                ?? HttpContext.TraceIdentifier
        });
    }
}
