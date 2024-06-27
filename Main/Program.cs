using Main.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Main.Repos;
using Main.Models.Domain;
using Main.Repos.Concrete;

var builder = WebApplication.CreateBuilder(args);

var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(identityConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<AppUser>>();

builder.Services.AddScoped<ITagRepo, TagRepo>();
builder.Services.AddScoped<IBlogPostRepo, BlogPostRepo>();
builder.Services.AddScoped<IImageRepo, ImagesRepo>();
builder.Services.AddScoped<ILikeRepo, LikeRepo>();
builder.Services.AddScoped<ICommentRepo, CommentRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddHttpContextAccessor();




var services = builder.Services;

var app = builder.Build();

CreateRoles(services.BuildServiceProvider()).Wait();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();


app.UseSession();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Main}/{action=MainPage}/{id?}");

app.MapControllerRoute(
    name: "login",
    pattern: "/Account/Login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "blogDetails",
    pattern: "Blogs/Details/{id}",
    defaults: new { controller = "Blogs", action = "Details" }
);
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();

CreateRoles(services.BuildServiceProvider()).Wait();


async Task CreateRoles(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string[] roleNames = { "Admin", "User", "Head Admin" };

    foreach (var roleName in roleNames)
    {
        if (!(await roleManager.RoleExistsAsync(roleName)))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    if ((await roleManager.FindByNameAsync("Head Admin")) != null)
    {
        var headAdminUser = new AppUser
        {
            UserName = "head_admin",
            NormalizedUserName = "head_admin".ToUpper(),
            Email = "head_admin@bytehub.com",
            NormalizedEmail = "head_admin@bytehub.com".ToUpper(),
            Id = "01d4524f-5d84-42a6-9319-70271d7845cd",
        };

        headAdminUser.PasswordHash = new PasswordHasher<AppUser>().HashPassword(headAdminUser, "Prishtina34#$");

        if ((await userManager.FindByEmailAsync("head_admin@bytehub.com")) == null)
        {
            var result = await userManager.CreateAsync(headAdminUser);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(headAdminUser, "Head Admin");
            }
        }
    }
}
