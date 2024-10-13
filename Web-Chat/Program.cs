using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Web_Chat.Models;
using Web_Chat.Services;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddJsonOptions(options => {
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddScoped<AppTheme>();

builder.Services.AddDbContext<Web_Chat.Models.DBContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<LocalUploadedFileStorage>(x =>
{
    var env = x.GetRequiredService<IWebHostEnvironment>();
    return new LocalUploadedFileStorage(Path.Combine(env.WebRootPath, "uploads", "images"));
});

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequiredLength = 5;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
}).AddRoles<IdentityRole<int>>()
  .AddEntityFrameworkStores<Web_Chat.Models.DBContext>()
  .AddDefaultTokenProviders();


builder.Services.AddSingleton<ChatHandler>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "WebChat.AuthCookie";
    options.Cookie.Path = "/";
    options.Cookie.Domain = null;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWebSockets();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var chatHandler = new ChatHandler();
        await ChatHandler.WebSocketRequest(webSocket);
    }
    else
    {
        await next();
    }
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
