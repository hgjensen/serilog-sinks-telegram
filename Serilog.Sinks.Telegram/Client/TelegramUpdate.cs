using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Serilog.Sinks.Telegram.Client {
  public class TelegramUpdateWrapper {
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("result")]
    public List<TelegramUpdate> Result { get; set; }
  }

  public class TelegramUpdate {
    [JsonIgnore]
    internal Action<string> ReplyAction { get; set; }

    [JsonPropertyName("update_id")]
    public int UpdateId { get; set; }

    [JsonPropertyName("message")]
    public TelegramIncomingMessage Message { get; set; }

    [JsonPropertyName("channel_post")]
    public TelegramIncomingMessage ChannelPost { get; set; }

    public async Task Reply(string replyText) {
      await Task.Run(() => ReplyAction(replyText));
    }
  }
}