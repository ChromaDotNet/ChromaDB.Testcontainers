namespace Testcontainers.Chroma;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
[PublicAPI]
public sealed class ChromaBuilder : ContainerBuilder<ChromaBuilder, ChromaContainer, ChromaConfiguration>
{
    [Obsolete("This constant is obsolete and will be removed in the future. Use the constructor with the image parameter instead: https://github.com/testcontainers/testcontainers-dotnet/discussions/1470#discussioncomment-15185721.")]
    public const string ChromaImage = "qdrant/qdrant:v1.13.4";

    public const ushort ChromaHttpPort = 6333;

    public const ushort ChromaGrpcPort = 6334;

    public const string CertificateFilePath = "/qdrant/tls/cert.pem";

    public const string CertificateKeyFilePath = "/qdrant/tls/key.pem";

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromaBuilder" /> class.
    /// </summary>
    [Obsolete("This parameterless constructor is obsolete and will be removed. Use the constructor with the image parameter instead: https://github.com/testcontainers/testcontainers-dotnet/discussions/1470#discussioncomment-15185721.")]
    [ExcludeFromCodeCoverage]
    public ChromaBuilder()
        : this(ChromaImage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromaBuilder" /> class.
    /// </summary>
    /// <param name="image">
    /// The full Docker image name, including the image repository and tag
    /// (e.g., <c>qdrant/qdrant:v1.13.4</c>).
    /// </param>
    /// <remarks>
    /// Docker image tags available at <see href="https://hub.docker.com/r/qdrant/qdrant/tags" />.
    /// </remarks>
    public ChromaBuilder(string image)
        : this(new DockerImage(image))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromaBuilder" /> class.
    /// </summary>
    /// <param name="image">
    /// An <see cref="IImage" /> instance that specifies the Docker image to be used
    /// for the container builder configuration.
    /// </param>
    /// <remarks>
    /// Docker image tags available at <see href="https://hub.docker.com/r/qdrant/qdrant/tags" />.
    /// </remarks>
    public ChromaBuilder(IImage image)
        : this(new ChromaConfiguration())
    {
        DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromaBuilder" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    private ChromaBuilder(ChromaConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override ChromaConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Sets the API key to secure the instance.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>A configured instance of <see cref="ChromaBuilder" />.</returns>
    public ChromaBuilder WithApiKey(string apiKey)
    {
        return Merge(DockerResourceConfiguration, new ChromaConfiguration(apiKey: apiKey))
            .WithEnvironment("QDRANT__SERVICE__API_KEY", apiKey);
    }

    /// <summary>
    /// Sets the public certificate and private key to enable TLS.
    /// </summary>
    /// <param name="certificate">The public certificate in PEM format.</param>
    /// <param name="certificateKey">The private key associated with the certificate in PEM format.</param>
    /// <returns>A configured instance of <see cref="ChromaBuilder" />.</returns>
    public ChromaBuilder WithCertificate(string certificate, string certificateKey)
    {
        return Merge(DockerResourceConfiguration, new ChromaConfiguration(certificate: certificate, certificateKey: certificateKey))
            .WithEnvironment("QDRANT__SERVICE__ENABLE_TLS", "1")
            .WithEnvironment("QDRANT__TLS__CERT", CertificateFilePath)
            .WithEnvironment("QDRANT__TLS__KEY", CertificateKeyFilePath)
            .WithResourceMapping(Encoding.Default.GetBytes(certificate), CertificateFilePath)
            .WithResourceMapping(Encoding.Default.GetBytes(certificateKey), CertificateKeyFilePath);
    }

    /// <inheritdoc />
    public override ChromaContainer Build()
    {
        Validate();

        // By default, the base builder waits until the container is running. However, for Qdrant, a more advanced waiting strategy is necessary that requires access to the configured certificate.
        // If the user does not provide a custom waiting strategy, append the default Qdrant waiting strategy.
        var chromaBuilder = DockerResourceConfiguration.WaitStrategies.Count() > 1 ? this : WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(new WaitUntil(DockerResourceConfiguration)));
        return new ChromaContainer(chromaBuilder.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override ChromaBuilder Init()
    {
        return base.Init()
            .WithPortBinding(ChromaHttpPort, true)
            .WithPortBinding(ChromaGrpcPort, true)
            .WithConnectionStringProvider(new ChromaConnectionStringProvider());
    }

    /// <inheritdoc />
    protected override ChromaBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new ChromaConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override ChromaBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new ChromaConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override ChromaBuilder Merge(ChromaConfiguration oldValue, ChromaConfiguration newValue)
    {
        return new ChromaBuilder(new ChromaConfiguration(oldValue, newValue));
    }

    /// <inheritdoc cref="IWaitUntil" />
    private sealed class WaitUntil : IWaitUntil
    {
        private readonly bool _tlsEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitUntil" /> class.
        /// </summary>
        /// <param name="configuration">The container configuration.</param>
        public WaitUntil(ChromaConfiguration configuration)
        {
            _tlsEnabled = configuration.TlsEnabled;
        }

        /// <inheritdoc />
        public async Task<bool> UntilAsync(IContainer container)
        {
            using var httpMessageHandler = new HttpClientHandler();
            httpMessageHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

            var httpWaitStrategy = new HttpWaitStrategy()
                .UsingHttpMessageHandler(httpMessageHandler)
                .UsingTls(_tlsEnabled)
                .ForPort(ChromaHttpPort)
                .ForPath("/readyz");

            return await httpWaitStrategy.UntilAsync(container)
                .ConfigureAwait(false);
        }
    }
}