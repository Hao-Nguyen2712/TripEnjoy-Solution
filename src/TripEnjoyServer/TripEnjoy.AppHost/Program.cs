var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithDataVolume()
    .AddDatabase("TripEnjoy");

// Add Redis cache
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Add RabbitMQ message broker
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume();

// Add TripEnjoy API
var api = builder.AddProject<Projects.TripEnjoy_Api>("tripenjoy-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitmq);

// Add TripEnjoy Blazor Client (optional)
builder.AddProject<Projects.TripEnjoy_Client>("tripenjoy-client")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
