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

// ��������� ������ �������/�������� - ��������� ����
builder.Services.AddScoped<IDataPortServiceFactory<Ticket>, TicketDataPortServiceFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

// ���� ���������� �������� �� ������� ����
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
