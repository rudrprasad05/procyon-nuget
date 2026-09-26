using Microsoft.EntityFrameworkCore;
using Procyon.Media.Azure;
using Procyon.Example.Azure.Data;
using Procyon.Media;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Media
builder.Services.AddProcyonMedia(builder.Configuration);
builder.Services.AddAzureMediaProvider(builder.Configuration);


// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
