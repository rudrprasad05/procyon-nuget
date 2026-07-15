using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Procyon.Email.DependencyInjection;

internal sealed class ProcyonEmailBuilder(
    IServiceCollection services,
    IConfiguration configuration) : IProcyonEmailBuilder
{
    public IServiceCollection Services { get; } = services;

    public IConfiguration Configuration { get; } = configuration;
}
