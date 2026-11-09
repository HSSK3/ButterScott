
using ButterScott.Data;
using Microsoft.EntityFrameworkCore;
using ButterScott.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configura o acesso ao banco de dados MySQL

builder.Services.AddDbContext<ButterScottContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("ButterScottConnection")
        ?? throw new InvalidOperationException(
            "A conexão ButterScottConnection não foi configurada."
        )
    ));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CarrinhoService>();

// Configura o MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configura o pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
