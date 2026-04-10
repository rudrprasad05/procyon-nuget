using Microsoft.Extensions.Options;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Options;

namespace Procyon.Media.Resolvers;

public class DefaultMediaUrlResolver : IMediaUrlResolver
{
    private readonly MediaOptions _options;

    public DefaultMediaUrlResolver(IOptions<MediaOptions> options)
    {
        _options = options.Value;
    }

    public string Resolve(string key)
    {
        return $"{_options.BaseUrl.TrimEnd('/')}/{key}";
    }
}