using SGIF.Classes;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "Configuration/SGIFBD.json",
    optional: false,
    reloadOnChange: true
);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Sessão (guarda quem está logado, como no GRMP)
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Classes de acesso a dados
builder.Services.AddScoped<Usuarios>();
builder.Services.AddScoped<Trens>();
builder.Services.AddScoped<Locomotivas>();
builder.Services.AddScoped<Vagoes>();
builder.Services.AddScoped<Sensores>();
builder.Services.AddScoped<Leituras>();
builder.Services.AddScoped<Alertas>();

var app = builder.Build();

// Cria o administrador inicial se a tabela Usuarios estiver vazia
DadosIniciais.CriarAdministrador(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=LoginExibir}/{id?}")
    .WithStaticAssets();

app.Run();