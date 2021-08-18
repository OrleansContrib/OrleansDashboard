using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

// We need a silo to connect to, start one now. If you have an external Silo running then you can skip this line
await CreateSampleSiloHost().StartAsync();

// Create an orleans client that connects to our Silo
using var orleansClient = new ClientBuilder()
    .Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "helloworldcluster";
        options.ServiceId = "1";
    })
    .UseLocalhostClustering()
    .ConfigureLogging(logging => logging.AddConsole())
    .Build();

// Create our appBuilder
var builder = WebApplication.CreateBuilder(args);
// Register our client as a GrainFactory
builder.Services.AddSingleton(c => (IGrainFactory)orleansClient);
// Add services required for the OrleansDashboard middleware
builder.Services.AddServicesForSelfHostedDashboard();

var app = builder.Build();
// Connect our client
await orleansClient.Connect();

// Register our middleware
app.UseOrleansDashboard();

app.Run(); 




// A helper method used to create a sample silo
static ISiloHost CreateSampleSiloHost() 
    => new SiloHostBuilder()
        .UseDashboard(options =>
        {
            options.HostSelf = false;
        })
        .UseLocalhostClustering()
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "helloworldcluster";
            options.ServiceId = "1";
        })
        .Build();