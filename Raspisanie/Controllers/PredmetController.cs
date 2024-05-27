using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Raspisanie.Data;
using Raspisanie.Models.ViewModels;
using Raspisanie.Models;
using Microsoft.EntityFrameworkCore;

namespace Raspisanie.Controllers
{
    public class PredmetController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PredmetController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            
            IEnumerable<Predmet> objList = _db.Predmet.OrderBy(p=>p.GroupId);
            foreach (var obj in objList)
            {
                obj.SecondTeacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.SecondTeacherId);
                obj.Teacher = _db.Teacher.FirstOrDefault(u => u.Id == obj.TeacherId);
                obj.Group = _db.Group.FirstOrDefault(u => u.Id == obj.GroupId);

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

            PredmetVM predmetVM = new PredmetVM()
            {
                Predmet = new Predmet(),
                TeacherSelectList = _db.Teacher.Select(i => new SelectListItem
                {
                    Text = i.TeacherName,
                    Value = i.Id.ToString()
                }),
                GroupSelectList = _db.Group.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };
            if (Id == null)
            {
                //Создаем новый
                return View(predmetVM);
            }
            else
            {
                predmetVM.Predmet = _db.Predmet.Find(Id);
                if (predmetVM.Predmet == null)
                {
                    return NotFound();
                }
                return View(predmetVM);
            }

        }
        //POST-UPSERT
        [HttpPost]//Action метод типа пост дададада
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult Upsert(PredmetVM predmetVM)
        {
            if (predmetVM.Predmet.Id == 0)
            {
                _db.Predmet.Add(predmetVM.Predmet);
                TempData[WC.Success] = "Предмет создан успешно!";
            }
            else
            {
                //обновляем
                _db.Predmet.Update(predmetVM.Predmet);
                TempData[WC.Success] = "Предмет изменён успешно!";
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET-DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null) { return NotFound(); }
            Predmet predmet = _db.Predmet.Include(u => u.Teacher).Include(u => u.Group).Include(u=>u.SecondTeacher).FirstOrDefault(u => u.Id == Id);
            //var obj = _db.Teacher.Find(Id);
            if (predmet == null) { return NotFound(); }
            return View(predmet);
        }
        //POST-DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {
            var obj = _db.Predmet.Find(Id);
            if (obj == null) { return NotFound(); }

            // Находим связанные записи в таблице Placement
            var relatedPlacements1 = _db.Placement.Where(p => p.PredmetId == obj.Id).ToList();var relatedPlacements2 = _db.Placement.Where(p => p.SecondPredmetId == obj.Id).ToList();

            // Удаляем связанные записи из таблицы Placement
            _db.Placement.RemoveRange(relatedPlacements1);_db.Placement.RemoveRange(relatedPlacements2);

            // Удаляем запись из таблицы Predmet
            _db.Predmet.Remove(obj);

            _db.SaveChanges();
            TempData[WC.Success] = "Предмет и связанные с ним записи удалены успешно!";
            return RedirectToAction("Index");
        }

    }
}
