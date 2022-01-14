using System;
using Serilog.Configuration;
using Serilog.Events;

namespace Serilog.Sinks.Telegram {
  public static class TelegramSinkExtension {
    public static LoggerConfiguration Telegram(
      this LoggerSinkConfiguration loggerConfiguration,
      string token,
      string chatId,
      TelegramSink.RenderMessageMethod renderMessageImplementation = null,
      LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
      IFormatProvider formatProvider = null
    ) {
      if (loggerConfiguration == null)
        throw new ArgumentNullException(nameof(loggerConfiguration));

      return loggerConfiguration.Sink(
        new TelegramSink(
          chatId,
          token,
          renderMessageImplementation,
          formatProvider
        ),
        restrictedToMinimumLevel);
    }

    public static LoggerConfiguration Telegram(
      this LoggerSinkConfiguration loggerConfiguration,
      string token,
      string chatId,
      TelegramIncomingMessageDelegate incomingMessageHandler,
      out IDisposable handle,
      TelegramSink.RenderMessageMethod renderMessageImplementation = null,
      LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
      IFormatProvider formatProvider = null
    ) {
      if (loggerConfiguration == null)
        throw new ArgumentNullException(nameof(loggerConfiguration));

      var listener = new TelegramSinkMessageHandler(token, chatId);
      listener.UpdatesReceived += incomingMessageHandler;
      handle = listener;

      return loggerConfiguration.Sink(
        new TelegramSink(
          chatId,
          token,
          renderMessageImplementation,
          formatProvider
        ),
        restrictedToMinimumLevel);
    }
  }
}