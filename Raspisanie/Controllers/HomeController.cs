using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Raspisanie.Data;
using Raspisanie.Models;
using Raspisanie.Models.ViewModels;
using System.Diagnostics;

namespace Raspisanie.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly TelegramBotService _telegramBotService;

        private readonly ApplicationDbContext _db;
        

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, TelegramBotService telegramBotService)
        {
            _logger = logger;
            _db = db;
            _telegramBotService = telegramBotService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        [HttpPost]
        public IActionResult Generate(string groupNumber)
        {

            


            IEnumerable<Group> groupList = _db.Group.ToList();
            IEnumerable<Predmet> predmetList = _db.Predmet.ToList();
            IEnumerable<Auditoria> AuditoriaList = _db.Auditoria.ToList();


            List<PlacementVM> newPlacements = new List<PlacementVM>();

            // Получение текущей даты
            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");

            // Создание имени файла
            string fileName = $"Расписание на {currentDate}.txt";

            // Путь к файлу
            string path = Path.Combine(@"C:\Users\User\Desktop\DP\Raspisanie\Raspisanie\Files", fileName);

            // Получение Id выбранной группы из базы данных
            var group = groupList.FirstOrDefault(g => g.Name == groupNumber);
            if (group == null)
            {
                return View(); // или другой вид обработки ошибок
            }
            int groupId = group.Id;



            IEnumerable<Predmet> filteredPredmetList = predmetList.Where(p => p.GroupId == groupId).ToList();
            // Генерация случайного числа от 1 до 3


            // Создание записей в зависимости от дня недели
            int numberOfEntries = DateTime.Now.DayOfWeek == DayOfWeek.Friday ? 4 : 3;

            for (int i = 0; i < numberOfEntries; i++)
            {
                Random random = new Random();
                int randomNumber = i+1;
                // Выбор случайного предмета
                int randomIndex = random.Next(filteredPredmetList.Count())+1;

                int[] Ids = new int[filteredPredmetList.Count()];
                int[] Auds = new int[filteredPredmetList.Count()];

                int id_index = 0;
                foreach (var predmet in filteredPredmetList)
                {
                    Ids[id_index] = predmet.Id;
                    Auds[id_index] = predmet.Group.AuditoriaId;
                    id_index++;
                }

                //var randomSubject = predmetList.FirstOrDefault(g=>g.PredmetName == "Физика");
                int randomnum = random.Next(0, Ids.Length);
                int subjectId = Ids[randomnum];
                int auditoriaId = Auds[randomnum];

                // Создание строки для записи в файл
                string entry = $"Номер группы: {groupNumber}, Дата: {currentDate}, Id группы: {groupId}, Id предмета: {subjectId}, Случайное число: {randomNumber}\n";

                // Добавление строки в файл
                System.IO.File.AppendAllText(path, entry);

                // Создание нового объекта Schedule и добавление его в базу данных
                Placement placement = new Placement
                {
                    GroupId = groupId,
                    PredmetId = subjectId,
                    Date = DateTime.Now.ToString(),
                    index = randomNumber,
                    AuditoriaId = auditoriaId
                };
                _db.Placement.Add(placement);
                PlacementVM placementVM = new PlacementVM()
                {
                    Placement = placement,
                    GroupSelectList = _db.Group.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PredmetSelectList = _db.Predmet.Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    })
                };

                // Добавление нового ViewModel в список
                newPlacements.Add(placementVM);
            }

            
            

            return View(newPlacements);
        }



        [HttpPost]
        public IActionResult SaveSchedules(List<PlacementVM> placementVMs)
        {
            
            
            foreach (var placementVM in placementVMs)
            {
                
                // Если Id Placement равен нулю, это новая запись, иначе - обновление существующей
                if (placementVM.Placement.Id == 0)
                {
                    _db.Placement.Add(placementVM.Placement);
                }
                else
                {
                    _db.Placement.Update(placementVM.Placement);
                }
            }
            _db.SaveChanges();
            string chatId = "486450728";
            string message = "Сохраненные данные:\n";
            foreach (var placementVM in placementVMs)
            {
                var group = _db.Group.FirstOrDefault(g => g.Id == placementVM.Placement.GroupId);
                var predmet = _db.Predmet.FirstOrDefault(p => p.Id == placementVM.Placement.PredmetId);
                var auditoria = _db.Auditoria.FirstOrDefault(p => p.Id == placementVM.Placement.AuditoriaId);
                message += $"Группа: {group.Name}, Дисциплина: {predmet.PredmetName}, Date: {placementVM.Placement.Date}, Index: {placementVM.Placement.index}, Аудитория: {auditoria.AuditoryName}\n";
            }

            // Отправка сообщения боту

            SendMessageToBot(chatId, message);
            return RedirectToAction("Index");

        }

        [HttpGet]
        [Route("/SendMessageToBot")]
        public async Task<IActionResult> SendMessageToBot(string chatId, string message)
        {
            await _telegramBotService.SendMessageAsync(chatId, message);
            return Ok(new { status = "Message sent" });
        }

    }
}