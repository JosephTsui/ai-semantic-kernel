using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

string filePath = Path.GetFullPath("appsettings.json");
var config = new ConfigurationBuilder()
    .AddJsonFile(filePath)
    .Build();

// Set your values in appsettings.json
string apiKey = config["AZURE_OPENAI_KEY"]!;
string endpoint = config["AZURE_OPENAI_ENDPOINT"]!;
string deploymentName = config["DEPLOYMENT_NAME"]!;

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create the chat history
var chatHistory = new ChatHistory();

// Create a semantic kernel prompt template
var skTemplateFactory = new KernelPromptTemplateFactory();
var skPromptTemplate = skTemplateFactory.Create(
    new PromptTemplateConfig(
        """
        你是一個很專業的職業諮詢顧問。根據使用者的技能和興趣，建議最多五個適合的職業。
        回傳值格式為 JSON ，內容格式如下：
        "職業建議"：
        {
            "建議職業"： [],
            "領域": [],
            "建議薪資範圍": ""
        }

        我的技能有： ，我的興趣是： ， 有哪些適合我的職業？
        """
    ));

// Render the Semantic Kernel prompt with arguments
var skRenderedPrompt = await skPromptTemplate.RenderAsync(
    kernel,
    new KernelArguments
    {
        ["skills"] = "程式設計, C#, Python, Drawing, Guitar, Dance",
        ["interests"] = "教育, 心理學, 程式設計, 幫助他人"
    }
);

// Add the Semantic Kernel prompt to the chat history and get the reply
chatHistory.AddUserMessage(skRenderedPrompt);

// Create a handlebars template
var hbTemplateFactory = new HandlebarsPromptTemplateFactory();
var hbPromptTemplate = hbTemplateFactory.Create(new PromptTemplateConfig()
{
    TemplateFormat = "handlebars",
    Name = "MissingSkillsPrompt",
    Template = """
        <message role="system">
        Instructions: You are a career advisor. Analyze the skill gap 
        between the user's current skills and the requirements of the target role.
        </message>
        <message role="user">Target Role: </message>
        <message role="user">Current Skills: </message>

        <message role="assistant">
        "Skill Gap Analysis":
        {
            "missingSkills": [],
            "coursesToTake":[],
            "certificationSuggestions":[]
        }
        </message>
    """
});

// Render the Handlebars prompt with arguments
var hbRenderedPrompt = await hbPromptTemplate.RenderAsync(
    kernel,
    new KernelArguments
    {
        ["targetRole"] = "遊戲開發者",
        ["currentSkills"] = "程式設計, C#, Python, Drawing, Guitar, Dance"
    }
);

// Add the Handlebars prompt to the chat history and get the reply
chatHistory.AddUserMessage(hbRenderedPrompt);
await GetReply();

// Get a follow-up prompt from the user
Console.WriteLine("Assistant: 我能幫助你什麼？");
Console.Write("User: ");
string input = Console.ReadLine()!;

// Add the user input to the chat history and get the reply
chatHistory.AddUserMessage(input);
await GetReply();

async Task GetReply()
{
    // Get the reply from the chat completion service
    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        kernel: kernel
    );
    Console.WriteLine("Assistant:" + reply.ToString());
    chatHistory.AddAssistantMessage(reply.ToString());
}