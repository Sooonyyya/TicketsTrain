using TicketsTrainInfrastructure;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;
using TicketsTrainInfrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TicketsTrainContext>(option => option.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

// Реєстрація сервісів імпорту/експорту - переміщено сюди
builder.Services.AddScoped<IDataPortServiceFactory<Ticket>, TicketDataPortServiceFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=TicketMaster}/{id?}")
    .WithStaticAssets();

app.Run();