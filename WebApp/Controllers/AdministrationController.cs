using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly SqliteContext _context;

        private static readonly string pageName = "Administration";

        public AdministrationController(SqliteContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            var users = await _context.Users.Include(u => u.InstitutionAssignments).ThenInclude(u => u.Institution).Include(u => u.Role).ToListAsync();
            var institutions = await _context.Institutions.Include(i => i.Specialities).ToListAsync();
            var model = new { Users = users, Institutions = institutions };
            ViewBag.NewUserErrorMessage = TempData["NewUserErrorMessage"]?.ToString();
            ViewBag.InstitutionErrorMessage = TempData["InstitutionErrorMessage"]?.ToString();
            ViewBag.SpecialityErrorMessage = TempData["SpecialityErrorMessage"]?.ToString();
            ViewBag.InstitutionId = TempData["InstitutionId"]?.ToString();
            ViewBag.KeepSectionOpen = TempData["KeepSectionOpen"]?.ToString();
            ViewBag.Institutions = new SelectList(_context.Institutions.ToList(), "Id", "InstitutionName");
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "RoleName");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddInstitutionAssignment(int id, int institutionId)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            if (_context.InstitutionAssignments.Where(ia => ia.UserId == id && ia.InstitutionId == institutionId).Count() == 0)
            {
                _context.InstitutionAssignments.Add(new InstitutionAssignment() { UserId = id, InstitutionId = institutionId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string type, int id)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            var t = Type.GetType(string.Format("WebApp.Models.{0}", type));
            if (t != null)
            {
                var x = await _context.FindAsync(t, id);
                if(x != null)
                    _context.Remove(x);
                await _context.SaveChangesAsync();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string password)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Password = Helper.CreateMD5(password);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("");
        }

        [HttpPost]
        public async Task<IActionResult> EditInstitution(int id, string name)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            if (id == -1)
            {
                if (await _context.Institutions.FirstOrDefaultAsync(x => x.InstitutionName.Equals(name)) != null)
                    TempData["InstitutionErrorMessage"] = "Учебное заведение с таким названием уже существует";
                else
                    await _context.Institutions.AddAsync(new Institution() { InstitutionName = name });
            }
            else
            {
                var inst = await _context.Institutions.FindAsync(id);
                if (inst != null)
                {
                    inst.InstitutionName = name;
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("");
        }

        [HttpPost]
        public async Task<IActionResult> EditSpeciality(int id, int instId, string code, string name)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            if (id == -1)
            {
                if (await _context.Specialities.FirstOrDefaultAsync(x => (x.SpecialityCode.Equals(code) || x.SpecialityName.Equals(name)) && x.InstitutionId.Equals(instId)) != null)
                {
                    TempData["InstitutionId"] = instId.ToString();
                    TempData["SpecialityErrorMessage"] = "Направление/специальность с таким кодом и названием уже существует";
                }
                else
                    await _context.Specialities.AddAsync(new Speciality() { InstitutionId = instId, SpecialityCode = code, SpecialityName = name });
            }
            else
            {
                var spec = await _context.Specialities.FindAsync(id);
                if (spec != null)
                {
                    spec.SpecialityName = name;
                    spec.SpecialityCode = code;
                }
            }
            TempData["KeepSectionOpen"] = instId.ToString();
            await _context.SaveChangesAsync();
            return RedirectToAction("");
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(string login, string password, string lastName, string firstName, string middleName, int role)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            if (_context.Users.FirstOrDefault(u => u.Login.Equals(login) || (u.LastName.Equals(lastName) && u.FirstName.Equals(firstName) && ((string.IsNullOrEmpty(u.MiddleName) && string.IsNullOrEmpty(middleName)) || u.MiddleName.Equals(middleName)))) != null)
            {
                TempData["NewUserErrorMessage"] = "Пользователь с таким логином или ФИО уже существует!";
            }
            else
            {
                var newUser = new User() { Login = login, LastName = lastName, FirstName = firstName, MiddleName = middleName, RoleId = role, Password = Helper.CreateMD5(password) };
                _context.Add(newUser);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("");
        }

        public async Task<IActionResult> Plan(int id)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            var sp = await _context.Specialities.FirstOrDefaultAsync(x => x.Id == id);
            ViewBag.Speciality = string.Format("{0} {1}", sp?.SpecialityCode, sp?.SpecialityName);
            ViewBag.SpecialityId = sp?.Id;
            ViewBag.Semesters = await _context.Semesters.OrderBy(s => s.Id).ToListAsync();
            ViewBag.ModuleError = TempData["ModuleError"]?.ToString();
            ViewBag.DiscError = TempData["DiscError"]?.ToString();
            ViewBag.SemestersSelect = new SelectList(await _context.Semesters.ToListAsync(), "Id", "Caption");
            ViewBag.ModulesSelect = new SelectList(await _context.DisciplineModules.ToListAsync(), "Id", "ModuleName");
            var model = new { Disciplines = await _context.Disciplines.Where(d => d.Module.SpecialityId == id).Include(d => d.PracticeType).ToListAsync(), Modules = await _context.DisciplineModules.ToListAsync() };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditModule(int id, int spec, string code, string name)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            if(id == -1)
            {
                if (await _context.DisciplineModules.FirstOrDefaultAsync(x => x.SpecialityId == spec && (x.ModuleCode.Equals(code) || x.ModuleName.Equals(name))) != null)
                    TempData["ModuleError"] = "Модуль с таким кодом или названием существует";
                else
                    await _context.DisciplineModules.AddAsync(new DisciplineModule() { SpecialityId = spec, ModuleCode = code, ModuleName = name });

            }
            else
            {
                var dm = await _context.DisciplineModules.FindAsync(id);
                if (dm != null)
                {
                    dm.ModuleCode = code;
                    dm.ModuleName = name;
                }
            }
            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> EditDiscipline(int id, int module, string name, int semester, bool studyPractice, bool workPractice)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            if (id == -1)
            {
                if (await _context.Disciplines.FirstOrDefaultAsync(x => x.ModuleId == module && x.DisciplineName.Equals(name) && x.Semester.Equals(semester) && ((x.PracticeTypeId == 1 && studyPractice) || (x.PracticeTypeId == 2 && workPractice))) != null)
                    TempData["DiscError"] = "Дисциплина с таким названием в данном семестре существует";
                else
                {
                    if(studyPractice)
                        await _context.Disciplines.AddAsync(new Discipline() { ModuleId = module, DisciplineName = name, Semester = semester, PracticeTypeId = 1 });
                    if (workPractice)
                        await _context.Disciplines.AddAsync(new Discipline() { ModuleId = module, DisciplineName = name, Semester = semester, PracticeTypeId = 2 });
                }
            }
            else
            {
                var disc = await _context.Disciplines.FindAsync(id);
                if (disc != null)
                {
                    disc.ModuleId = module;
                    disc.DisciplineName = name;
                    disc.Semester = semester;
                }
            }
            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
