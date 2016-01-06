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

## Configuring the Dashboard

The dashboard supports the following attributes in the configuration:

* `Port` : Set the the number for the dashboard to listen on.
* `Username` : Set a username for accessing the dashboard (basic auth).
* `Password` : Set a password for accessing the dashboard (basic auth).

```xml
<BootstrapProviders>
    <Provider Type="OrleansDashboard.Dashboard" Name="Dashboard" Port="1234" Username="my_username" Password="my_password" />
</BootstrapProviders>
```

## Todo

* ~~Find a workaround to the Windows namespace reservations~~
* Consider additional data sources
* Consider allowing activation / garbage collection from the UI
* Allow custom counters to be registered?
* Improve the UI.
* Consider collecting historical values for more of the counters
* ~~Consider a simple username/password (basic auth) for authentication~~
