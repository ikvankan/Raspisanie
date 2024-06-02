using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Raspisanie.Data;
using Raspisanie.Models;
using Raspisanie.Models.ViewModels;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Security.Cryptography;
using IronXL;
using System.Drawing.Imaging;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using System.IO;



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
            List<Placement> placements = placementVMs.Select(p => p.Placement).ToList();
            foreach (var PLVM in placementVMs)
            {
                var obj = _db.Placement.Find(PLVM.Placement.Id);
                if (obj == null) 
                { PLVM.Placement.Id = 0; }
                
            }
            List<Placement> PlacementsToDelete = new List<Placement>();
            foreach (var placement in placementVMs)
            {
                PlacementsToDelete = _db.Placement.Where(p => p.Date == placement.Placement.Date).ToList();
                
            }
            foreach (var pl in PlacementsToDelete)
            {
                Predmet PredmetToPlus = _db.Predmet.FirstOrDefault(p => p.Id == pl.PredmetId);
                Predmet SPredmetToPlus = _db.Predmet.FirstOrDefault(p => p.Id == pl.SecondPredmetId);
                PredmetToPlus.Hours = PredmetToPlus.Hours + 2;
                if(PredmetToPlus != SPredmetToPlus)
                {
                    SPredmetToPlus.Hours = SPredmetToPlus.Hours + 2;
                }
                
                _db.Placement.RemoveRange(pl);
            }
            foreach (var placementVM in placementVMs)
            {
                Predmet PredmetToMinus = _db.Predmet.FirstOrDefault(p => p.Id == placementVM.Placement.PredmetId);
                Predmet SPredmetToMinus = _db.Predmet.FirstOrDefault(p => p.Id == placementVM.Placement.SecondPredmetId);
                PredmetToMinus.Hours = PredmetToMinus.Hours - 2;
                if (PredmetToMinus != SPredmetToMinus)
                {
                    SPredmetToMinus.Hours = SPredmetToMinus.Hours - 2;
                }
                
                _db.Placement.Add(placementVM.Placement);

            }
            _db.SaveChanges();

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
            TempData[WC.Success] = "Сохранено!";
            ModelState.Clear();
            return View("ShowAll", placementList);

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
            
            for (int i = 0; i < numOfPredmet; i++) 
            {
                foreach (var group in GroupList)
                {
                    bool CanGen = false;

                    int numG = 0;
                    while (!CanGen)
                    {
                        
                        IEnumerable<Predmet> filteredPredmetList = PredmetList.Where(p => p.GroupId == group.Id).ToList();
                        Random random = new Random();
                        if(filteredPredmetList.Count() == 0)
                        {
                            CanGen = true;
                            continue;
                        }
                        foreach(var pred in filteredPredmetList)
                        {
                            if(pred.GEN == true|| filteredPredmetList.Count() == 0||pred.Hours<=0)
                            {
                                CanGen = false;
                                continue;
                            }
                        }
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
        public IActionResult DeletePlacement( List<PlacementVM> placementVMs, int? Id)
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
        public IActionResult AddPlacement( List<PlacementVM> placementVMs, int? Id)
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










            List<Placement> Eplacements = placementList.Select(p => p.Placement).ToList();

            IEnumerable<Placement> EsortedPlacements = Eplacements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();
            // Считаем количество записей для каждого GroupId
            var EgroupCounts = Eplacements.GroupBy(p => p.GroupId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var obj in EsortedPlacements)
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

            List<PlacementVM> EplacementList = new List<PlacementVM>();

            foreach (var placement in EsortedPlacements)
            {
                bool teacherError = false;
                bool SteacherError = false;
                bool auditoryError = false;
                bool SauditoryError = false;

                var otherPlacements = EsortedPlacements.Where(p => p != placement);

                if (otherPlacements.Any(p => p.TeacherId == placement.TeacherId && p.index == placement.index) ||
                    otherPlacements.Any(p => p.SecondTeacherId == placement.TeacherId && p.index == placement.index && p.Predmet.Laboratory == true))
                {
                    teacherError = true;
                }
                if (otherPlacements.Any(p => p.SecondTeacherId == placement.SecondTeacherId && p.index == placement.index && p.Predmet.Laboratory == true) ||
                    otherPlacements.Any(p => p.TeacherId == placement.SecondTeacherId && p.index == placement.index))
                {
                    SteacherError = true;
                }
                if (otherPlacements.Any(p => p.AuditoriaId == placement.AuditoriaId && p.index == placement.index && !p.Predmet.NoAud) ||
                    otherPlacements.Any(p => p.SecondAuditoriaId == placement.AuditoriaId && p.index == placement.index && p.Predmet.Laboratory == true && !p.Predmet.NoAud))
                {
                    auditoryError = true;
                }
                if (otherPlacements.Any(p => p.SecondAuditoriaId == placement.SecondAuditoriaId && p.index == placement.index && p.Predmet.Laboratory == true && !p.Predmet.NoAud) ||
                    otherPlacements.Any(p => p.AuditoriaId == placement.SecondAuditoriaId && p.index == placement.index && !p.Predmet.NoAud))
                {
                    SauditoryError = true;
                }
                if (placement.SecondAuditoriaId == placement.AuditoriaId && placement.Predmet.Laboratory == true)
                {
                    auditoryError = true;
                    SauditoryError = true;
                }
                if (placement.SecondTeacherId == placement.TeacherId && placement.Predmet.Laboratory == true)
                {
                    SteacherError = true;
                    teacherError = true;
                }
                placement.PredmetId = placement.PredmetId;
                PlacementVM EplacementVM = new PlacementVM()
                {
                    AudithoryError = auditoryError,
                    SECAudithoryError = SauditoryError,
                    TeacherError = teacherError,
                    SECTeacherError = SteacherError,
                    NumOfPredmets = EgroupCounts[placement.GroupId], // Устанавливаем значение NumOfPredmets
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
                EplacementList.Add(EplacementVM);

            }













































            ModelState.Clear();
            return View("ShowAll", EplacementList);
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
                    otherPlacements.Any(p => p.SecondTeacherId == placement.TeacherId && p.index == placement.index && p.Predmet.Laboratory == true))
                {
                    teacherError = true;
                }
                if (otherPlacements.Any(p => p.SecondTeacherId == placement.SecondTeacherId && p.index == placement.index && p.Predmet.Laboratory == true) ||
                    otherPlacements.Any(p => p.TeacherId == placement.SecondTeacherId && p.index == placement.index))
                {
                    SteacherError = true;
                }
                if (otherPlacements.Any(p => p.AuditoriaId == placement.AuditoriaId && p.index == placement.index && !p.Predmet.NoAud) ||
                    otherPlacements.Any(p => p.SecondAuditoriaId == placement.AuditoriaId && p.index == placement.index && p.Predmet.Laboratory == true && !p.Predmet.NoAud))
                {
                    auditoryError = true;
                }
                if (otherPlacements.Any(p => p.SecondAuditoriaId == placement.SecondAuditoriaId && p.index == placement.index && p.Predmet.Laboratory == true && !p.Predmet.NoAud) ||
                    otherPlacements.Any(p => p.AuditoriaId == placement.SecondAuditoriaId && p.index == placement.index && !p.Predmet.NoAud))
                {
                    SauditoryError = true;
                }
                if (placement.SecondAuditoriaId == placement.AuditoriaId && placement.Predmet.Laboratory == true)
                {
                    auditoryError = true;
                    SauditoryError = true;
                }
                if (placement.SecondTeacherId == placement.TeacherId && placement.Predmet.Laboratory == true)
                {
                    SteacherError = true;
                    teacherError = true;
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

        public async Task<IActionResult> ExportToExcel(List<PlacementVM> model)
        {
            List<Placement> placements = model.Select(p => p.Placement).ToList();

            IEnumerable<Placement> sortedPlacements = placements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();

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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Расписание");

                worksheet.Cells["A1:R200"].Style.Font.Name = "Times New Roman";

                worksheet.Cells["A1"].Value = $"РАСПИСАНИЕ ЗАНЯТИЙ НА {DateTime.ParseExact(model.FirstOrDefault().Placement.Date, "dd.MM.yyyy", null):dd.MM.yyyy} ({DateTime.ParseExact(model.FirstOrDefault().Placement.Date, "dd.MM.yyyy", null):dddd})";
                worksheet.Cells["A1:E1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:E1"].Style.Font.Bold = true;
                worksheet.Cells["A1:E1"].Style.Font.Size = 20;
                worksheet.Cells["A1:E1"].Merge = true;
                worksheet.Cells["A2"].Value = "Номер группы, ауд";
                worksheet.Cells["B2:E2"].Merge = true;
                worksheet.Cells["B2"].Value = "Расписание";
                worksheet.Cells["B2:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 40;
                worksheet.Column(3).Width = 30;
                worksheet.Column(4).Width = 30;
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 1, 2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 1, 2, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[2, 1, 2, 5].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                worksheet.Cells[2, 1, model.Count + 2, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[2, 2, model.Count + 2, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[2, 2, model.Count + 2, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[2, 2, model.Count + 2, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                int row = 3;
                int old = -1;
                int startRow = 3;
                for (int i = 0; i < model.Count; i++)
                {
                    int temp = model[i].Placement.GroupId;
                    if (old != temp)
                    {
                        if (i != 0)
                        {
                            worksheet.Cells[startRow, 1, row - 1, 1].Merge = true;
                            worksheet.Row(startRow).Style.WrapText = true;
                            worksheet.Cells[startRow, 1, row - 1, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        }

                        worksheet.Cells[row, 1].Value = $"{model[i].Placement.Group.Name} ауд. {model[i].Placement.Group.Auditoria.AuditoryName}\nКуратор {model[i].Placement.Group.Teacher.TeacherName}";

                        startRow = row;
                        old = temp;
                    }
                    if (model[i].Placement.Predmet.NoAud)
                    {
                        worksheet.Cells[row, 2].Value = $"{model[i].Placement.index}.     {model[i].Placement.Predmet.PredmetName}   {model[i].Placement.Desc}";
                        worksheet.Cells[row, 4].Value = $"{model[i].Placement.Teacher.TeacherName}";
                    }
                    else
                    {
                        worksheet.Cells[row, 2].Value = $"{model[i].Placement.index}.     {model[i].Placement.Predmet.PredmetName}   {model[i].Placement.Desc}";
                        worksheet.Cells[row, 4].Value = $"{model[i].Placement.Teacher.TeacherName}, {model[i].Placement.Auditoria.AuditoryName}";
                    }
                    

                    if (model[i].Placement.Predmet.Laboratory && (model[i].Placement.SecondPredmet != model[i].Placement.Predmet))
                    {
                        worksheet.Cells[row, 3].Value = $"{model[i].Placement.SecondPredmet.PredmetName}   {model[i].Placement.SDesc}";

                    }
                    if (model[i].Placement.Predmet.Laboratory)
                    {
                        if (model[i].Placement.Predmet.NoAud)
                        {
                            worksheet.Cells[row, 5].Value = $"{model[i].Placement.SecondTeacher.TeacherName}";
                        }
                        else
                        {
                            worksheet.Cells[row, 5].Value = $"{model[i].Placement.SecondTeacher.TeacherName}, {model[i].Placement.SecondAuditoria.AuditoryName}";
                        }
                            
                    }

                    row++;
                }
                worksheet.Cells[startRow, 1, row - 1, 1].Merge = true;
                worksheet.Row(startRow).Style.WrapText = true;
                worksheet.Cells[startRow, 1, row - 1, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                package.Save();
            }

            stream.Position = 0;
            string excelName = $"Расписание-на {DateTime.ParseExact(model.FirstOrDefault().Placement.Date, "dd.MM.yyyy", null):dddd.dd.MM.yyyy}.xlsx";

            // Сохраняем Excel файл на диск временно
            var tempExcelPath = Path.GetTempFileName() + ".xlsx";
            using (var fileStream = new FileStream(tempExcelPath, FileMode.Create, FileAccess.Write))
            {
                stream.Position = 0;
                stream.CopyTo(fileStream);
            }

            
            
           

            // Удаление временных файлов
            System.IO.File.Delete(tempExcelPath);
            
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        private void ConvertExcelToImage(string excelPath, string imagePath)
        {
            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Определяем размеры таблицы в ячейках
                int rowCount = worksheet.Dimension.End.Row;
                int columnCount = worksheet.Dimension.End.Column;

                // Определяем размеры каждой ячейки на изображении
                int cellWidth = 300; // Размер ячейки по ширине
                int cellHeight = 40; // Размер ячейки по высоте

                // Вычисляем общие размеры изображения
                int width = columnCount * cellWidth;
                int height = rowCount * cellHeight;

                // Создаем изображение
                using (var bitmap = new Bitmap((width/4)+100, height/4))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(Color.White);
                        var range = worksheet.Cells[worksheet.Dimension.Address];
                        foreach (var cell in range)
                        {
                            int row = cell.Start.Row - 1;
                            int col = cell.Start.Column - 1;
                            string text = cell.Text;

                            // Определяем координаты и размеры каждой ячейки на изображении
                            RectangleF cellRect = new RectangleF(col * cellWidth, row * cellHeight, cellWidth, cellHeight);

                            // Рисуем текст в ячейке
                            if (row == 0 && col == 0) // Если это первая ячейка
                            {
                                // Определяем размеры текста
                                SizeF textSize = graphics.MeasureString(text, new Font("Times New Roman", 20));

                                // Вычисляем координаты для центрирования текста в ячейке
                                float centerX = ((width / 4) + 100)/4;
                                float centerY = cellRect.Top + (cellRect.Height - textSize.Height) / 2;

                                // Рисуем текст по центру ячейки
                                graphics.DrawString(text, new Font("Times New Roman", 20,FontStyle.Bold), Brushes.Black, centerX, centerY);
                            }
                            else // Для остальных ячеек
                            {
                                graphics.DrawString(text, new Font("Times New Roman", 14), Brushes.Black, cellRect);
                            }


                            var borders = cell.Style.Border;
                            if (borders.Top.Style != ExcelBorderStyle.None)
                            {
                                graphics.DrawLine(new Pen(Color.Black), cellRect.Left, cellRect.Top, cellRect.Right, cellRect.Top);
                            }
                            if (borders.Bottom.Style != ExcelBorderStyle.None)
                            {
                                graphics.DrawLine(new Pen(Color.Black), cellRect.Left, cellRect.Bottom, cellRect.Right, cellRect.Bottom);
                            }
                            if (borders.Left.Style != ExcelBorderStyle.None)
                            {
                                graphics.DrawLine(new Pen(Color.Black), cellRect.Left, cellRect.Top, cellRect.Left, cellRect.Bottom);
                            }
                            if (borders.Right.Style != ExcelBorderStyle.None)
                            {
                                graphics.DrawLine(new Pen(Color.Black), cellRect.Right, cellRect.Top, cellRect.Right, cellRect.Bottom);
                            }
                        }
                    }

                    // Сохраняем изображение
                    bitmap.Save(imagePath, ImageFormat.Png);
                }
            }
        }

        public async Task SendImageToTelegramBot(string imagePath)
        {
            List<TGUser> tGUsers = _db.TGUser.ToList();
            List<Teacher> teachers = _db.Teacher.ToList();
            List<Raspisanie.Models.Group> groups = _db.Group.ToList();
            string botToken = "7118569820:AAGKrobfosdvVyx44fTS9SpSJkeKL6i8WfI";
            foreach (TGUser user in tGUsers)
            {
                if (user.ChatId != null)
                {
                    var botClient = new TelegramBotClient(botToken);

                    using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputOnlineFile(fileStream, "Расписание.png");
                        await botClient.SendPhotoAsync(user.ChatId, inputOnlineFile, "Новое расписание!");
                    }
                }
            }
            foreach (Teacher user in teachers)
            {
                if (user.ChatId != null)
                {
                    var botClient = new TelegramBotClient(botToken);

                    using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputOnlineFile(fileStream, "Расписание.png");
                        await botClient.SendPhotoAsync(user.ChatId, inputOnlineFile, "Новое расписание!");
                    }
                }
            }
            foreach (Raspisanie.Models.Group user in groups)
            {
                if (user.ChatId != null)
                {
                    var botClient = new TelegramBotClient(botToken);

                    using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputOnlineFile(fileStream, "Расписание.png");
                        await botClient.SendPhotoAsync(user.ChatId, inputOnlineFile, "Новое расписание!");
                    }
                }
            }


        }

        public ActionResult RequestsList()
        {
            var lastWeek = DateTime.Now.AddDays(-7);
            var requests = _db.Request.ToList(); // Получаем все запросы

            // Преобразуем Date из string в DateTime и фильтруем записи за последнюю неделю
            IEnumerable<Request> sortedRequests = requests
                .Select(r => new {
                    Request = r,
                    Date = DateTime.ParseExact(r.Date, "dd.MM.yyyy", CultureInfo.InvariantCulture) // Используйте правильный формат даты
                })
                .Where(r => r.Date >= lastWeek)
                .OrderByDescending(r => r.Request.Id)
                .Select(r => r.Request)
                .ToList();

            foreach (var request in sortedRequests)
            {
                request.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == request.TeacherId);
            }
            return PartialView("_RequestsList", sortedRequests);
        }

        public async Task<IActionResult> SendAll(List<PlacementVM> model)
        {
            List<Placement> placements = model.Select(p => p.Placement).ToList();

            IEnumerable<Placement> sortedPlacements = placements
                .OrderBy(p => p.GroupId)
                .ThenBy(p => p.index)
                .ToList();

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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Расписание");

                worksheet.Cells["A1:R200"].Style.Font.Name = "Times New Roman";

                worksheet.Cells["A1"].Value = $"РАСПИСАНИЕ ЗАНЯТИЙ НА {DateTime.ParseExact(model.FirstOrDefault().Placement.Date, "dd.MM.yyyy", null):dd.MM.yyyy} ({DateTime.ParseExact(model.FirstOrDefault().Placement.Date, "dd.MM.yyyy", null):dddd})";
                worksheet.Cells["A1:E1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:E1"].Style.Font.Bold = true;
                worksheet.Cells["A1:E1"].Style.Font.Size = 20;
                worksheet.Cells["A1:E1"].Merge = true;
                worksheet.Cells["A2"].Value = "Номер группы, ауд";
                worksheet.Cells["B2:E2"].Merge = true;
                worksheet.Cells["B2"].Value = "Расписание";
                worksheet.Cells["B2:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 40;
                worksheet.Column(3).Width = 30;
                worksheet.Column(4).Width = 30;
                worksheet.Column(5).Width = 30;

                worksheet.Cells[2, 1, 2, 5].Style.Font.Bold = true;
                worksheet.Cells[2, 1, 2, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[2, 1, 2, 5].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                worksheet.Cells[2, 1, model.Count + 2, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[2, 2, model.Count + 2, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[2, 2, model.Count + 2, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[2, 2, model.Count + 2, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                int row = 3;
                int old = -1;
                int startRow = 3;
                for (int i = 0; i < model.Count; i++)
                {
                    int temp = model[i].Placement.GroupId;
                    if (old != temp)
                    {
                        if (i != 0)
                        {
                            worksheet.Cells[startRow, 1, row - 1, 1].Merge = true;
                            worksheet.Row(startRow).Style.WrapText = true;
                            worksheet.Cells[startRow, 1, row - 1, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        }

                        worksheet.Cells[row, 1].Value = $"{model[i].Placement.Group.Name} ауд. {model[i].Placement.Group.Auditoria.AuditoryName}\nКуратор {model[i].Placement.Group.Teacher.TeacherName}";

                        startRow = row;
                        old = temp;
                    }
                    if (model[i].Placement.Predmet.NoAud)
                    {
                        worksheet.Cells[row, 2].Value = $"{model[i].Placement.index}.     {model[i].Placement.Predmet.PredmetName}   {model[i].Placement.Desc}";
                        worksheet.Cells[row, 4].Value = $"{model[i].Placement.Teacher.TeacherName}";
                    }
                    else
                    {
                        worksheet.Cells[row, 2].Value = $"{model[i].Placement.index}.     {model[i].Placement.Predmet.PredmetName}   {model[i].Placement.Desc}";
                        worksheet.Cells[row, 4].Value = $"{model[i].Placement.Teacher.TeacherName}, {model[i].Placement.Auditoria.AuditoryName}";
                    }


                    if (model[i].Placement.Predmet.Laboratory && (model[i].Placement.SecondPredmet != model[i].Placement.Predmet))
                    {
                        worksheet.Cells[row, 3].Value = $"{model[i].Placement.SecondPredmet.PredmetName}   {model[i].Placement.SDesc}";

                    }
                    if (model[i].Placement.Predmet.Laboratory)
                    {
                        if (model[i].Placement.Predmet.NoAud)
                        {
                            worksheet.Cells[row, 5].Value = $"{model[i].Placement.SecondTeacher.TeacherName}";
                        }
                        else
                        {
                            worksheet.Cells[row, 5].Value = $"{model[i].Placement.SecondTeacher.TeacherName}, {model[i].Placement.SecondAuditoria.AuditoryName}";
                        }

                    }

                    row++;
                }
                worksheet.Cells[startRow, 1, row - 1, 1].Merge = true;
                worksheet.Row(startRow).Style.WrapText = true;
                worksheet.Cells[startRow, 1, row - 1, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                package.Save();
            }

            stream.Position = 0;
            string excelName = $"Расписание-на{DateTime.ParseExact(model.FirstOrDefault().Placement.Date, "dd.MM.yyyy", null):dddd.dd.MM.yyyy}.xlsx";

            // Сохраняем Excel файл на диск временно
            var tempExcelPath = Path.GetTempFileName() + ".xlsx";
            using (var fileStream = new FileStream(tempExcelPath, FileMode.Create, FileAccess.Write))
            {
                stream.Position = 0;
                stream.CopyTo(fileStream);
            }

            // Конвертация Excel в изображение
            var tempImagePath = Path.GetTempFileName() + ".png";
            ConvertExcelToImage(tempExcelPath, tempImagePath);

            // Отправка изображения через телеграмм бот
            await SendImageToTelegramBot(tempImagePath);

            // Удаление временных файлов
            System.IO.File.Delete(tempExcelPath);
            System.IO.File.Delete(tempImagePath);

            stream.Position = 0;



            

            foreach (var PLVM in model)
            {
                var obj = _db.Placement.Find(PLVM.Placement.Id);
                if (obj == null)
                { PLVM.Placement.Id = 0; }

            }
            List<Placement> PlacementsToDelete = new List<Placement>();
            foreach (var placement in model)
            {
                PlacementsToDelete = _db.Placement.Where(p => p.Date == placement.Placement.Date).ToList();

            }
            foreach (var pl in PlacementsToDelete)
            {
                Predmet PredmetToPlus = _db.Predmet.FirstOrDefault(p => p.Id == pl.PredmetId);
                Predmet SPredmetToPlus = _db.Predmet.FirstOrDefault(p => p.Id == pl.SecondPredmetId);
                PredmetToPlus.Hours = PredmetToPlus.Hours + 2;
                if (PredmetToPlus != SPredmetToPlus)
                {
                    SPredmetToPlus.Hours = SPredmetToPlus.Hours + 2;
                }

                _db.Placement.RemoveRange(pl);
            }
            foreach (var placementVM in model)
            {
                Predmet PredmetToMinus = _db.Predmet.FirstOrDefault(p => p.Id == placementVM.Placement.PredmetId);
                Predmet SPredmetToMinus = _db.Predmet.FirstOrDefault(p => p.Id == placementVM.Placement.SecondPredmetId);
                PredmetToMinus.Hours = PredmetToMinus.Hours - 2;
                if (PredmetToMinus != SPredmetToMinus)
                {
                    SPredmetToMinus.Hours = SPredmetToMinus.Hours - 2;
                }

                _db.Placement.Add(placementVM.Placement);

            }
            _db.SaveChanges();
            
            
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
            TempData[WC.Success] = "Сохранено и отправленно!";
            ModelState.Clear();
            return View("ShowAll", placementList);
        }

    }
}