# Orleans Dashboard

![](https://github.com/OrleansContrib/OrleansDashboard/workflows/Node%20CI/badge.svg?branch=master) ![](https://github.com/OrleansContrib/OrleansDashboard/workflows/.NET%20Core/badge.svg?branch=master)
![Nuget](https://img.shields.io/nuget/v/OrleansDashboard)

An admin dashboard for Microsoft Orleans.

![](screenshots/dashboard.png)

## Installation

Using the Package Manager Console:

```
PM> Install-Package OrleansDashboard
```

Then add with programmatic configuration:

```c#
new SiloHostBuilder()
  .UseDashboard(options => { })
  .Build();
```

Start the silo, and open this url in your browser: [`http://localhost:8080`](http://localhost:8080)

Please note, the dashboard registers its services and grains using `ConfigureApplicationParts` which disables the
automatic discovery of grains in Orleans. To enable automatic discovery of the grains of the original project, change
the configuration to:

```c#
new SiloHostBuilder()
  .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
  .UseDashboard(options => { })
  .Build();
```

### CPU and Memory Metrics on Windows

The CPU and Memory metrics are only enabled on Windows when you add the [Microsoft.Orleans.OrleansTelemetryConsumers.Counters](https://www.nuget.org/packages/Microsoft.Orleans.OrleansTelemetryConsumers.Counters/) package and have registered an implementation of  `IHostEnvironmentStatistics` such as with `builder.UsePerfCounterEnvironmentStatistics()` (currently Windows only).
You also have to wait some time before you see the data.

### CPU and Memory Metrics on Linux

Since version 2.3, Orleans includes an implementation of `IHostEnvironmentStatistics` for linux in
[Microsoft.Orleans.OrleansTelemetryConsumers.Linux](https://www.nuget.org/packages/Microsoft.Orleans.OrleansTelemetryConsumers.Linux/).
To enable CPU and Memory metrics, install the nuget package and add the implementation using to the silo using
`siloBuilder.UseLinuxEnvironmentStatistics()`.

## Configuring the Dashboard

The dashboard supports the following properties for the configuration:

* `Username` : Set a username for accessing the dashboard (basic auth).
* `Password` : Set a password for accessing the dashboard (basic auth).
* `Host` : Host name to bind the web server to (default is *).
* `Port` : Set the the number for the dashboard to listen on (default is 8080).
* `HostSelf` : Set the dashboard to host it's own http server (default is true).
* `CounterUpdateIntervalMs` : The update interval in milliseconds between sampling counters (default is 1000).

```c#
new SiloHostBuilder()
  .UseDashboard(options => {
    options.Username = "USERNAME";
    options.Password = "PASSWORD";
    options.Host = "*";
    options.Port = 8080;
    options.HostSelf = true;
    options.CounterUpdateIntervalMs = 1000;
  })
  .Build();
```

Note that some users have noticed performance degredation when using the dashboard. In this case it is recommended that you try increasing the `CounterUpdateIntervalMS` to 10000 to see if that helps.

## Using the Dashboard

Once your silos are running, you can connect to any of them using your web browser: `http://silo-address:8080/`

If you've started the dashboard on an alternative port, you'll need to specify that instead.

The dashboard will also relay trace information over http. You can view this in the dashboard, or from the terminal: `curl http://silo-address:8080/Trace`

## Building the UI

This is only required if you want to modify the user interface.

The user interface is react.js, using browserify to compose the javascript delivered to the browser.
The HTML and JS files are embedded resources within the dashboard DLL.

To build the UI, you must have [node.js](https://nodejs.org/en/) and npm installed.

To build `index.min.js`, which contains the UI components and dependencies, install the dependencies and run the build script using npm:

```
$ cd App
$ npm install
$ npm run build
```

This will copy the bundled, minified javascript file into the correct place for it to be picked up as an embedded resource in the .NET OrleansDashboard project.

You will need to rebuild the OrleansDashboard project to see any changes.

## Testing the Dashboard

The `Tests/TestHosts/` directory contains a number of preconfigured test application.

Try the `Tests/TestHosts/TestHost` project as a starting point.

## Dashboard API

The dashboard exposes an HTTP API you can consume yourself.

### DashboardCounters

```
GET /DashboardCounters
```

Returns a summary of cluster metrics. Number of active hosts (and a history), number of activations (and a history), summary of the active grains and active hosts.

```js
{
  "totalActiveHostCount": 3,
  "totalActiveHostCountHistory": [ ... ],
  "hosts": [ ... ],
  "simpleGrainStats": [ ... ],
  "totalActivationCount": 32,
  "totalActivationCountHistory": [ ... ]
}
```

### Historical Stats

```
GET /HistoricalStats/{siloAddress}
```

Returns last 100 samples of a silo's stats.

```js
[
  {
    "activationCount": 175,
    "recentlyUsedActivationCount": 173,
    "requestQueueLength": 0,
    "sendQueueLength": 0,
    "receiveQueueLength": 0,
    "cpuUsage": 88.216095,
    "availableMemory": 5097017340,
    "memoryUsage": 46837756,
    "totalPhysicalMemory": 17179869184,
    "isOverloaded": false,
    "clientCount": 1,
    "receivedMessages": 8115,
    "sentMessages": 8114,
    "dateTime": "2017-07-05T11:58:11.39491Z"
  },
  ...
]
```

### Silo Properties

```
GET /SiloProperties/{address}
```

Returns properties captured for the given Silo. At the moment this is just the Orleans version.

```js
{
  "OrleansVersion": "1.5.0.0"
}
````

### Grain Stats

```
GET /GrainStats/{grainName}
```

Returns the grain method profiling counters collected over the last 100 seconds for each grain, aggregated across all silos

```js
{
    "TestGrains.TestGrain.ExampleMethod2": {
    "2017-07-05T12:23:31": {
    "period": "2017-07-05T12:23:31.2230715Z",
    "siloAddress": null,
    "grain": "TestGrains.TestGrain",
    "method": "ExampleMethod2",
    "count": 2,
    "exceptionCount": 2,
    "elapsedTime": 52.1346,
    "grainAndMethod": "TestGrains.TestGrain.ExampleMethod2"
  },
  "2017-07-05T12:23:32": {
    "period": "2017-07-05T12:23:32.0823568Z",
    "siloAddress": null,
    "grain": "TestGrains.TestGrain",
    "method": "ExampleMethod2",
    "count": 5,
    "exceptionCount": 4,
    "elapsedTime": 127.04310000000001,
    "grainAndMethod": "TestGrains.TestGrain.ExampleMethod2"
  },
  ...
}
```

### Cluster Stats

```
GET /ClusterStats
```

Returns the aggregated grain method profiling counters collected over the last 100 seconds for whole cluster.

You should only look at the values for `period`, `count`, `exceptionCount` and `elapsedTime`. The other fields are not used in this response.

```js
{
  "2017-07-05T12:11:32": {
    "period": "2017-07-05T12:11:32.6507369Z",
    "siloAddress": null,
    "grain": null,
    "method": null,
    "count": 32,
    "exceptionCount": 4,
    "elapsedTime": 153.57039999999998,
    "grainAndMethod": "."
  },
  "2017-07-05T12:11:33": {
    "period": "2017-07-05T12:11:33.7203266Z",
    "siloAddress": null,
    "grain": null,
    "method": null,
    "count": 10,
    "exceptionCount": 2,
    "elapsedTime": 65.87930000000001,
    "grainAndMethod": "."
  },
  ...
}
```

### Silo Stats

```
GET /SiloStats/{siloAddress}
```

Returns the aggregated grain method profiling counters collected over the last 100 seconds for that silo.

You should only look at the values for `period`, `count`, `exceptionCount` and `elapsedTime`. The other fields are not used in this response.

```js
{
  "2017-07-05T12:11:32": {
    "period": "2017-07-05T12:11:32.6507369Z",
    "siloAddress": null,
    "grain": null,
    "method": null,
    "count": 32,
    "exceptionCount": 4,
    "elapsedTime": 153.57039999999998,
    "grainAndMethod": "."
  },
  "2017-07-05T12:11:33": {
    "period": "2017-07-05T12:11:33.7203266Z",
    "siloAddress": null,
    "grain": null,
    "method": null,
    "count": 10,
    "exceptionCount": 2,
    "elapsedTime": 65.87930000000001,
    "grainAndMethod": "."
  },
  ...
}
```

### Silo Stats

```
GET /SiloCounters/{siloAddress}
```

Returns the current values for the Silo's counters.

```js
[
  {
    "name": "App.Requests.Latency.Average.Millis",
    "value": "153.000",
    "delta": null
  },
  {
    "name": "App.Requests.TimedOut",
    "value": "0",
    "delta": "0"
  },
  ...
]
```

### Top Grain Methods

```
GET /TopGrainMethods
```

Returns the top 5 grain methods in terms of requests/sec, error rate and latency

```js
{
  "calls": [
    {
      "grain": "TestGrains.TestGrain",
      "method": "ExampleMethod2",
      "count": 1621,
      "exceptionCount": 783,
      "elapsedTime": 343.75,
      "numberOfSamples": 100
    },
    {
      "grain": "TestGrains.TestGrain",
      "method": "ExampleMethod1",
      "count": 1621,
      "exceptionCount": 0,
      "elapsedTime": 91026.73,
      "numberOfSamples": 100
    }
    ...
  ],
  "latency": [ ... ],
  "errors": [ ... ],
}
```

### Reminders

```
GET /Reminders/{page}
```

Returns the total number of reminders, and a page of 25 reminders. If the page number is not supplied, it defaults to page 1.

```js
{
  "count": 1500,
  "reminders": [
    {
      "grainReference": "GrainReference:*grn/D32F2751/0000007b",
      "name": "Frequent",
      "startAt": "2017-07-05T11:53:51.8648668Z",
      "period": "00:01:00",
      "primaryKey": "123"
    },
    ...
  ]
}
```

### Trace

```
GET /Trace
```

Streams the trace log as plain text in a long running HTTP request.



