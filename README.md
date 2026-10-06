[![ChromaDotNet](https://raw.githubusercontent.com/ChromaDotNet/.github/main/assets/logo-64.png)](https://chromadotnet.org)

# ChromaDB.Testcontainers

A [Testcontainers for .NET](https://dotnet.testcontainers.org/) module for [Chroma](https://www.trychroma.com/), published as the `ChromaDotNet.Testcontainers` package. It starts a throwaway Chroma container in your tests.

> This is a community project. It is not affiliated with or endorsed by Testcontainers or Chroma.

Website: [chromadotnet.org](https://chromadotnet.org)

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

`GetBaseAddress()` returns the same address. `ChromaBuilder.ChromaHttpPort` is the API port inside the container, 8000.

With [ChromaDB.Client](https://github.com/ChromaDotNet/ChromaDB.Client), the `ChromaDotNet.Client` package:

```csharp
using ChromaDB.Client;

using var httpClient = new HttpClient();
var options = new ChromaConfigurationOptions(chroma.GetConnectionString());
var client = new ChromaClient(options, httpClient);

var collection = await client.CreateCollectionAsync("documents");
```

The container is ready when the Chroma API answers its heartbeat: the v2 API from Chroma 0.5.16, the v1 API on earlier releases. So the module works with any Chroma image, whatever its tag.

## Building and testing

```bash
dotnet build ChromaDB.Testcontainers.slnx
dotnet test tests/Testcontainers.Chroma.Tests
CHROMA_IMAGE=chromadb/chroma:0.4.10 dotnet test tests/Testcontainers.Chroma.ImageTests
```

The tests start Chroma in containers, so they need Docker. `tests/Testcontainers.Chroma.ImageTests` runs on the image in `CHROMA_IMAGE`, and the CI sets it to each Chroma release it tests.

## Origin

The module follows the structure and conventions of the [Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet) modules, so that it can become one of them:

- the code and the tests started as a copy of its Qdrant module, under the MIT license;
- [docs/modules/chroma.md](https://github.com/ChromaDotNet/ChromaDB.Testcontainers/blob/main/docs/modules/chroma.md) is the page proposed for its documentation site. It names the package `Testcontainers.Chroma` that Testcontainers for .NET would publish, and its `--8<--` lines include the code when that site is built.

Here the package is `ChromaDotNet.Testcontainers`.

It is proposed to Testcontainers for .NET in [testcontainers/testcontainers-dotnet#1784](https://github.com/testcontainers/testcontainers-dotnet/pull/1784). This repository publishes it until Testcontainers for .NET ships `Testcontainers.Chroma`. Then this repository will be archived, and the package deprecated in favor of that one. The namespace and the API are the same, so moving changes only the package reference.

Every change to the module goes first into that pull request. `eng/sync-from-upstream.sh` then copies `src/Testcontainers.Chroma`, `tests/Testcontainers.Chroma.Tests` and `docs/modules/chroma.md` here. The rest belongs to this repository, like the build files, the CI and `tests/Testcontainers.Chroma.ImageTests`.
