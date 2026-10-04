namespace Testcontainers.Chroma;

public sealed class ChromaSecureContainerTest : IAsyncLifetime
{
    private static readonly string ApiKey = Guid.NewGuid().ToString("D");

    private static readonly string CommonName = PemCertificate.Instance.CommonName;

    private static readonly string Certificate = PemCertificate.Instance.Certificate;

    private static readonly string CertificateKey = PemCertificate.Instance.CertificateKey;

    private static readonly string Thumbprint = PemCertificate.Instance.Thumbprint;

    private readonly ChromaContainer _chromaContainer = new ChromaBuilder(TestSession.GetImageFromDockerfile())
        // # --8<-- [start:ConfigureChromaContainerApiKey]
        .WithApiKey(ApiKey)
        // # --8<-- [end:ConfigureChromaContainerApiKey]

        // # --8<-- [start:ConfigureChromaContainerCertificate]
        .WithCertificate(Certificate, CertificateKey)
        // # --8<-- [end:ConfigureChromaContainerCertificate]
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _chromaContainer.StartAsync()
            .ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        return _chromaContainer.DisposeAsync();
    }

    [Fact]
    [Trait(nameof(DockerCli.DockerPlatform), nameof(DockerCli.DockerPlatform.Linux))]
    public async Task HealthReturnsValidResponse()
    {
        // Given
        // # --8<-- [start:ConfigureChromaClientCertificate-1]
        using var httpMessageHandler = new HttpClientHandler();
        httpMessageHandler.ServerCertificateCustomValidationCallback = CertificateValidation.Thumbprint(Thumbprint);

        using var httpClient = new HttpClient(httpMessageHandler);
        httpClient.DefaultRequestHeaders.Host = CommonName;
        // # --8<-- [end:ConfigureChromaClientCertificate-1]

        // # --8<-- [start:ConfigureChromaClientApiKey]
        httpClient.DefaultRequestHeaders.Add("api-key", ApiKey);
        // # --8<-- [end:ConfigureChromaClientApiKey]

        // # --8<-- [start:ConfigureChromaClientCertificate-2]
        var grpcChannelOptions = new GrpcChannelOptions();
        grpcChannelOptions.HttpClient = httpClient;

        using var grpcChannel = GrpcChannel.ForAddress(_chromaContainer.GetGrpcConnectionString(), grpcChannelOptions);

        using var grpcClient = new QdrantGrpcClient(grpcChannel);

        using var client = new QdrantClient(grpcClient);
        // # --8<-- [end:ConfigureChromaClientCertificate-2]

        // When
        var response = await client.HealthAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        // Then
        Assert.NotEmpty(response.Title);
    }

    [Fact]
    [Trait(nameof(DockerCli.DockerPlatform), nameof(DockerCli.DockerPlatform.Linux))]
    public async Task HealthWithoutApiKeyReturnsUnauthenticated()
    {
        // Given
        using var httpMessageHandler = new HttpClientHandler();
        httpMessageHandler.ServerCertificateCustomValidationCallback = CertificateValidation.Thumbprint(Thumbprint);

        using var httpClient = new HttpClient(httpMessageHandler);
        httpClient.DefaultRequestHeaders.Host = CommonName;

        var grpcChannelOptions = new GrpcChannelOptions();
        grpcChannelOptions.HttpClient = httpClient;

        using var grpcChannel = GrpcChannel.ForAddress(_chromaContainer.GetGrpcConnectionString(), grpcChannelOptions);

        using var grpcClient = new QdrantGrpcClient(grpcChannel);

        using var client = new QdrantClient(grpcClient);

        // When
        var exception = await Assert.ThrowsAsync<RpcException>(() => client.HealthAsync(TestContext.Current.CancellationToken))
            .ConfigureAwait(true);

        // Then
        Assert.Equal(StatusCode.Unauthenticated, exception.Status.StatusCode);
    }

    [Fact]
    [Trait(nameof(DockerCli.DockerPlatform), nameof(DockerCli.DockerPlatform.Linux))]
    public async Task HealthWithoutCertificateValidationReturnsSecureConnectionError()
    {
        // Given
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_chromaContainer.GetHttpConnectionString());
        httpClient.DefaultRequestHeaders.Host = CommonName;
        httpClient.DefaultRequestHeaders.Add("api-key", ApiKey);

        // When
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.GetAsync("/", TestContext.Current.CancellationToken))
            .ConfigureAwait(true);

        // Then
        Assert.Equal(HttpRequestError.SecureConnectionError, exception.HttpRequestError);
    }
}