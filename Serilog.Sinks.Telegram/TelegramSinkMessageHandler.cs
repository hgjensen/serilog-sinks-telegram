using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Sinks.Telegram.Client;

namespace Serilog.Sinks.Telegram {
  public delegate void TelegramIncomingMessageDelegate(List<TelegramUpdate> updates);

  public class TelegramSinkMessageHandler : IDisposable {
    public event TelegramIncomingMessageDelegate UpdatesReceived;

    private static readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly string _token;
    private readonly string _chatId;
    private static readonly object listenThreadLocker = new object();
    private static Thread _listenThread;
    private int _lastMessageId;

    public TelegramSinkMessageHandler(string token, string chatId) {
      if (string.IsNullOrWhiteSpace(token))
        throw new ArgumentNullException(nameof(token));

      _token = token;
      _chatId = chatId;
      lock (listenThreadLocker)
        if (_listenThread == null) {
          _listenThread = new Thread(listen);
          _listenThread.Start();
        }
    }

    private void listen() {
      while (!cts.Token.IsCancellationRequested) {
        try {
          var client = new TelegramClient(_token, 5);
          var updates = client.GetUpdates(_lastMessageId, cts.Token).Result;
          if (updates != null && updates.Count > 0) {
            _lastMessageId = updates.Max(p => p.UpdateId) + 1;
            updates.ForEach(p => p.ReplyAction = s => Task.Run(() => client.PostAsync(new TelegramMessage(s), _chatId)));
            UpdatesReceived?.Invoke(updates);
          }

          if (cts.Token.IsCancellationRequested) break;
        } catch (OperationCanceledException) {
          return;
        } catch {
          if (cts.Token.IsCancellationRequested)
            return;
          //Just wait a minute ... and retry
          Thread.Sleep(TimeSpan.FromMinutes(1));
        }
      }
    }

    public void Dispose() {
      cts.Cancel();
      for (int i = 0; i < 8; i++) {
        if (_listenThread.IsAlive)
          Thread.Sleep(250);
      }

      if (_listenThread.IsAlive)
        try {
          _listenThread.Abort();
        } catch {
          // ignored
        }
    }
  }
}