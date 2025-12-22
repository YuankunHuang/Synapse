using Synapse.Server.Extensions;
using Synapse.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddWebServices()
                .AddCoreServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseWebPipeline();
app.MapWebEndpoints();

app.Run();