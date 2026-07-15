using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Procyon.Email.DependencyInjection;

/// <summary>
/// Provides access to the Procyon.Email registration context used by provider packages.
/// </summary>
public interface IProcyonEmailBuilder
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the application configuration used for email setup.
    /// </summary>
    IConfiguration Configuration { get; }
}
