using Assistants.API;
using Assistants.API.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => options.SerializeAsV2 = true);
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseMiddleware<ApiKeyMiddleware>();
}

app.UseHttpsRedirection();

app.MapApi();
app.Run();
