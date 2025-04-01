
/*using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;

var builder = Kernel.CreateBuilder();

builder.Services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient("http://localhost:11434"));

builder.Services.AddScoped<IChatCompletionService, OllamaChatCompletionService>();

var kernel = builder.Build();

var chatService = kernel.GetRequiredService<IChatCompletionService>();

var history = new ChatHistory();
history.AddSystemMessage("You are help full assistant that will help you with your questions.");

while (true)
{
    Console.Write("You: ");
    var userMessage = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userMessage))
    {
        break;
    }

    history.AddUserMessage(userMessage);

    var response = await chatService.GetChatMessageContentAsync(history);

    Console.WriteLine($"Bot: {response.Content}");

    history.AddMessage(response.Role, response.Content ?? string.Empty);
}
*/

// https://blog.antosubash.com/posts/ollama-semantic-kernal-connector
using System.IO.MemoryMappedFiles;
using ClosedXML;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.TextGeneration;

Uri endpoint = new Uri("http://localhost:11434");
const string question = "What is Harry's Address?";
const string model = "llama3";

var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion(model, endpoint)
.AddOllamaTextEmbeddingGeneration(model, endpoint)
.AddOllamaTextGeneration(model, endpoint);

var kernel = builder.Build();
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var embedingsService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
var textService = kernel.GetRequiredService<ITextGenerationService>();

// https://github.com/microsoft/kernel-memory/blob/main/README.md
var memory = new KernelMemoryBuilder()
    .WithSemanticKernelTextEmbeddingGenerationService(embedingsService, new Microsoft.KernelMemory.SemanticKernel.SemanticKernelConfig(){ MaxTokenTotal=400}, textTokenizer: Microsoft.KernelMemory.AI.TokenizerFactory.GetTokenizerForModel(model))
    .WithSemanticKernelTextGenerationService(textService, new Microsoft.KernelMemory.SemanticKernel.SemanticKernelConfig(){ MaxTokenTotal=400 }, textTokenizer: Microsoft.KernelMemory.AI.TokenizerFactory.GetTokenizerForModel(model))
    .WithSimpleFileStorage(new Microsoft.KernelMemory.DocumentStorage.DevTools.SimpleFileStorageConfig(){ Directory = "memory", StorageType = Microsoft.KernelMemory.FileSystem.DevTools.FileSystemTypes.Volatile })
    .WithCustomTextPartitioningOptions(
        new TextPartitioningOptions
        {
            MaxTokensPerParagraph = 400,
            OverlappingTokens = 100
        })
    .Build();
builder.Services.AddSingleton<IKernelMemory>(memory);
// Import a file
await memory.ImportDocumentAsync("C:\\projects\\study\\AIML\\harry-potter-and-the-philosophers-stone-by-jk-rowling.pdf", "harry-potter-and-the-philosophers-stone-by-jk-rowling.pdf");

var answer1 = await memory.AskAsync(question,minRelevance:0.8f,options: new SearchOptions { Stream=false});



var history = new ChatHistory();
history.AddSystemMessage("You are a helpful assistant.");

while (true)
{
    Console.Write("You: ");
    var userMessage = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userMessage))
    {
        break;
    }

    history.AddUserMessage(userMessage);

    var response = await chatService.GetChatMessageContentAsync(history);

    Console.WriteLine($"Bot: {response.Content}");

    history.AddMessage(response.Role, response.Content ?? string.Empty);
}