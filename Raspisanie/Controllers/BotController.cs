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
        public async Task<IActionResult> SendMessageToBot()
        {
            string chatId = "486450728";
            string message = "Привет, я отправил сообщение через ASP.NET Core!";

            await _telegramBotService.SendMessageAsync(chatId, message);
            await _telegramBotService.SendDocumentAsync(chatId, "C:\\Users\\User\\Desktop\\DP\\Raspisanie\\Raspisanie\\Files\\БД курсач.docx");
            return Ok(new { status = "Message sent" });
        }
    }

}
