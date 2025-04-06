

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
using OllamaSharp;
using OllamaPlayground;

const string question = "What is Harry's Address?";

var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion(OllamaChatCompletionService.modelId, OllamaChatCompletionService.endpoint)
.AddOllamaTextEmbeddingGeneration(OllamaChatCompletionService.modelId, OllamaChatCompletionService.endpoint)
.AddOllamaTextGeneration(OllamaChatCompletionService.modelId, OllamaChatCompletionService.endpoint);

var kernel = builder.Build();
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var embedingsService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
var textService = kernel.GetRequiredService<ITextGenerationService>();

// https://github.com/microsoft/kernel-memory/blob/main/README.md
// https://microsoft.github.io/kernel-memory/service/architecture
var memory = new KernelMemoryBuilder()
    .WithSemanticKernelTextEmbeddingGenerationService(embedingsService, new Microsoft.KernelMemory.SemanticKernel.SemanticKernelConfig(){ MaxTokenTotal=400}, textTokenizer: Microsoft.KernelMemory.AI.TokenizerFactory.GetTokenizerForModel(OllamaChatCompletionService.modelId))
    .WithSemanticKernelTextGenerationService(textService, new Microsoft.KernelMemory.SemanticKernel.SemanticKernelConfig(){ MaxTokenTotal=400 }, textTokenizer: Microsoft.KernelMemory.AI.TokenizerFactory.GetTokenizerForModel(OllamaChatCompletionService.modelId))
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
await memory.ImportDocumentAsync(new Document("harry-potter-and-the-philosophers-stone-by-jk-rowling.pdf").AddFile("C:\\projects\\YuriiYa\\SemanticKernelPlayground\\harry-potter-and-the-philosophers-stone-by-jk-rowling.pdf"), index: "potterindex001", steps:Constants.PipelineWithoutSummary);

var answer1 = await memory.AskAsync(question,minRelevance:0.9f,options: new SearchOptions { Stream=true});

Console.WriteLine($"My answer is {answer1}");
