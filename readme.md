# Orleans Dashboard

> This project is alpha quality, and is published to collect community feedback.

An admin dashboard for Microsoft Orleans.

![](screenshots/dashboard.png)

![](screenshots/silo.png)

## Installation

Nuget is currently not available (coming soon).

* Build this project.
* Copy the assemblies to the location of your Orleans host.
* Add this bootstrap provider to your Orleans configuration:

```xml
<?xml version="1.0" encoding="utf-8"?>
<OrleansConfiguration xmlns="urn:orleans">
  <Globals>
    <BootstrapProviders>
      <Provider Type="OrleansDashboard.Dashboard" Name="Dashboard" />
    </BootstrapProviders>
    ...
```

* Open this url in your browser: [`http://localhost:8080`](http://localhost:8080)

## Running on an alternative port

You can change the port number that the dashboard runs on in the configuration:

```xml
<BootstrapProviders>
    <Provider Type="OrleansDashboard.Dashboard" Name="Dashboard" port="1234" />
</BootstrapProviders>
```

## Todo

* ~~Find a workaround to the Windows namespace reservations~~
* Consider additional data sources
* Consider allowing activation / garbage collection from the UI
* Allow custom counters to be registered?
* Improve the UI.
* Consider collecting historical values for more of the counters
* Consider a simple username/password (basic auth) for authentication
