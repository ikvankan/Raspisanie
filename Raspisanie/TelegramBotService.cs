namespace Raspisanie
{
    using System.IO;
    using System.IO.Pipes;
    using Telegram.Bot;
    using Telegram.Bot.Types;
    using Telegram.Bot.Types.InputFiles;

    public class TelegramBotService
    {
        private readonly TelegramBotClient _botClient;
        

        public TelegramBotService(string botToken)
        {
            _botClient = new TelegramBotClient(botToken);
        }

        public async Task SendMessageAsync(string chatId, string message)
        {
            await _botClient.SendTextMessageAsync(chatId, message);
            
        }

        public async Task SendDocumentAsync(string chatId, string filePath)
        {
            using (var fileStream = System.IO.File.OpenRead(filePath))
            {
                var inputFile = new InputOnlineFile(fileStream);
                await _botClient.SendDocumentAsync(chatId, inputFile, caption: Path.GetFileName(filePath));
            }
        }




    }

}
