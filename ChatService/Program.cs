using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ChatService.Configuration;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using ChatService.Storage.Interfaces;
using ChatService.Storage.Implementations;
using ChatService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Configuration
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<BlobSettings>(builder.Configuration.GetSection("BlobStorage"));

// Add Services
builder.Services.AddSingleton<IProfileInterface, CosmosProfileStore>();
builder.Services.AddSingleton(sp =>
{
    var cosmosOptions = sp.GetRequiredService<IOptions<CosmosSettings>>();
    return new CosmosClient(cosmosOptions.Value.ConnectionString);
});

builder.Services.AddSingleton<IImageInterface, BlobPictureStore>();
builder.Services.AddSingleton(sp =>
{
    var blobSettings = sp.GetRequiredService<IOptions<BlobSettings>>();
    return new BlobServiceClient(blobSettings.Value.ConnectionString);
});

builder.Services.AddSingleton<IMessagesStore, CosmosMessageStore>();
builder.Services.AddSingleton<IConversationStore, CosmosConversationStore>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddSingleton<IMessageService, MessageService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }