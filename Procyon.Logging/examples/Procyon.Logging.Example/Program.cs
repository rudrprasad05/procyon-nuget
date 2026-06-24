using Procyon.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProcyonLogging(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseProcyonLogging();

app.MapGet("/", () => Results.Redirect("/procyon/logs"));
app.MapControllers();

app.Run();
