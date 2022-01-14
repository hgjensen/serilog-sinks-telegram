using System.Text.Json.Serialization;

namespace Serilog.Sinks.Telegram.Client {
  public sealed class TelegramMessage {
    public TelegramMessage(string text) {
      Text = text;
    }

    [JsonPropertyName("text")]
    public string Text { get; }
  }

  public sealed class TelegramIncomingMessage {
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("chat")]
    public TelegramChat Chat { get; set; }
  }

  public class TelegramChat {
    [JsonPropertyName("id")]
    public int Id { get; set; }
  }
}