using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog.Sinks.Telegram.Client;

namespace Serilog.Sinks.Telegram.Example {
  class Program {
    private static CancellationTokenSource _cts = new CancellationTokenSource();
    static IDisposable _telegramHandle;

    static void Main(string[] args) {
      var log = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Telegram("", ""),
          ListenerOnUpdatesReceived, out _telegramHandle)
        .CreateLogger();

      log.Information("This is an information message!");

      var listener = new TelegramSinkMessageHandler("", "");
      listener.UpdatesReceived += ListenerOnUpdatesReceived;

      //try {
      //  log.Verbose("This is an verbose message!");
      //  log.Debug("This is an debug message!");
      //  log.Information("This is an information message!");
      //  log.Warning("This is an warning message!");
      //  log.Error("This is an error message!");
      //  throw new Exception("This is an exception!");
      //} catch (Exception exception) {
      //  log.Fatal(exception, "Exception catched at Main.");
      //}

      _cts.Token.WaitHandle.WaitOne();
      _telegramHandle?.Dispose();
      Console.WriteLine("Done.");
    }

    private static void ListenerOnUpdatesReceived(List<TelegramUpdate> updates) {
      foreach (var upd in updates) {
        string msg = upd.Message != null ? upd.Message.Text : upd.ChannelPost?.Text ?? "-";
        string firstWord = msg.Split(' ').FirstOrDefault()?.ToUpperInvariant() ?? string.Empty;
        switch (firstWord) {
          case "/HELP":
            upd.Reply("Så er der hjælp på vej...").Wait();
            break;
          case "/QUIT":
            upd.Reply("Exiting ... ;-)").Wait();
            _cts.Cancel();
            break;
          default:
            Console.WriteLine("=> " + msg);
            break;
        }
      }
    }
  }
}