using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Raspisanie.Data;
using Raspisanie.Models;
using Raspisanie.Models.ViewModels;

namespace Raspisanie.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _db;
        public TeacherController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Teacher> objList = _db.Teacher;
            foreach (var obj in objList)
            {
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoryId);
                
            }
            return View(objList);
        }

        //GET-UPSERT
        public IActionResult Upsert(int? Id)
        {
            //IEnumerable<SelectListItem> ItemTypeDropDown = _db.ItemType.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});
            //ViewBag.ItemTypeDropDown = ItemTypeDropDown;
            //Item item = new Item();
            TeacherVM teacherVM;
            var teacherToEdut = _db.Teacher.Find(Id);
            if (Id!=null)
            {
                teacherVM = new TeacherVM()
                {
                    Teacher = new Teacher(),
                    AuditoriaSelectList = _db.Auditoria
                    .Where(a => a.AuditoryName.ToLower() == "Нету" ||a.Id ==teacherToEdut.AuditoryId || !_db.Teacher.Any(t => t.AuditoryId == a.Id)) // Добавляем условие для аудитории "Нету"
                    .Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                };
            }
            else
            {
                teacherVM = new TeacherVM()
                {
                    Teacher = new Teacher(),
                    AuditoriaSelectList = _db.Auditoria
                    .Where(a => a.AuditoryName.ToLower() == "Нету" || !_db.Teacher.Any(t => t.AuditoryId == a.Id)) // Добавляем условие для аудитории "Нету"
                    .Select(i => new SelectListItem
                    {
                        Text = i.AuditoryName,
                        Value = i.Id.ToString()
                    }),
                };
            }
            


            if (Id == null)
            {
                //Создаем новый
                return View(teacherVM);
                
            }
            else
            {
                teacherVM.Teacher = _db.Teacher.Find(Id);
                if (teacherVM.Teacher == null)
                {
                    return NotFound();
                }
                return View(teacherVM);
            }

        }
        //POST-UPSERT
        [HttpPost]//Action метод типа пост дададада
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult Upsert(TeacherVM teacherVM)
        {
            if (teacherVM.Teacher.Id == 0)
            {
                _db.Teacher.Add(teacherVM.Teacher);
                TempData[WC.Success] = "Создан успешно!";
            }
            else
            {
                //обновляем
                _db.Teacher.Update(teacherVM.Teacher);
                TempData[WC.Success] = "Изменён успешно!";
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET-DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null) { return NotFound(); }
            Teacher teacher = _db.Teacher.Include(u => u.Auditoria).FirstOrDefault(u => u.Id == Id);
            //var obj = _db.Teacher.Find(Id);
            if (teacher == null) { return NotFound(); }
            return View(teacher);
        }
        //POST-DELETE
        [HttpPost, ActionName("Delete")]//Action метод типа пост дададада,ну и имя для метода чтобы асп понимал как к нему лучше обращатся
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult DeletePost(int? Id)
        {
            var obj = _db.Teacher.Find(Id);
            if (obj == null) { return NotFound(); }

            var relatedPredmets1 = _db.Predmet.Where(p=>p.TeacherId==obj.Id).ToList();
            var relatedPredmets2 = _db.Predmet.Where(p => p.SecondTeacherId == obj.Id).ToList();
            foreach (var predmet in relatedPredmets1)
            {
                var relatedPlacement_Predmets = _db.Placement.Where(pl=>pl.PredmetId==predmet.Id).ToList();
                _db.Placement.RemoveRange(relatedPlacement_Predmets);
            }
            foreach (var predmet in relatedPredmets2)
            {
                var relatedPlacement_Predmets = _db.Placement.Where(pl => pl.PredmetId == predmet.Id).ToList();
                _db.Placement.RemoveRange(relatedPlacement_Predmets);
            }
            _db.Predmet.RemoveRange(relatedPredmets1);_db.Predmet.RemoveRange(relatedPredmets2);

            var relatedGroups = _db.Group.Where(p => p.TeacherId == obj.Id).ToList();









            foreach (var group in relatedGroups)
            {
                var relatedPlacements_Groups = _db.Placement.Where(pl => pl.GroupId == group.Id).ToList();
                var relatedPredmets_Groups = _db.Predmet.Where(pl => pl.GroupId == group.Id).ToList();
                // Удаляем связанные c таблицей Predmet записи из таблицы Placement
                foreach (var predmet in relatedPredmets_Groups)
                {
                    var relatedPlacementsInPredmet = _db.Placement.Where(pll => pll.PredmetId == predmet.Id).ToList();
                    _db.Placement.RemoveRange(relatedPlacementsInPredmet);
                }
                _db.Predmet.RemoveRange(relatedPredmets_Groups);
                _db.Placement.RemoveRange(relatedPlacements_Groups);
                // Удаляем связанные записи из таблицы Group
                _db.Group.Remove(group);
            }




            _db.Teacher.Remove(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Удалено успешно!";
            return RedirectToAction("Index");
        }
    }
}
