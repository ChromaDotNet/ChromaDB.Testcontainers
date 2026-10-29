namespace Testcontainers.Chroma;

/// <summary>
/// The overload of <c>WithConnectionStringProvider</c> that takes a factory, which the module calls.
/// </summary>
/// <remarks>
/// testcontainers-dotnet added it after Testcontainers 4.16.0, so the package of Testcontainers this module builds
/// against doesn't have it yet. Until a release has it, this extension takes its place: it creates the provider once,
/// when the builder is initialized, as the overload that takes an instance does. Remove it with the first release that
/// has the overload, which the module then calls instead.
/// </remarks>
internal static class ChromaBuilderExtensions
{
    public static ChromaBuilder WithConnectionStringProvider(this ChromaBuilder builder, Func<IConnectionStringProvider<ChromaContainer, ChromaConfiguration>> connectionStringProviderFactory)
    {
        return builder.WithConnectionStringProvider(connectionStringProviderFactory());
    }
}
