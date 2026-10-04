namespace Testcontainers.Chroma;

/// <summary>
/// Provides the Qdrant connection string.
/// </summary>
internal sealed class ChromaConnectionStringProvider : ContainerConnectionStringProvider<ChromaContainer, ChromaConfiguration>
{
    /// <inheritdoc />
    protected override string GetHostConnectionString()
    {
        return Container.GetHttpConnectionString();
    }
}