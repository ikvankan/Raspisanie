using Microsoft.AspNetCore.Mvc;

namespace Raspisanie.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly TelegramBotService _telegramBotService;

        public BotController(TelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        [HttpGet]
        [Route("/SendMessageToBot")]
        public async Task<IActionResult> SendMessageToBot(string chatId,string message)
        {
            await _telegramBotService.SendMessageAsync(chatId, message);
            return Ok(new { status = "Message sent" });
        }
        
        
        [HttpGet]
        [Route("/SendFileToBot")]
        public async Task<IActionResult> SendFileToBot(string chatId,string filePath)
        {
            await _telegramBotService.SendDocumentAsync(chatId, filePath);
            return Ok(new { status = "Message sent" });
        }
    }

}
