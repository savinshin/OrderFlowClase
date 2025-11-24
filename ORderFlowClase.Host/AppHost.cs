var builder = DistributedApplication.CreateBuilder(args);


var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("postgres-data-identity")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));


var db =  postgres.AddDatabase("identity");

builder.AddProject<Projects.OrderFlowClase_API_Identity>("orderflowclase-api-identity")
    .WaitFor(db)
    .WithReference(db);

builder.AddProject<Projects.OrderFlowClase_ApiGateway>("orderflowclase-apigateway");

builder.Build().Run();
