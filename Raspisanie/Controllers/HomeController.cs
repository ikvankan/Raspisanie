using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Raspisanie.Data;
using Raspisanie.Models;
using Raspisanie.Models.ViewModels;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

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

            


            IEnumerable<Models.Group> groupList = _db.Group.ToList();
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

                // Создание нового объекта Placement и добавление его в базу данных
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
            foreach(var PLVM in placementVMs)
            {
                var obj = _db.Placement.Find(PLVM.Placement.Id);
                if (obj == null) 
                { PLVM.Placement.Id = 0; }
                
            }
            
            foreach (var placement in placementVMs)
            {
                var PlacementsToDelete = _db.Placement.Where(p => p.Date == placement.Placement.Date).ToList();
                foreach(var pl in PlacementsToDelete)
                {
                    _db.Placement.RemoveRange(pl);
                }
            }
            foreach (var placementVM in placementVMs)
            {
                
                _db.Placement.Add(placementVM.Placement);

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


        
        public IActionResult GenerateAll(DateTime DateToGenerate)
        {
            IEnumerable<Models.Group> GroupList = _db.Group.ToList();
            Random rnd = new Random();
            GroupList = GroupList.OrderBy(x => rnd.Next()).ToList();
            IEnumerable<Predmet> PredmetList = _db.Predmet.ToList();
            IEnumerable<Auditoria> AuditoriaList = _db.Auditoria.ToList();
            IEnumerable<Teacher> TeacherList = _db.Teacher.ToList();
            List<PlacementVM> newPlacements = new List<PlacementVM>();
            
            int numOfPredmet = 0;
            switch (DateToGenerate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    numOfPredmet = 3;
                    break;
                case DayOfWeek.Tuesday:
                    numOfPredmet = 4;
                    break;
                case DayOfWeek.Wednesday:
                    numOfPredmet = 3;
                    break;
                case DayOfWeek.Thursday:
                    numOfPredmet = 4;
                    break;
                case DayOfWeek.Friday:
                    numOfPredmet = 3;
                    break;
                case DayOfWeek.Saturday:
                    numOfPredmet = 2;
                    break;
                default:
                    throw new ArgumentException("error");
            }
            bool CanGen = false;
            for (int i = 0; i < numOfPredmet; i++) 
            {
                foreach (var group in GroupList)
                {

                    int numG = 0;
                    while (!CanGen)
                    {
                        
                        IEnumerable<Predmet> filteredPredmetList = PredmetList.Where(p => p.GroupId == group.Id).ToList();
                        Random random = new Random();
                        
                        // Выбор случайного предмета

                        int randomIndex = random.Next(filteredPredmetList.Count()) + 1;

                        int[] Ids = new int[filteredPredmetList.Count()];
                        int[] Auds = new int[filteredPredmetList.Count()];

                        int id_index = 0;
                        foreach (var predmet in filteredPredmetList)
                        {
                            Ids[id_index] = predmet.Id;
                            Auds[id_index] = predmet.Group.AuditoriaId;

                            id_index++;
                        }

                        int randomnum = random.Next(0, Ids.Length);
                        int subjectId = Ids[randomnum];



                        var generatedPredmet = _db.Predmet.FirstOrDefault(u => u.Id == subjectId);


                        int firstAud = generatedPredmet.Group.AuditoriaId;
                        int secondAud = firstAud;

                        if (generatedPredmet.Laboratory == true)
                        {
                            firstAud = generatedPredmet.Teacher.AuditoryId;
                            secondAud = generatedPredmet.SecondTeacher.AuditoryId;
                        }

                        int firstTeacher = generatedPredmet.TeacherId;
                        int secondTeacher = generatedPredmet.SecondTeacherId;






                        IEnumerable<Placement> DBPL = _db.Placement.ToList();
                        int randomID = random.Next();
                        bool flag = true;
                        while (flag)
                        {
                            if (DBPL.Count() == 0)
                            {
                                flag = false;
                            }
                            foreach (var pl in DBPL)
                            {
                                if (pl.Id == randomID) randomID = random.Next();
                                else flag = false;
                            }
                        }


                        
                        Placement placement = new Placement
                        {
                            Id = randomID,
                            GroupId = group.Id,
                            PredmetId = generatedPredmet.Id,
                            SecondPredmetId = generatedPredmet.Id,
                            Date = DateToGenerate.ToShortDateString().ToString(),
                            index = i+1,
                            AuditoriaId = firstAud,
                            SecondAuditoriaId = secondAud,
                            TeacherId = firstTeacher,
                            SecondTeacherId = secondTeacher,
                        };
                        _db.Placement.Add(placement);
                        PlacementVM placementVM = new PlacementVM()
                        {
                            Placement = placement,
                            NumOfPredmets = numOfPredmet,
                            GroupSelectList = _db.Group.Select(i => new SelectListItem
                            {
                                Text = i.Name,
                                Value = i.Id.ToString()
                            }),
                            PredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Select(i => new SelectListItem
                            {
                                Text = i.PredmetName,
                                Value = i.Id.ToString()
                            }),
                            SecondPredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Where(p => p.Laboratory == true).Select(i => new SelectListItem
                            {
                                Text = i.PredmetName,
                                Value = i.Id.ToString()
                            }),
                            AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                            {
                                Text = i.AuditoryName,
                                Value = i.Id.ToString()
                            }),
                            TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                            {
                                Text = i.TeacherName,
                                Value = i.Id.ToString()
                            }),
                        };

                        if ((newPlacements.Any(p => p.Placement.TeacherId == placement.TeacherId && p.Placement.index == placement.index))|| (newPlacements.Any(p => p.Placement.AuditoriaId == placement.AuditoriaId && p.Placement.index == placement.index))|| (newPlacements.Any(p => p.Placement.SecondTeacherId == placement.SecondTeacherId && p.Placement.index == placement.index))|| (newPlacements.Any(p => p.Placement.SecondAuditoriaId == placement.SecondAuditoriaId && p.Placement.index == placement.index))|| (newPlacements.Any(p => p.Placement.SecondTeacherId == placement.TeacherId && p.Placement.index == placement.index))|| (newPlacements.Any(p => p.Placement.SecondAuditoriaId == placement.AuditoriaId && p.Placement.index == placement.index))|| (newPlacements.Count(p => p.Placement.PredmetId == placement.PredmetId && p.Placement.GroupId == placement.GroupId) >= 2))
                        {
                            if (numG > 1000)
                            {
                                CanGen = true;
                                numG = 0;
                            }
                            else
                            {
                                CanGen = false;
                                _db.Placement.Remove(placement);
                                numG++;
                                
                            }
                            
                            
                        }
                        else
                        {
                            CanGen= true;
                            newPlacements.Add(placementVM);
                        }
                        
                    }
                    if (CanGen) { CanGen = false; }

                }
                

            }


            // Извлекаем все записи Placement из placementVMs
            List<Placement> Placements = newPlacements.Select(p => p.Placement).ToList();

            // Сортируем записи по GroupId и Index
            IEnumerable<Placement> sortedPlacements = Placements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();



            // Считаем количество записей для каждого GroupId
            var groupCounts = Placements.GroupBy(p => p.GroupId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var obj in sortedPlacements)
            {
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);
                obj.Predmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.PredmetId);
                obj.SecondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.SecondPredmetId);
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);
                obj.SecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.SecondAuditoriaId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
                obj.Group.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.Group.AuditoriaId);
                obj.Group.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.Group.TeacherId);
            }

            List<PlacementVM> placementList = new List<PlacementVM>();

            foreach (var placement in sortedPlacements)
            {
                var firstPredmet = _db.Predmet.FirstOrDefault(u => u.Id == placement.PredmetId);
                var secondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == placement.SecondPredmetId);

                if (!firstPredmet.Laboratory)
                {
                    secondPredmet = firstPredmet;
                }
                if (firstPredmet.Laboratory && !secondPredmet.Laboratory)
                {
                    secondPredmet = firstPredmet;
                }

                var findedFirstTeacher = _db.Teacher.FirstOrDefault(u => u.Id == firstPredmet.TeacherId);
                var findedSecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == secondPredmet.SecondTeacherId);


                var findedFirstAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == firstPredmet.Group.AuditoriaId);
                var findedSecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == secondPredmet.Group.AuditoriaId);
                if (firstPredmet.Laboratory)
                {
                    findedFirstAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == findedFirstTeacher.AuditoryId);
                    findedSecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == findedSecondTeacher.AuditoryId);
                }
                placement.TeacherId = findedFirstTeacher.Id;
                placement.SecondTeacherId = findedSecondTeacher.Id;
                placement.AuditoriaId = findedFirstAuditoria.Id;
                placement.SecondAuditoriaId = findedSecondAuditoria.Id;
                placement.PredmetId = firstPredmet.Id;
                placement.SecondPredmetId = secondPredmet.Id;

                placement.PredmetId = placement.PredmetId;
                PlacementVM placementVM = new PlacementVM()
                {
                    NumOfPredmets = groupCounts[placement.GroupId], // Устанавливаем значение NumOfPredmets
                    Placement = placement,
                    GroupSelectList = _db.Group.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString(),

                    }),
                    SecondPredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Where(p => p.Laboratory == true).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                    TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                    {
                        Text = i.TeacherName,
                        Value = i.Id.ToString()
                    }),
                };
                placementList.Add(placementVM);

            }


            return View(placementList);
        }
        public string GetTeacherName(int predmetId)
        {
            // Load the Predmet from the database
            var predmet = _db.Predmet.FirstOrDefault(p=>p.Id==predmetId);
            var teacher = _db.Teacher.FirstOrDefault(pl => pl.Id == predmet.TeacherId);
            return teacher.TeacherName;
        }


        public string GetSecondTeacherName(int predmetId)
        {
            // Load the Predmet from the database
            var predmet = _db.Predmet.FirstOrDefault(p => p.Id == predmetId);
            var teacher = _db.Teacher.FirstOrDefault(pl => pl.Id == predmet.SecondTeacherId);
            return teacher.TeacherName;
        }


        public IActionResult ShowAll(DateTime DateToShow)
        {
            // Получаем записи и сортируем по GroupId и Index
            IEnumerable<Placement> Placements = _db.Placement
                .Where(p => p.Date == DateToShow.ToShortDateString())
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();

            // Считаем количество записей для каждого GroupId
            var groupCounts = Placements.GroupBy(p => p.GroupId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var obj in Placements)
            {
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);
                obj.Predmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.PredmetId);
                obj.SecondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.SecondPredmetId);
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);
                obj.SecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.SecondAuditoriaId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
                obj.Group.Auditoria = _db.Auditoria.FirstOrDefault(u=>u.Id==obj.Group.AuditoriaId);
                obj.Group.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.Group.TeacherId);
            }

            List<PlacementVM> placementList = new List<PlacementVM>();

            foreach (var placement in Placements)
            {

                PlacementVM placementVM = new PlacementVM()
                {
                    
                    NumOfPredmets = groupCounts[placement.GroupId], // Устанавливаем значение NumOfPredmets
                    Placement = placement,
                    GroupSelectList = _db.Group.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    SecondPredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Where(p => p.Laboratory == true).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                    TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                    {
                        Text = i.TeacherName,
                        Value = i.Id.ToString()
                    }),
                };
                placementList.Add(placementVM);
            }

            return View(placementList);
        }
        
        [HttpPost]
        public IActionResult DeletePlacement(/*string GroupId, int Index, DateTime DateToShow*/ List<PlacementVM> placementVMs, int? Id)
        {

            


            

            Placement placementToDelete = _db.Placement.FirstOrDefault(p => p.Id == Id);


            // Извлекаем все записи Placement из placementVMs
            List<Placement> placements = placementVMs.Select(p => p.Placement).ToList();


            placements.Remove(placements.FirstOrDefault(p=>p.Id==Id));
            


            IEnumerable<Placement> sortedPlacements = placements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();
            // Считаем количество записей для каждого GroupId
            var groupCounts = placements.GroupBy(p => p.GroupId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var obj in sortedPlacements)
            {
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);
                obj.Predmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.PredmetId);
                obj.SecondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.SecondPredmetId);
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);
                obj.SecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.SecondAuditoriaId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
                obj.Group.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.Group.AuditoriaId);
                obj.Group.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.Group.TeacherId);
            }

            List<PlacementVM> placementList = new List<PlacementVM>();

            foreach (var placement in sortedPlacements)
            {
                placement.PredmetId = placement.PredmetId;
                PlacementVM placementVM = new PlacementVM()
                {
                    NumOfPredmets = groupCounts[placement.GroupId], // Устанавливаем значение NumOfPredmets
                    Placement = placement,
                    GroupSelectList = _db.Group.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString(),

                    }),
                    SecondPredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Where(p => p.Laboratory == true).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                    TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                    {
                        Text = i.TeacherName,
                        Value = i.Id.ToString()
                    }),
                };
                placementList.Add(placementVM);

            }















            //// Найдите и удалите запись
            //var placement = _db.Placement.FirstOrDefault(p => p.Group.Name == GroupId && p.index == Index && p.Date == DateToShow.ToShortDateString());
            //if (placement != null)
            //{
            //    _db.Placement.Remove(placement);
            //    _db.SaveChanges();
            //}

            //// Получите обновленные данные и верните их обратно на страницу
            //return RedirectToAction("ShowAll", new { DateToShow = DateToShow });

            ModelState.Clear();
            return View("ShowAll", placementList);
        }
        [HttpPost]
        public IActionResult AddPlacement(/*string GroupId, DateTime DateToShow,*/ List<PlacementVM> placementVMs, int? Id)
        {
            int indexI = 1;
            
            
            Random random = new Random();

            // Извлекаем все записи Placement из placementVMs
            List<Placement> placements = placementVMs.Select(p => p.Placement).ToList();
            Placement placementToCreate = placements.FirstOrDefault(p => p.Id == Id);

            foreach (var pl in placements)
            {
                if (pl.GroupId == placementToCreate.GroupId)
                {
                    indexI++;
                }
            }
            IEnumerable<Placement> DBPL = _db.Placement.ToList();
            int randomID = random.Next();
            bool flag = true;
            while (flag)
            {
                if (DBPL.Count() == 0)
                {
                    flag = false;
                }
                foreach (var pl in DBPL)
                {
                    if(pl.Id == randomID)randomID = random.Next();
                    else flag=false;
                }
            }
            Placement newPlacement = new Placement
            {
                Id = randomID,
                GroupId = placementToCreate.GroupId,
                PredmetId = placementToCreate.PredmetId,
                SecondPredmetId = placementToCreate.SecondPredmetId,
                Date = placementToCreate.Date,
                index = indexI,
                AuditoriaId = placementToCreate.AuditoriaId,
                SecondAuditoriaId = placementToCreate.SecondAuditoriaId,
                TeacherId = placementToCreate.TeacherId,
                SecondTeacherId = placementToCreate.SecondTeacherId,




                //Id = randomID,
                //GroupId = placementToCreate.GroupId,
                //PredmetId = placementToCreate.PredmetId,
                //Date = placementToCreate.Date,
                //index = indexI,
                //AuditoriaId = placementToCreate.AuditoriaId
            };
            placements.Add(newPlacement);


            IEnumerable<Placement> sortedPlacements = placements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();
            // Считаем количество записей для каждого GroupId
            var groupCounts = placements.GroupBy(p => p.GroupId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var obj in sortedPlacements)
            {
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);
                obj.Predmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.PredmetId);
                obj.SecondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.SecondPredmetId);
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);
                obj.SecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.SecondAuditoriaId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
                obj.Group.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.Group.AuditoriaId);
                obj.Group.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.Group.TeacherId);
            }

            List<PlacementVM> placementList = new List<PlacementVM>();

            foreach (var placement in sortedPlacements)
            {
                

                placement.PredmetId = placement.PredmetId;
                PlacementVM placementVM = new PlacementVM()
                {
                    NumOfPredmets = groupCounts[placement.GroupId], // Устанавливаем значение NumOfPredmets
                    Placement = placement,
                    GroupSelectList = _db.Group.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString(),

                    }),
                    SecondPredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Where(p => p.Laboratory == true).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                    TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                    {
                        Text = i.TeacherName,
                        Value = i.Id.ToString()
                    }),
                };
                placementList.Add(placementVM);

            }

            ModelState.Clear();
            return View("ShowAll", placementList);
            //IEnumerable<Predmet> PredmetList = _db.Predmet.ToList();
            //var Group = _db.Group.FirstOrDefault(p=>p.Name==GroupId);
            //IEnumerable<Predmet> filteredPredmetList = PredmetList.Where(p => p.GroupId == Group.Id).ToList();
            

            //Placement placement = new Placement
            //{
            //    GroupId = Group.Id,
            //    PredmetId = filteredPredmetList.FirstOrDefault().Id,
            //    Date = DateToShow.ToShortDateString(),
            //    index = 1,
            //    AuditoriaId = Group.AuditoriaId
            //};
            //_db.Placement.Add(placement);
            //_db.SaveChanges();
            //// Получите обновленные данные и верните их обратно на страницу
            //return RedirectToAction("ShowAll", new { DateToShow = DateToShow });
        }


        [HttpPost]
        public IActionResult Refresh(List<PlacementVM> placementVMs, int? Id)
        {
            
            // Извлекаем все записи Placement из placementVMs
            List<Placement> Placements = placementVMs.Select(p => p.Placement).ToList();

            // Сортируем записи по GroupId и Index
            IEnumerable<Placement> sortedPlacements = Placements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();



            // Считаем количество записей для каждого GroupId
            var groupCounts = Placements.GroupBy(p => p.GroupId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var obj in sortedPlacements)
            {
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);
                obj.Predmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.PredmetId);
                obj.SecondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.SecondPredmetId);
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);
                obj.SecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.SecondAuditoriaId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
                obj.Group.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.Group.AuditoriaId);
                obj.Group.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.Group.TeacherId);
            }

            List<PlacementVM> placementList = new List<PlacementVM>();

            foreach (var placement in sortedPlacements)
            {
                if (placement.Id == Id)
                {
                    var firstPredmet = _db.Predmet.FirstOrDefault(u => u.Id == placement.PredmetId);
                    var secondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == placement.SecondPredmetId);

                    if (!firstPredmet.Laboratory)
                    {
                        secondPredmet = firstPredmet;
                    }
                    if (firstPredmet.Laboratory && !secondPredmet.Laboratory)
                    {
                        secondPredmet = firstPredmet;
                    }

                    var findedFirstTeacher = _db.Teacher.FirstOrDefault(u => u.Id == firstPredmet.TeacherId);
                    var findedSecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == secondPredmet.SecondTeacherId);


                    var findedFirstAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == firstPredmet.Group.AuditoriaId);
                    var findedSecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == secondPredmet.Group.AuditoriaId);
                    if (firstPredmet.Laboratory)
                    {
                        findedFirstAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == findedFirstTeacher.AuditoryId);
                        findedSecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == findedSecondTeacher.AuditoryId);
                    }
                    placement.TeacherId = findedFirstTeacher.Id;
                    placement.SecondTeacherId = findedSecondTeacher.Id;
                    placement.AuditoriaId = findedFirstAuditoria.Id;
                    placement.SecondAuditoriaId = findedSecondAuditoria.Id;
                    placement.PredmetId = firstPredmet.Id;
                    placement.SecondPredmetId = secondPredmet.Id;

                    placement.PredmetId = placement.PredmetId;
                }
                
                PlacementVM placementVM = new PlacementVM()
                {
                    NumOfPredmets = groupCounts[placement.GroupId], // Устанавливаем значение NumOfPredmets
                    Placement = placement,
                    GroupSelectList = _db.Group.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString(),
                        
                    }),
                    SecondPredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Where(p => p.Laboratory == true).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                    TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                    {
                        Text = i.TeacherName,
                        Value = i.Id.ToString()
                    }),
                };
                placementList.Add(placementVM);
                
            }


            ModelState.Clear();
            return View("ShowAll", placementList);
        }

        [HttpGet]
        public IActionResult CheckRecord(int day, int month, int year)
        {
            var date = new DateTime(year, month, day);
            var hasRecord = _db.Placement.Any(p => p.Date == date.ToShortDateString()) ;
            return Json(new { hasRecord });
        }

        [HttpPost]
        public IActionResult CheckError(List<PlacementVM> placementVMs)
        {
            List<Placement> placements = placementVMs.Select(p => p.Placement).ToList();

            IEnumerable<Placement> sortedPlacements = placements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();
            // Считаем количество записей для каждого GroupId
            var groupCounts = placements.GroupBy(p => p.GroupId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var obj in sortedPlacements)
            {
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);
                obj.Predmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.PredmetId);
                obj.SecondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.SecondPredmetId);
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);
                obj.SecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.SecondAuditoriaId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
                obj.Group.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.Group.AuditoriaId);
                obj.Group.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.Group.TeacherId);
            }

            List<PlacementVM> placementList = new List<PlacementVM>();
            
            foreach (var placement in sortedPlacements)
            {
                bool teacherError = false;
                bool SteacherError = false;
                bool auditoryError = false;
                bool SauditoryError = false;

                var otherPlacements = sortedPlacements.Where(p => p != placement);

                if (otherPlacements.Any(p => p.TeacherId == placement.TeacherId && p.index == placement.index) ||
                    otherPlacements.Any(p => p.SecondTeacherId == placement.TeacherId && p.index == placement.index))
                {
                    teacherError = true;
                }
                if (otherPlacements.Any(p => p.SecondTeacherId == placement.SecondTeacherId && p.index == placement.index) ||
                    otherPlacements.Any(p => p.TeacherId == placement.SecondTeacherId && p.index == placement.index))
                {
                    SteacherError = true;
                }
                if (otherPlacements.Any(p => p.AuditoriaId == placement.AuditoriaId && p.index == placement.index) ||
                    otherPlacements.Any(p => p.SecondAuditoriaId == placement.AuditoriaId && p.index == placement.index))
                {
                    auditoryError = true;
                }
                if (otherPlacements.Any(p => p.SecondAuditoriaId == placement.SecondAuditoriaId && p.index == placement.index) ||
                    otherPlacements.Any(p => p.AuditoriaId == placement.SecondAuditoriaId && p.index == placement.index))
                {
                    SauditoryError = true;
                }

                placement.PredmetId = placement.PredmetId;
                PlacementVM placementVM = new PlacementVM()
                {
                    AudithoryError = auditoryError,
                    SECAudithoryError=SauditoryError,
                    TeacherError=teacherError,
                    SECTeacherError=SteacherError,
                    NumOfPredmets = groupCounts[placement.GroupId], // Устанавливаем значение NumOfPredmets
                    Placement = placement,
                    GroupSelectList = _db.Group.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString(),

                    }),
                    SecondPredmetSelectList = _db.Predmet.Where(p => p.GroupId == placement.GroupId).Where(p => p.Laboratory == true).Select(i => new SelectListItem
                    {
                        Text = i.PredmetName,
                        Value = i.Id.ToString()
                    }),
                    AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                    TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                    {
                        Text = i.TeacherName,
                        Value = i.Id.ToString()
                    }),
                };
                placementList.Add(placementVM);

            }

            ModelState.Clear();
            return View("ShowAll", placementList);
        }

    }
}


