namespace Serilog.Sinks.Telegram.Client
{
    public sealed class TelegramMessage
    {
        public TelegramMessage(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}