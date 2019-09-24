# HttpTracing

Complete tracing of requests / responses for `HttpClient`


[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

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
    .AddHttpTracing<MyNamedClient>(); // Attaches a named tracing handler.
```

The tracing handler (`AddHttpTracing`) should probably be the last handler in the
chain in order to capture all modifications done by other handlers if they exist.

The logger category [follows the conventions defined by the `IHttpClientFactory`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#logging)
by naming the category `System.Net.Http.HttpClient.MyNamedClient.TraceHandler`.

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
    .AddHttpTracing<MyNamedClient>(
        response => response.StatusCode >= HttpStatusCode.InternalServerError);
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
