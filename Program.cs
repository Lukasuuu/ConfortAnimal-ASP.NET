using ConfortAnimal.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()                           // Adiciona suporte para gerenciamento de funções (roles) no Identity, permitindo que você atribua diferentes permissões e níveis de acesso aos usuários com base em suas funções.
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Configura o Identity para usar o ApplicationDbContext como o armazenamento de dados
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();


// Criar Admin no banco automaticamente
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(); // Obtém uma instância do UserManager
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(); // Obtém uma instância do RoleManager

    // Cria a Role Admin se não existir
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin")); // Cria a função "Admin" no banco de dados, se ela ainda não existir.
    }

    // Cria a Role Proprietario se não existir
    if (!await roleManager.RoleExistsAsync("Proprietario"))
    {
        await roleManager.CreateAsync(new IdentityRole("Proprietario")); // Cria a função "Proprietario" no banco de dados, se ela ainda não existir.
    }

    // Cria o utilizador Admin se não existir
    var adminEmail = "admin@confortanimal.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = "admin",     //--- o nome de utilizador (sem @)
            Email = adminEmail,     //--- o email
            EmailConfirmed = true   //--- confirma o email automaticamente
        };

        var createResult = await userManager.CreateAsync(adminUser, "Admin@123");
        
        if (createResult.Succeeded)
        {
            // Adiciona a Role Admin ao utilizador (com maiúscula, conforme criada)
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();

