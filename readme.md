# Orleans Dashboard

> This project is alpha quality, and is published to collect community feedback.

An admin dashboard for Microsoft Orleans.

![](screenshots/dashboard.png)

![](screenshots/silo.png)

## Installation

Nuget is currently not available (coming soon).

1. Build this project.
1. Copy the assemblies to the location of your Orleans host.
1. Add this bootstrap provider to your Orleans configuration:
```xml
<?xml version="1.0" encoding="utf-8"?>
<OrleansConfiguration xmlns="urn:orleans">
  <Globals>
    <BootstrapProviders>
      <Provider Type="OrleansDashboard.Dashboard" Name="Dashboard" />
    </BootstrapProviders>
    ...
```
1. Run the Orleans host elevated.
1. Open this url in your browser: `http://localhost:8080`

## Why run Orleans elevated?

...because of [namespace reservations on Windows](https://github.com/NancyFx/Nancy/wiki/Self-Hosting-Nancy#namespace-reservations).

Alternatively you can run an `netsh` command and run without elevation:

```
netsh http add urlacl url=http://+:8080/ user=Everyone
```

## Running on an alternative port

You can change the port number that the dashboard runs on in the configuration:

```xml
<BootstrapProviders>
    <Provider Type="OrleansDashboard.Dashboard" Name="Dashboard" port="1234" />
</BootstrapProviders>
```

## Todo

1. Find a workaround to the Windows namespace reservations
1. Consider additional data sources
1. Consider allowing activation / garbage collection from the UI
1. Allow custom counters to be registered?
1. Improve the UI.
1. Consider collecting historical values for more of the counters

