using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using System.IO.MemoryMappedFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.TextGeneration;

namespace OllamaPlayground;

// https://blog.antosubash.com/posts/ollama-semantic-kernal-connector
// https://www.youtube.com/watch?v=MsH6rYAkVZg
// https://blog.antosubash.com/posts/ollama-with-semantic-kernel
// Infor about llama3.1 model 8b https://ollama.com/library/llama3.1
public class SimpleOllamaChatBot
{
    public async Task Run()
    {
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
    }
}