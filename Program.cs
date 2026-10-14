using FunktsiooniUurimineWeb.Data;
using FunktsiooniUurimineWeb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RakenduseKontekst>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Vaikimisi")));
builder.Services.AddScoped<FunktsiooniAnalyys>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Funktsioon/Viga");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RakenduseKontekst>();
    await db.Database.MigrateAsync();
    await Algandmed.LooAsync(db);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Funktsioon}/{action=Index}/{id?}");

app.Run();
