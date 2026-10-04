namespace Testcontainers.Chroma;

public sealed class ChromaDefaultContainerTest : IAsyncLifetime
{
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
    public async Task GetHeartbeatReturnsHttpStatusCodeOk()
    {
        // Given
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_chromaContainer.GetBaseAddress());

        // When
        using var httpResponse = await httpClient.GetAsync("/api/v2/heartbeat", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        // Then
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.Equal(_chromaContainer.GetBaseAddress(), _chromaContainer.GetConnectionString());
    }
}
