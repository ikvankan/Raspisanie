using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Raspisanie.Data;
using Raspisanie.Models;
using Raspisanie.Models.ViewModels;

namespace Raspisanie.Controllers
{
    public class PlacementController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PlacementController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Placement> objList = _db.Placement;
            foreach (var obj in objList)
            {
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);
                obj.Predmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.PredmetId);
                obj.SecondPredmet = _db.Predmet.FirstOrDefault(u => u.Id == obj.SecondPredmetId);
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);
                obj.SecondAuditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.SecondAuditoriaId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
            }
            return View(objList);
        }
        public IActionResult DeleteByDateRange(DateTime startDate, DateTime endDate)
        {
            // Преобразование всех записей в память и фильтрация на стороне клиента
            var recordsToDelete = _db.Placement
                                          .AsEnumerable()
                                          .Where(p => DateTime.ParseExact(p.Date, "dd.MM.yyyy", null) >= startDate &&
                                                      DateTime.ParseExact(p.Date, "dd.MM.yyyy", null) <= endDate)
                                          .ToList();
            _db.Placement.RemoveRange(recordsToDelete);
            _db.SaveChanges();
            TempData[WC.Success] = "Удалено успешно!";
            return RedirectToAction("Index");
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

            PlacementVM placementVM = new PlacementVM()
            {
                Placement = new Placement(),
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
                }),
                TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                {
                    Text = i.TeacherName,
                    Value = i.Id.ToString()
                }),
            };
            if (Id == null)
            {
                //Создаем новый
                return View(placementVM);
            }
            else
            {
                placementVM.Placement = _db.Placement.Find(Id);
                if (placementVM.Placement == null)
                {
                    return NotFound();
                }
                return View(placementVM);
            }

        }
        //POST-UPSERT
        [HttpPost]//Action метод типа пост дададада
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult Upsert(PlacementVM placementVM)
        {
            if (placementVM.Placement.Id == 0)
            {
                _db.Placement.Add(placementVM.Placement);
                TempData[WC.Success] = "Создан успешно!";
            }
            else
            {
                //обновляем
                _db.Placement.Update(placementVM.Placement);
                TempData[WC.Success] = "Изменён успешно!";
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET-DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null) { return NotFound(); }
            Placement placement = _db.Placement.Include(u => u.Group).Include(u => u.Predmet).Include(u => u.SecondPredmet).Include(u => u.Auditoria).Include(u=>u.SecondAuditoria).Include(u => u.Teacher).Include(u => u.SecondTeacher).FirstOrDefault(u => u.Id == Id);
            //var obj = _db.Teacher.Find(Id);
            if (placement == null) { return NotFound(); }
            return View(placement);
        }
        //POST-DELETE
        [HttpPost, ActionName("Delete")]//Action метод типа пост дададада,ну и имя для метода чтобы асп понимал как к нему лучше обращатся
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult DeletePost(int? Id)
        {
            var obj = _db.Placement.Find(Id);
            if (obj == null) { return NotFound(); }
            _db.Placement.Remove(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Удалено успешно!";
            return RedirectToAction("Index");
        }
    }
}
