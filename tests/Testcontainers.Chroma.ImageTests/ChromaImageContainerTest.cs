namespace Testcontainers.Chroma;

// Runs on the image in the CHROMA_IMAGE environment variable, which the CI sets to each Chroma
// release it tests, and is skipped when the variable is not set.
public sealed class ChromaImageContainerTest : IAsyncLifetime
{
    private static readonly string Image = Environment.GetEnvironmentVariable("CHROMA_IMAGE");

    private readonly ChromaContainer _chromaContainer = string.IsNullOrEmpty(Image) ? null : new ChromaBuilder(Image).Build();

    public async ValueTask InitializeAsync()
    {
        if (_chromaContainer != null)
        {
            await _chromaContainer.StartAsync()
                .ConfigureAwait(false);
        }
    }

    public ValueTask DisposeAsync()
    {
        return _chromaContainer?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    [Fact]
    [Trait(nameof(DockerCli.DockerPlatform), nameof(DockerCli.DockerPlatform.Linux))]
    public async Task HeartbeatAnswersOnceTheContainerIsReady()
    {
        Assert.SkipWhen(_chromaContainer == null, "CHROMA_IMAGE is not set.");

        // Given
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_chromaContainer.GetBaseAddress());

        // When
        using var v2Response = await httpClient.GetAsync("/api/v2/heartbeat", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        using var v1Response = await httpClient.GetAsync("/api/v1/heartbeat", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        // Then
        Assert.Contains(HttpStatusCode.OK, new[] { v2Response.StatusCode, v1Response.StatusCode });
    }
}
