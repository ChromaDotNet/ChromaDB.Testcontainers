namespace Testcontainers.Chroma;

/// <inheritdoc cref="DockerContainer" />
[PublicAPI]
public sealed class ChromaContainer : DockerContainer
{
    private readonly ChromaConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromaContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public ChromaContainer(ChromaConfiguration configuration)
        : base(configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the connection string for connecting to Qdrant REST APIs.
    /// </summary>
    public string GetHttpConnectionString()
    {
        var scheme = _configuration.TlsEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        var endpoint = new UriBuilder(scheme, Hostname, GetMappedPublicPort(ChromaBuilder.ChromaHttpPort));
        return endpoint.ToString();
    }

    /// <summary>
    /// Gets the connection string for connecting to Qdrant gRPC APIs.
    /// </summary>
    public string GetGrpcConnectionString()
    {
        var scheme = _configuration.TlsEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        var endpoint = new UriBuilder(scheme, Hostname, GetMappedPublicPort(ChromaBuilder.ChromaGrpcPort));
        return endpoint.ToString();
    }
}