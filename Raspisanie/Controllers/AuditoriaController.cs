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
        [HttpPost]//Action метод типа пост дададада
        [ValidateAntiForgeryToken]//Токен от взлома
        public IActionResult DeletePost(int? Id)
        {
            var obj = _db.Auditoria.Find(Id);
            if (obj == null) { return NotFound(); }
            _db.Auditoria.Remove(obj);
            _db.SaveChanges();
            TempData[WC.Success] = "Аудитория удалена успешно!";
            return RedirectToAction("Index");
        }
    }
}
