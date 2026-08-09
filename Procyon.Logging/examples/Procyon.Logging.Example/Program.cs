using Procyon.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProcyonLogging(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseStaticFiles();
app.UseProcyonLogging();
app.UseProcyonLoggingUi();

var logUiPath = builder.Configuration["Procyon:Logging:Web:Path"] ?? "/procyon/logging";
app.MapGet("/", () => Results.Redirect(logUiPath.StartsWith('/') ? logUiPath : "/" + logUiPath));
app.MapControllers();

app.Run();
