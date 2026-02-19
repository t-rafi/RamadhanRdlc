using Microsoft.EntityFrameworkCore;
using RamadanReportApp.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllersWithViews();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
