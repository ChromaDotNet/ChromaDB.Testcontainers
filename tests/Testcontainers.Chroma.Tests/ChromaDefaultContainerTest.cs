namespace Testcontainers.Chroma;

public sealed class ChromaDefaultContainerTest : IAsyncLifetime
{
    // # --8<-- [start:UseChromaContainer]
    private readonly ChromaContainer _chromaContainer = new ChromaBuilder(TestSession.GetImageFromDockerfile()).Build();

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
        using var client = new QdrantClient(new Uri(_chromaContainer.GetGrpcConnectionString()));

        // When
        var response = await client.HealthAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        // Then
        Assert.NotEmpty(response.Title);
        Assert.Equal(_chromaContainer.GetHttpConnectionString(), _chromaContainer.GetConnectionString());
    }
    // # --8<-- [end:UseChromaContainer]

    [Fact]
    [Trait(nameof(DockerCli.DockerPlatform), nameof(DockerCli.DockerPlatform.Linux))]
    public async Task GetRootEndpointReturnsHttpStatusCodeOk()
    {
        // Given
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_chromaContainer.GetHttpConnectionString());

        // When
        using var httpResponse = await httpClient.GetAsync("/", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        // Then
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}