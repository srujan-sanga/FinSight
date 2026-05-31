using TransactionService.Api.Grpc;
using TransactionService.Contracts.ServiceContracts;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Server;

var builder = WebApplication.CreateBuilder(args);
var businessAssembly = BusinessAssemblyLoader.Load("TransactionService.Business");
var grpcManagerServices = builder.Services.AddExternalManagerGrpcServices(
    typeof(ITransactionManager).Assembly,
    businessAssembly);

builder.Services.AddCodeFirstGrpc();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

app.MapExternalManagerGrpcServices(grpcManagerServices);
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
