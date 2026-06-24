namespace Procyon.Logging.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class NoLogAttribute : Attribute
{
}
