using EduConnect.Components;
using EduConnect.Data;
using EduConnect.Interfaces;
using EduConnect.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure EF Core with SQLite (local DB file)
builder.Services.AddDbContext<EduConnectContext>(options =>
    options.UseSqlite("Data Source=educonnect.db"));

// Custom services
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IGradeService, GradeService>();

var app = builder.Build();

// Ensure database and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EduConnectContext>();
    DbSeeder.EnsureSeedData(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
