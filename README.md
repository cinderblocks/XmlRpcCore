# XmlRpcCore

[![Build status](https://ci.appveyor.com/api/projects/status/1mw8qwd83q1u7l5d?svg=true)](https://ci.appveyor.com/project/cinderblocks57647/xmlrpccore)  
[![XmlRpcCore NuGet-Release](https://img.shields.io/nuget/v/xmlrpccore.svg?label=XmlRpcCore)](https://www.nuget.org/packages/XmlRpcCore/)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/XmlRpcCore?label=NuGet%20downloads)](https://www.nuget.org/packages/XmlRpcCore/)

Introduction
------------

`XmlRpcCore` is a small, modernized XML-RPC client and serializer library compatible with .NET Standard 2.0 and later. The library favors modern .NET patterns:

- Generic collections (`List<object>`, `Dictionary<string, object>`) instead of legacy non-generic collections.
- DI-friendly serializers/deserializers (`IXmlRpcSerializer`).
- Async streaming request/response APIs to avoid intermediate allocations.
- Extensible mapping from XML-RPC values to POCOs via `IObjectMapper`.

This README shows current usage examples and a migration guide from legacy code (3.x and XmlRpcCs).

Namespace and breaking-change note
----------------------------------

Public types you will commonly use are:

- `XmlRpcCore.XmlRpcRequest` / `XmlRpcCore.XmlRpcResponse`
- `XmlRpcCore.IXmlRpcSerializer` (serializer abstraction)
- `XmlRpcCore.XmlRpcNetSerializer` (default serializer)
- `XmlRpcCore.XmlRpcClient` (modern, DI-friendly client)
- `XmlRpcCore.ObjectMapper` (default POCO mapper)

Breaking / migration notes:

- There is an `[Obsolete]` constructor on `XmlRpcRequest` that accepts `ArrayList` kept only for transition. Prefer `List<object>` / `Dictionary<string, object>`.

Quickstart (copy/paste-ready)
-----------------------------

A minimal example showing the typical call flow (includes required usings):

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XmlRpcCore;

class Program
{
    static async Task Main()
    {
        var httpClient = new HttpClient();
        var request = new XmlRpcRequest("demo.sum", new List<object> { 4, 5 });
        var response = await httpClient.PostAsXmlRpcAsync("https://example.com/rpc", request);
        Console.WriteLine(response.Value);
    }
}
```

DI + HttpClientFactory example
------------------------------

Register the serializer, mapper and typed client using `IServiceCollection` so you can consume a typed `XmlRpcClient` with `IHttpClientFactory` integration:

```csharp
using Microsoft.Extensions.DependencyInjection;
using XmlRpcCore;

var services = new ServiceCollection();

// register defaults
services.AddSingleton<IXmlRpcSerializer, XmlRpcNetSerializer>();
services.AddSingleton<IObjectMapper, ObjectMapper>();

// register typed client; AddHttpClient will supply HttpClient in constructor
services.AddHttpClient<XmlRpcClient>();

var provider = services.BuildServiceProvider();
var xmlClient = provider.GetRequiredService<XmlRpcClient>();

// use xmlClient.InvokeAsync<T>(url, request)
```

Streaming & cancellation example
-------------------------------

Use the async stream APIs to avoid intermediate string allocations and to support cancellation:

```csharp
using System.IO;
using System.Threading;
using XmlRpcCore;

var serializer = new XmlRpcNetSerializer();
var request = new XmlRpcRequest("demo.heavy", new List<object> { /* large payload */ });
using var ms = new MemoryStream();

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await serializer.SerializeRequestAsync(request, ms, cts.Token);
ms.Position = 0;
var response = await serializer.DeserializeResponseAsync(ms, cts.Token);
```

Mapping caveats & System.Text.Json behavior
-------------------------------------------

The default `ObjectMapper` uses `System.Text.Json` internally to map dictionary-shaped XML-RPC results into POCOs and back. A few important notes:

- Nested objects and arrays are handled: the mapper converts `JsonElement` trees into plain CLR objects (integers, longs, doubles, strings, DateTime, `List<object>`, `Dictionary<string, object>`).
- Numeric types may be `int` or `long` depending on value magnitude; code deserializing dictionary values should be resilient to `long` vs `int` (tests include helpers to convert when necessary).
- Enums are mapped from strings when possible (default `JsonStringEnumConverter` behavior).
- If you need tighter control or better performance for hot paths, consider implementing a reflection-based `IObjectMapper` and registering it in DI.

Security / XXE and hardening defaults
------------------------------------

XmlRpcCore uses safe defaults to mitigate XXE and XML-based DoS attacks:

- DTD processing is disabled by default and `XmlResolver` is not used.
- A maximum element depth (`MaxDepth`, default 128) is enforced during parsing.
- `XmlRpcOptions` exposes additional limits: `MaxCharactersInDocument` and `MaxCharactersFromEntities`. These are applied when supported by the runtime.

To tune settings for your application, set `XmlRpcSettingsManager.Options` at startup (example below). Only enable DTDs or an `XmlResolver` if you fully understand the security implications.

```csharp
XmlRpcSettingsManager.Options = new XmlRpcOptions
{
    MaxDepth = 256,
    MaxCharactersInDocument = 50_000_000,
    MaxCharactersFromEntities = 2_000_000,
    AllowDtd = false,
    AllowXmlResolver = false
};
```

Notes
- Changing `Options` affects all deserializers that use the shared settings. Set before handling untrusted input.
- Only change `AllowDtd`/`AllowXmlResolver` if you fully understand the security implications.

Migration guide (legacy -> modern)
----------------------------------

If you previously used `ArrayList`/`Hashtable` or singleton serializers, follow these steps to migrate safely:

- Replace `ArrayList` inputs with `List<object>` and `Hashtable` with `Dictionary<string, object>`.

Legacy code example:

```csharp
// legacy
ArrayList args = new ArrayList();
args.Add("hello");
Hashtable map = new Hashtable();
map["a"] = 1;
```

Modern replacement:

```csharp
var args = new List<object> { "hello" };
var map = new Dictionary<string, object> { ["a"] = 1 };
```

- Use the DI-friendly `IXmlRpcSerializer` instead of singletons. Pass a serializer into `XmlRpcClient` or to `XmlRpcRequest` constructors when you need custom behavior.

- Replace `XmlRpcRequestSerializer.Singleton` or `XmlRpcResponseSerializer.Singleton` calls by constructing an instance or registering one in DI.

Breaking changes
----------------

- Legacy non-generic collections support has been removed from the core serialization pipeline. The codebase still ships a marked `[Obsolete]` constructor on `XmlRpcRequest` that accepts `ArrayList` as a transition, but you should migrate to generic collections.
- The library now prefers modern APIs and async streaming patterns; some convenience synchronous APIs may still exist for compatibility.

Error handling
--------------

There are typed exceptions to help diagnosing issues:

- `XmlRpcProtocolException` — parsing/format errors in XML.
- `XmlRpcTransportException` — transport or unexpected deserialization errors.
- `XmlRpcException` — represents an XML-RPC fault (faultCode/faultString).

Testing
-------

The repository includes unit tests covering serialization, deserialization, streaming, mapping, and client behaviors. Run them with:

```bash
dotnet test XmlRpcCore.Tests/XmlRpcCore.Tests.csproj
```

Performance and tuning
----------------------

- Use the stream-based async APIs to avoid intermediate string allocations in high-throughput scenarios.
- Swap the `ObjectMapper` for a reflection-based mapper if JSON round-trips are too costly for your workload.
- Configure `JsonSerializerOptions` or provide a custom `IObjectMapper` to control mapping behavior.

Contributing
------------

Contributions are welcome. Please follow the existing project style and add unit tests for new behaviors. Open issues or PRs on the project GitHub repository.

License
-------

XmlRpcCore is under the BSD license. See: `LICENSE`.

References
----------

- XML-RPC spec: http://xmlrpc.org
- Original project: http://xmlrpccs.sourceforge.net/
