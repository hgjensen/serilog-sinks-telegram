﻿using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Telegram.Client;

namespace Serilog.Sinks.Telegram {
  public class TelegramSink : ILogEventSink {
    /// <summary>
    /// Delegate to allow overriding of the RenderMessage method.
    /// </summary>
    public delegate TelegramMessage RenderMessageMethod(LogEvent input);

    private readonly string _chatId;
    private readonly string _token;
    protected readonly IFormatProvider FormatProvider;

    /// <summary>
    /// RenderMessage method that will transform LogEvent into a Telegram message.
    /// </summary>
    protected RenderMessageMethod RenderMessageImplementation = RenderMessage;

    public TelegramSink(string chatId, string token, RenderMessageMethod renderMessageImplementation,
      IFormatProvider formatProvider) {
      if (string.IsNullOrWhiteSpace(chatId))
        throw new ArgumentNullException(nameof(chatId));

      if (string.IsNullOrWhiteSpace(token))
        throw new ArgumentNullException(nameof(token));

      FormatProvider = formatProvider;
      if (renderMessageImplementation != null)
        RenderMessageImplementation = renderMessageImplementation;
      _chatId = chatId;
      _token = token;
    }

    #region ILogEventSink implementation

    public void Emit(LogEvent logEvent) {
      var message = FormatProvider != null
        ? new TelegramMessage(logEvent.RenderMessage(FormatProvider))
        : RenderMessageImplementation(logEvent);
      SendMessage(_token, _chatId, message);
    }

    #endregion

    protected static TelegramMessage RenderMessage(LogEvent logEvent) {
      var sb = new StringBuilder();
      sb.AppendLine($"{getEmoji(logEvent)} {logEvent.RenderMessage()}");
      var sourceContext = logEvent.Properties.ContainsKey("SourceContext") ? logEvent.Properties["SourceContext"]?.ToString() : null;
      if (sourceContext != null)
        sb.AppendLine($"\r\nSource context: *{sourceContext.Replace("\"", "")}*");

      if (logEvent.Exception != null) {
        sb.AppendLine($"\r\n*{logEvent.Exception.Message}*\r\n");
        sb.AppendLine($"Message: `{logEvent.Exception.Message}`");
        sb.AppendLine($"Type: `{logEvent.Exception.GetType().Name}`\r\n");
        sb.AppendLine($"Stack Trace```\r\n{logEvent.Exception.ToString()}\r\n```");
      }

      return new TelegramMessage(sb.ToString());
    }

    private static string getEmoji(LogEvent log) {
      switch (log.Level) {
        case LogEventLevel.Debug:
          return "👉";
        case LogEventLevel.Error:
          return "❗";
        case LogEventLevel.Fatal:
          return "‼";
        case LogEventLevel.Information:
          return "ℹ";
        case LogEventLevel.Verbose:
          return "⚡";
        case LogEventLevel.Warning:
          return "⚠";
        default:
          return "";
      }
    }

    protected void SendMessage(string token, string chatId, TelegramMessage message) {
      SelfLog.WriteLine($"Trying to send message to chatId '{chatId}': '{message}'.");

      var client = new TelegramClient(token, 5);

      var sendMessageTask = client.PostAsync(message, chatId);
      Task.WaitAll(sendMessageTask);

      var sendMessageResult = sendMessageTask.Result;
      if (sendMessageResult != null)
        SelfLog.WriteLine($"Message sent to chatId '{chatId}': '{sendMessageResult.StatusCode}'.");
    }
  }
}