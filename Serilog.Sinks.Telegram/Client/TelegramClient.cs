using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Sinks.Telegram.Client {
  public class TelegramClient {
    private readonly Uri _apiUrl;
    private readonly HttpClient _httpClient = new HttpClient();

    public TelegramClient(string botToken, int timeoutSeconds = 10) {
      if (string.IsNullOrEmpty(botToken))
        throw new ArgumentException("Bot token can't be empty", nameof(botToken));

      _apiUrl = new Uri($"https://api.telegram.org/bot{botToken}/");
      _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<HttpResponseMessage> PostAsync(TelegramMessage message, string chatId) {
      var payload = new { chat_id = chatId, text = escapeMarkdownV2(message.Text), parse_mode = "MarkdownV2" };
      var json = JsonSerializer.Serialize(payload);
      var sendMsgUri = new Uri(_apiUrl, "sendMessage");
      var response = await _httpClient.PostAsync(sendMsgUri,
        new StringContent(json, Encoding.UTF8, "application/json"));

      string jsonString = await response.Content.ReadAsStringAsync();
      Debug.WriteLine(jsonString);

      return response;
    }

    public async Task<List<TelegramUpdate>> GetUpdates(int offset, CancellationToken ct) {
      var payload = new { offset = offset, allowed_updates = new[] { "message", "channel_post" }, timeout = 60 * 60 };
      var json = JsonSerializer.Serialize(payload);
      var client = new HttpClient();
      client.Timeout = Timeout.InfiniteTimeSpan;
      var getUpdatesUri = new Uri(_apiUrl, "getUpdates");
      var response = await client.PostAsync(getUpdatesUri,
        new StringContent(json, Encoding.UTF8, "application/json"), ct);

      try {
        response.EnsureSuccessStatusCode();
        var jsonStream = await response.Content.ReadAsStreamAsync();

        //string jsonString = await response.Content.ReadAsStringAsync();
        //Debug.WriteLine(jsonString);

        var updates = await JsonSerializer.DeserializeAsync<TelegramUpdateWrapper>(jsonStream, cancellationToken: ct);
        if (!(updates?.Ok ?? false)) throw new Exception("Response not ok");
        return updates.Result;
      } catch (Exception x) {
        Log.Information("Serilog Telegram logging error: " + x.Message);
        Log.Verbose(x, "Serilog Telegram logging error");
        return new List<TelegramUpdate>();
      }
    }

    private static string escapeMarkdownV2(string text) {
      var toReplace = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
      foreach (var item in toReplace)
        text = text.Replace(item, $"\\{item}");
      return text;
    }
  }
}