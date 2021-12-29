using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Serilog.Sinks.Telegram.Client
{
  public class TelegramClient
  {
    private readonly Uri _apiUrl;
    private readonly HttpClient _httpClient = new HttpClient();

    public TelegramClient(string botToken, int timeoutSeconds = 10)
    {
      if (string.IsNullOrEmpty(botToken))
        throw new ArgumentException("Bot token can't be empty", nameof(botToken));

      _apiUrl = new Uri($"https://api.telegram.org/bot{botToken}/sendMessage");
      _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<HttpResponseMessage> PostAsync(TelegramMessage message, string chatId)
    {
      var payload = new { chat_id = chatId, text = message.Text, parse_mode = "MarkdownV2" };
      var json = JsonSerializer.Serialize(payload);
      var response = await _httpClient.PostAsync(_apiUrl,
        new StringContent(json, Encoding.UTF8, "application/json"));

      return response;
    }
  }
}