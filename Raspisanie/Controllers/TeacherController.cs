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

            TeacherVM teacherVM = new TeacherVM()
            {
                Teacher = new Teacher(),
                AuditoriaSelectList = _db.Auditoria.Select(i => new SelectListItem
                {
                    Text = i.AuditoryName,
                    Value = i.Id.ToString()
                }),
            };
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
                TempData[WC.Success] = "Предмет создан успешно!";
            }
            else
            {
                //обновляем
                _db.Teacher.Update(teacherVM.Teacher);
                TempData[WC.Success] = "Предмет изменён успешно!";
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

            var relatedPredmets = _db.Predmet.Where(p=>p.TeacherId==obj.Id).ToList();
            _db.Predmet.RemoveRange(relatedPredmets);


            _db.Teacher.Remove(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Аудитория удалена успешно!";
            return RedirectToAction("Index");
        }
    }
}
