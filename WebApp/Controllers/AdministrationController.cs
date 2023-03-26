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
        private readonly DContext _context;

        private readonly string pageName = "Administration";

        public AdministrationController(DContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            //список пользователей
            var users = await _context.Users.Include(u => u.InstitutionAssignments).ThenInclude(u => u.Institution).Include(u => u.Role).ToListAsync();
            //список учебных заведений
            var institutions = await _context.Institutions.Include(i => i.Specialities).ToListAsync();
            //анонимный класс для передачи в представление без строго типизированной модели
            var model = new { Users = users, Institutions = institutions };
            //сообщение об ошибке при добавлении пользователя
            ViewBag.NewUserErrorMessage = TempData["NewUserErrorMessage"]?.ToString();
            //сообщение об ошибке при добавлении учебного заведения
            ViewBag.InstitutionErrorMessage = TempData["InstitutionErrorMessage"]?.ToString();
            //сообщение об ошибке при добавлении направления/специальности учебному заведения
            ViewBag.SpecialityErrorMessage = TempData["SpecialityErrorMessage"]?.ToString();
            ViewBag.InstitutionId = TempData["InstitutionId"]?.ToString();
            ViewBag.KeepSectionOpen = TempData["KeepSectionOpen"]?.ToString();
            //список учебных заведений для формы завкрепления уч.з. за пользователем
            ViewBag.Institutions = new SelectList(_context.Institutions.ToList(), "Id", "InstitutionName");
            //список ролей для формы создания пользователя
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "RoleName");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddInstitutionAssignment(int id, int institutionId)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            //проверка наличия уч.з. в закрепленных за пользователем перед добавлением
            if (_context.InstitutionAssignments.FirstOrDefault(ia => ia.UserId == id && ia.InstitutionId == institutionId) == null)
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
            //поиск типа данных
            var t = Type.GetType(string.Format("WebApp.Models.{0}", type));
            if (t != null)
            {
                //поиск записи с указанным типом и идентификатором
                var x = await _context.FindAsync(t, id);
                if (x != null)
                    _context.Remove(x);
                await _context.SaveChangesAsync();
            }
            //возврат к предыдущей странице
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
            //проверка наличия учебного заведения с указанным названием
            if (await _context.Institutions.FirstOrDefaultAsync(x => x.InstitutionName.Equals(name)) != null)
                TempData["InstitutionErrorMessage"] = "Учебное заведение с таким названием уже существует";
            else
            {
                if (id == -1)//новое учебное заведение
                    await _context.Institutions.AddAsync(new Institution() { InstitutionName = name });
                else
                {
                    var inst = await _context.Institutions.FindAsync(id);
                    if (inst != null)
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
            TempData["KeepSectionOpen"] = instId.ToString();
            //проверка ниличия направления/специальности перед добавлением
            if (await _context.Specialities.FirstOrDefaultAsync(x => (x.SpecialityCode.Equals(code) || x.SpecialityName.Equals(name)) && x.InstitutionId.Equals(instId)) != null)
            {
                TempData["InstitutionId"] = instId.ToString();
                TempData["SpecialityErrorMessage"] = "Направление/специальность с таким кодом и названием уже существует";
            }
            else
            {
                if (id == -1)//новое направление/специальность
                    await _context.Specialities.AddAsync(new Speciality() { InstitutionId = instId, SpecialityCode = code, SpecialityName = name });
                else
                {
                    var spec = await _context.Specialities.FindAsync(id);
                    if (spec != null)
                    {
                        spec.SpecialityName = name;
                        spec.SpecialityCode = code;
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("");
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(string login, string password, string lastName, string firstName, string middleName, int role)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            //проверка занятости логина или начилия пользователя с указнными ФИО
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

        public async Task<IActionResult> Speciality(int id)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            //список студентов
            var students = await _context.Students.Where(s => s.SpecialityId == id).ToListAsync();
            //список модулей дисциплин
            var modules = await _context.DisciplineModules.ToListAsync();
            //список дисциплин
            var disciplines = await _context.Disciplines.Where(d => d.Module.SpecialityId == id).Include(d => d.PracticeType).ToListAsync();
            //Направление/специальность для отображения
            var sp = await _context.Specialities.FirstOrDefaultAsync(x => x.Id == id);
            ViewBag.Speciality = string.Format("{0} {1}", sp?.SpecialityCode, sp?.SpecialityName);
            ViewBag.SpecialityId = sp?.Id;
            //Семестры для таблицы
            ViewBag.Semesters = await _context.Semesters.OrderBy(s => s.Id).ToListAsync();
            //Ошибка добавления модуля
            ViewBag.ModuleError = TempData["ModuleError"]?.ToString();
            //Ошибка добавления дисциплины
            ViewBag.DiscError = TempData["DiscError"]?.ToString();
            //Ошибка добавления студента
            ViewBag.StudentError = TempData["StudentError"]?.ToString();
            //Список семестров для формы добавления дисциплиеы
            ViewBag.SemestersSelect = new SelectList(await _context.Semesters.ToListAsync(), "Id", "Caption");
            //Список модулей для формы добавления дисциплиеы
            ViewBag.ModulesSelect = new SelectList(await _context.DisciplineModules.Where(dm => dm.SpecialityId == id).ToListAsync(), "Id", "ModuleName");
            //анонимный класс для передачи в представление без строго типизированной модели
            var model = new { Disciplines = disciplines, Modules = modules, Students = students };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditModule(int id, int spec, string code, string name)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            //проверка наличия модуля с кодом и названием
            if (await _context.DisciplineModules.FirstOrDefaultAsync(x => x.SpecialityId == spec && (x.ModuleCode.Equals(code) || x.ModuleName.Equals(name))) != null)
                TempData["ModuleError"] = "Модуль с таким кодом или названием существует";
            else
            {
                if (id == -1)//новый модуль
                    await _context.DisciplineModules.AddAsync(new DisciplineModule() { SpecialityId = spec, ModuleCode = code, ModuleName = name });
                else
                {
                    var dm = await _context.DisciplineModules.FindAsync(id);
                    if (dm != null)
                    {
                        dm.ModuleCode = code;
                        dm.ModuleName = name;
                    }
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
            string errorMessage = "";
            //проверка наличия дисциплины
            if (id == -1)//новая дисциплина
            {
                var students = await _context.Students.Where(s => s.SpecialityId == _context.DisciplineModules.Find(module).SpecialityId).ToListAsync();
                if (studyPractice)//учебная практика
                {
                    if (await _context.Disciplines.FirstOrDefaultAsync(x => x.ModuleId == module && x.DisciplineName.Equals(name) && x.Semester.Equals(semester) && x.PracticeTypeId == 1) != null)
                        errorMessage += "Дисциплина с таким названием и видом практики \"учебная\" в данном семестре существует\r\n";
                    else
                    {
                        var d = new Discipline() { ModuleId = module, DisciplineName = name, Semester = semester, PracticeTypeId = 1 };
                        foreach (var student in students)
                        {
                            d.Examinations.Add(new Models.Examination() { Student = student });
                        }
                        await _context.Disciplines.AddAsync(d);
                    }
                }
                if (workPractice)//производственная практика
                {
                    if (await _context.Disciplines.FirstOrDefaultAsync(x => x.ModuleId == module && x.DisciplineName.Equals(name) && x.Semester.Equals(semester) && x.PracticeTypeId == 2) != null)
                        errorMessage += "Дисциплина с таким названием и видом практики \"производственная\" в данном семестре существует\r\n";
                    else
                    {
                        var d = new Discipline() { ModuleId = module, DisciplineName = name, Semester = semester, PracticeTypeId = 2 };
                        foreach (var student in students)
                        {
                            d.Examinations.Add(new Models.Examination() { Student = student });
                        }
                        await _context.Disciplines.AddAsync(d);
                    }
                }
                if (!studyPractice && !workPractice)
                {
                    if (await _context.Disciplines.FirstOrDefaultAsync(x => x.ModuleId == module && x.DisciplineName.Equals(name) && x.Semester.Equals(semester)) != null)
                        errorMessage += "Дисциплина с таким названием в данном семестре существует\r\n";
                    else
                    {
                        var d = new Discipline() { ModuleId = module, DisciplineName = name, Semester = semester };
                        foreach (var student in students)
                        {
                            d.Examinations.Add(new Models.Examination() { Student = student });
                        }
                        await _context.Disciplines.AddAsync(d);
                    }
                }
            }
            else
            {

                var disc = await _context.Disciplines.FindAsync(id);
                if (disc != null)
                {
                    if (await _context.Disciplines.FirstOrDefaultAsync(x => x.ModuleId == module && x.DisciplineName.Equals(name) && x.Semester.Equals(semester) && x.PracticeTypeId == disc.PracticeTypeId) != null)
                        errorMessage += "Дисциплина с таким названием и видом практики в данном семестре существует";
                    else
                    {
                        disc.ModuleId = module;
                        disc.DisciplineName = name;
                    }
                }
            }
            TempData["DiscError"] = errorMessage;
            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(int id, int spec, string lastName, string firstName, string middleName, int kurs)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            //Проверка наличия студента
            if (await _context.Students.FirstOrDefaultAsync(s => s.SpecialityId.Equals(spec) && s.LastName.Equals(lastName) && s.FirstName.Equals(firstName) && ((string.IsNullOrEmpty(s.MiddleName) && string.IsNullOrEmpty(middleName)) || s.MiddleName.Equals(middleName)) && (s.Kurs.Equals(kurs) || id != -1)) != null)
                TempData["StudentError"] = "Студент с такими ФИО на указаном курсе уже существует";
            else
            {
                if (id == -1)//новый студент
                {
                    var newStudent = new Student() { SpecialityId = spec, LastName = lastName, FirstName = firstName, MiddleName = middleName, Kurs = kurs };
                    var disc = await _context.Disciplines.Include(d => d.Module).Where(d => d.Module.SpecialityId == spec).ToListAsync();
                    foreach (var d in disc)
                    {
                        newStudent.Examinations.Add(new Models.Examination() { Discipline = d });
                    }
                    await _context.Students.AddAsync(newStudent);
                }
                else
                {
                    var stud = await _context.Students.FindAsync(id);
                    if (stud != null)
                    {
                        stud.LastName = lastName;
                        stud.FirstName = firstName;
                        stud.MiddleName = middleName;
                        stud.Kurs = kurs;
                    }
                }
            }
            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
