# ChromaDB.Testcontainers

A [Testcontainers for .NET](https://dotnet.testcontainers.org/) module for [Chroma](https://www.trychroma.com/), published as the `ChromaDotNet.Testcontainers` package: start a throwaway Chroma container in your tests.

> This is a community project. It is not affiliated with or endorsed by Testcontainers or Chroma.

## Usage

```bash
dotnet add package ChromaDotNet.Testcontainers
```

```csharp
using Testcontainers.Chroma;

await using var chroma = new ChromaBuilder("chromadb/chroma:1.5.9").Build();
await chroma.StartAsync();

// The base address of the Chroma API, like http://localhost:32768/
var address = chroma.GetConnectionString();
```

With [ChromaDotNet.Client](https://github.com/ChromaDotNet/ChromaDB.Client):

```csharp
using var httpClient = new HttpClient();
var options = new ChromaConfigurationOptions(chroma.GetConnectionString());
var client = new ChromaClient(options, httpClient);

var collection = await client.CreateCollection("documents");
```

The container is ready when the heartbeat of the Chroma API answers: the v2 API from Chroma 0.5.16, and the v1 API on the earlier releases, so the module works with any Chroma image, whatever its tag.

## Building and testing

```bash
dotnet build ChromaDB.Testcontainers.slnx
dotnet test tests/Testcontainers.Chroma.Tests
CHROMA_IMAGE=chromadb/chroma:0.4.10 dotnet test tests/Testcontainers.Chroma.ImageTests
```

The tests start Chroma in containers, so they need Docker. `tests/Testcontainers.Chroma.ImageTests` runs on the image in `CHROMA_IMAGE`, which the CI sets to each Chroma release it tests.

## Origin

The module follows the structure and conventions of the modules of [Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet), so that it can become one of them: the code and the tests started as a copy of its Qdrant module, under the MIT license, and [docs/modules/chroma.md](docs/modules/chroma.md) follows the format of its module pages.

It is proposed to Testcontainers for .NET in [testcontainers/testcontainers-dotnet#1784](https://github.com/testcontainers/testcontainers-dotnet/pull/1784). This repository publishes it until Testcontainers for .NET ships `Testcontainers.Chroma`; then it will be archived and the package deprecated in its favor. The namespace and the API are the same, so moving there changes only the package reference.

Every change to the module is made first in that pull request; `eng/sync-from-upstream.sh` copies `src/Testcontainers.Chroma`, `tests/Testcontainers.Chroma.Tests` and `docs/modules/chroma.md` here. The rest, like the build files, the CI and `tests/Testcontainers.Chroma.ImageTests`, belongs to this repository.
