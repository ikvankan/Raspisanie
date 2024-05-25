using Microsoft.AspNetCore.Mvc;
using Raspisanie.Data;
using Raspisanie.Models;

namespace Raspisanie.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AuditoriaController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Auditoria> objList = _db.Auditoria;
            return View(objList);
        }

        public IActionResult Create()
        {

            return View();
        }
        //POST-CREATE
        [HttpPost]//Action метод типа пост дададада
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult Create(Auditoria obj)
        {
            _db.Auditoria.Add(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Аудитория создана успешно!";
            return RedirectToAction("Index");
        }

        //GET-EDIT
        public IActionResult Edit(int? Id)
        {
            if (Id == null) { return NotFound(); }
            var obj = _db.Auditoria.Find(Id);
            if (obj == null) { return NotFound(); }
            return View(obj);
        }

        //POST-EDIT
        [HttpPost]//Action метод типа пост дададада
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult Edit(Auditoria obj)
        {
            _db.Auditoria.Update(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Аудитория изменена успешно!";
            return RedirectToAction("Index");
        }

        //GET-DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null) { return NotFound(); }
            var obj = _db.Auditoria.Find(Id);
            if (obj == null) { return NotFound(); }
            return View(obj);
        }


        //POST-DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {
            var obj = _db.Auditoria.Find(Id);
            if (obj == null) { return NotFound(); }
            // Находим связанные записи в таблице Group и Teacher и Placement
            var relatedGroups = _db.Group.Where(p => p.AuditoriaId == obj.Id).ToList();
            var relatedTeachers = _db.Teacher.Where(p => p.AuditoryId == obj.Id).ToList();
            var relatedPlacements1 = _db.Placement.Where(p => p.AuditoriaId == obj.Id).ToList();
            var relatedPlacements2 = _db.Placement.Where(p => p.SecondAuditoriaId == obj.Id).ToList();
            foreach (var group in relatedGroups)
            {
                var relatedPlacementsInGroups = _db.Placement.Where(pl=>pl.GroupId == group.Id).ToList();
                var relatedPredmets = _db.Predmet.Where(pl => pl.GroupId == group.Id).ToList();
                // Удаляем связанные c таблицей Predmet записи из таблицы Placement
                foreach (var predmet in relatedPredmets)
                {
                    var relatedPlacementsInPredmet = _db.Placement.Where(pll=> pll.PredmetId == predmet.Id).ToList();
                    _db.Placement.RemoveRange(relatedPlacementsInPredmet);
                }
                _db.Predmet.RemoveRange(relatedPredmets);
                _db.Placement.RemoveRange(relatedPlacementsInGroups);
                // Удаляем связанные записи из таблицы Group
                _db.Group.Remove(group);
            }
            foreach (var teacher in relatedTeachers)
            {
                // Находим связанные записи в таблице Predmet
                var relatedPredmets = _db.Predmet.Where(pl => pl.TeacherId == teacher.Id).ToList();
                foreach(var predmet in relatedPredmets)
                {
                    var relatedPlacementsInPredmet = _db.Placement.Where(pll=> pll.PredmetId == predmet.Id).ToList();
                    _db.Placement.RemoveRange(relatedPlacementsInPredmet);
                }
                
                // Удаляем связанные записи из таблицы Predmet
                _db.Predmet.RemoveRange(relatedPredmets);
                
                // Удаляем связанные записи из таблицы Teacher
                _db.Teacher.Remove(teacher);
            }
            _db.Placement.RemoveRange(relatedPlacements1);_db.Placement.RemoveRange(relatedPlacements2);

            // Удаляем запись из таблицы Auditoria
            _db.Auditoria.Remove(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Аудитория и связанные с ней записи удалены успешно!";
            return RedirectToAction("Index");
        }

    }
}
