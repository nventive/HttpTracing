# HttpTracing

Complete tracing of requests / responses for `HttpClient`


[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![Build Status](https://dev.azure.com/nventive-public/nventive/_apis/build/status/nventive.HttpTracing?branchName=master)](https://dev.azure.com/nventive-public/nventive/_build/latest?definitionId=9&branchName=master)
![Nuget](https://img.shields.io/nuget/v/HttpTracing)

## Getting Started

Install the package:

```
Install-Package HttpTracing
```

## Features

When configuring the `HttpClient` via the factory, add the tracing handler:

```csharp

using Microsoft.Extensions.DependencyInjection;


services
    .AddHttpClient<MyNamedClient>() // Adds a named HttpClient
    .AddHttpTracing(); // Attaches a tracing handler.
```

The tracing handler (`AddHttpTracing`) should probably be the last handler in the
chain in order to capture all modifications done by other handlers if they exist.

The logger category [follows the conventions defined by the `IHttpClientFactory`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#logging)
by naming the category `System.Net.Http.HttpClient.{HttpClient.Name}.TraceHandler`
(e.g. `System.Net.Http.HttpClient.MyNamedClient.TraceHandler`).

Event ids used for the various messages:

| Event id | Event Name         | Log level | Description                         |
|----------|--------------------|-----------|-------------------------------------|
| 200      | RequestSuccessful  | Trace     | Request trace on successful calls   |
| 201      | RequestError       | Warning   | Request trace on unsuccessful calls |
| 210      | ResponseSuccessful | Trace     | Response on successful calls        |
| 211      | ResponseError      | Warning   | Response on unsuccessful calls      |

A successful call is determined by default using `HttpResponseMessage.IsSuccessStatusCode`.
This can be customized when adding the handler:

```csharp
services
    .AddHttpClient<MyNamedClient>()
    .AddHttpTracing(
        isResponseSuccessful: response => response.StatusCode >= HttpStatusCode.InternalServerError);
```

The logger category name can also be customized if needed:

```csharp
services
    .AddHttpClient<MyNamedClient>()
    .AddHttpTracing(categoryName: "Foo.Bar");
```

### Using with Application Insights

By default, Application Insights captures only `Warning` and `Error` log levels.
To enable tracing of successful requests and responses, [configure the log level for Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger). 
Example within the `appsettings.json` file:

```json
{
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "System.Net.Http.HttpClient.MyNamedClient.TraceHandler": "Trace"
      }
    },
  }
}
```

### Buffering the requests content

Some frameworks uses `HttpContent` streams on `HttpRequestMessage` that cannot be replayed, thus preventing the component
to read the body of the request in such cases.

The default behavior is to write the following line in lieu of the body:
```
[InvalidOperationException: Request content is not buffered (...). Use the bufferRequests parameter to allow the reading.]
```

If you want to see the request content in such cases, use the `bufferRequests` parameter:

```csharp
services
    .AddHttpClient<MyNamedClient>()
    .AddHttpTracing(bufferRequests: true);
```

### Adding globally

It is possible to add tracing to **all** `HttpClient` registered through the `IHttpClientFactory`:

```csharp
services.AddHttpTracingToAllHttpClients();
```

This way, the tracing handler is added to all instances globally.
To customize the parameters, use the factory configuration method:

```csharp
services.AddHttpTracingToAllHttpClients((sp, builder) =>
{
    return builder.Name switch
    {
        nameof(BufferedClient) => new HttpMessageHandlerTracingConfiguration { BufferRequests = true },
        nameof(DisabledClient) => new HttpMessageHandlerTracingConfiguration { Enabled = false },
        _ => null, // Default configuration
    };
})
```

## Changelog

Please consult the [CHANGELOG](CHANGELOG.md) for more information about version
history.

## License

This project is licensed under the Apache 2.0 license - see the
[LICENSE](LICENSE) file for details.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for
contributing to this project.

Be mindful of our [Code of Conduct](CODE_OF_CONDUCT.md).
