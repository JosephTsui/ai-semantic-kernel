using Microsoft.Extensions.Configuration;

// Import namespaces
using Microsoft.SemanticKernel;

string filePath = Path.GetFullPath("appsettings.json");
var config = new ConfigurationBuilder()
    .AddJsonFile(filePath)
    .Build();

// Set your values in appsettings.json
string apiKey = config["AZURE_OPENAI_KEY"]!;
string endpoint = config["AZURE_OPENAI_ENDPOINT"]!;
string deploymentName = config["DEPLOYMENT_NAME"]!;


// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
deploymentName, endpoint, apiKey
);
Kernel kernel = builder.Build();

// Test the chat completion service
var result = await kernel.InvokePromptAsync("給我一份有十種含蛋及起司的早餐食物清單。");
Console.WriteLine(result);