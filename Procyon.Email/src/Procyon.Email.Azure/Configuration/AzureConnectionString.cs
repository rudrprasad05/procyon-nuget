namespace Procyon.Email.Azure.Configuration;

internal sealed record AzureConnectionString(Uri Endpoint, string AccessKey)
{
    public static AzureConnectionString? TryParse(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var values = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

        if (!values.TryGetValue("Endpoint", out var endpoint) ||
            !Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) ||
            endpointUri.Scheme is not ("http" or "https") ||
            !values.TryGetValue("AccessKey", out var accessKey) ||
            string.IsNullOrWhiteSpace(accessKey))
        {
            return null;
        }

        return new AzureConnectionString(endpointUri, accessKey);
    }
}
