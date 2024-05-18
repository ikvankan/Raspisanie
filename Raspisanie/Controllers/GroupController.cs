using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Raspisanie.Data;
using Raspisanie.Models.ViewModels;
using Raspisanie.Models;
using Microsoft.EntityFrameworkCore;

namespace Raspisanie.Controllers
{
    public class GroupController : Controller
    {
        private readonly ApplicationDbContext _db;
        public GroupController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Group> objList = _db.Group;
            foreach (var obj in objList)
            {
                obj.Auditoria = _db.Auditoria.FirstOrDefault(u => u.Id == obj.AuditoriaId);

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

            GroupVM groupVM = new GroupVM()
            {
                Group = new Group(),
                AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                {
                    Text = i.AuditoryName,
                    Value = i.Id.ToString()
                }),
            };
            if (Id == null)
            {
                //Создаем новый
                return View(groupVM);
            }
            else
            {
                groupVM.Group = _db.Group.Find(Id);
                if (groupVM.Group == null)
                {
                    return NotFound();
                }
                return View(groupVM);
            }

        }
        //POST-UPSERT
        [HttpPost]//Action метод типа пост дададада
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult Upsert(GroupVM groupVM)
        {
            if (groupVM.Group.Id == 0)
            {
                _db.Group.Add(groupVM.Group);
                TempData[WC.Success] = "Предмет создан успешно!";
            }
            else
            {
                //обновляем
                _db.Group.Update(groupVM.Group);
                TempData[WC.Success] = "Предмет изменён успешно!";
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET-DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null) { return NotFound(); }
            Group group = _db.Group.Include(u => u.Auditoria).FirstOrDefault(u => u.Id == Id);
            //var obj = _db.Teacher.Find(Id);
            if (group == null) { return NotFound(); }
            return View(group);
        }
        //POST-DELETE
        [HttpPost, ActionName("Delete")]//Action метод типа пост дададада,ну и имя для метода чтобы асп понимал как к нему лучше обращатся
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult DeletePost(int? Id)
        {
            var obj = _db.Group.Find(Id);
            if (obj == null) { return NotFound(); }

            var relatedPlacements = _db.Placement.Where(p=>p.GroupId==obj.Id).ToList();
            var relatedPredmets = _db.Predmet.Where(p => p.GroupId == obj.Id).ToList();
            _db.Placement.RemoveRange(relatedPlacements);


            foreach(var predmet in relatedPredmets)
            {
                var relatedPlacementsInPredmets = _db.Placement.Where(pl => pl.PredmetId == predmet.Id).ToList();
                _db.Placement.RemoveRange(relatedPlacementsInPredmets);
            }


            _db.Predmet.RemoveRange(relatedPredmets);



            _db.Group.Remove(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Аудитория удалена успешно!";
            return RedirectToAction("Index");
        }
    }
}
