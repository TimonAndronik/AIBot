using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

class Program
{
    private static readonly string telegramToken = "Bot_Token";
    private static readonly string mistralApiKey = "Mistral_Token";
    private static readonly string mistralUrl = "https://api.mistral.ai/v1/chat/completions";
    private static readonly HttpClient httpClient = new HttpClient();

    static async Task Main()
    {
        var botClient = new TelegramBotClient(telegramToken);
        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions);
        Console.WriteLine("🤖 Бот запущено!");
        await Task.Delay(-1);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
       
        if (update.Message is not { } message || message.Text is null) return;
       
        
        Console.WriteLine($"{message.Chat.Username} написав: {message.Text}");
            string response = await GetMistralResponse(message.Text);
            await botClient.SendMessage(message.Chat.Id, response, cancellationToken: cancellationToken);
        
        
    }

    private static async Task<string> GetMistralResponse(string question)
    {
        var requestData = new
        {
            model = "mistral-tiny",
            messages = new[]
            {
                new { role = "system", content = "Ти розумний AI-помічник. Відповідай тільки українською." },
                new { role = "user", content = question }
            }
        };

        string jsonRequest = JsonSerializer.Serialize(requestData);
        var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {mistralApiKey}");

        HttpResponseMessage response = await httpClient.PostAsync(mistralUrl, requestContent);
        string jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Помилка: {exception.Message}");
        return Task.CompletedTask;
    }
}
