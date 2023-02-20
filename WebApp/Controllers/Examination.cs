using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Drawing;
using WebApp.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class Examination : Controller
    {
        private readonly string pageName = "Examination";

        private readonly SqliteContext _context;

        public Examination(SqliteContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? inst, int? spec, int? sem)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            ViewBag.selectFormData = await _context.InstitutionAssignments.Include(i => i.User).Include(i => i.Institution).Where(i => i.UserId == HttpContext.Session.GetInt32("userId") || i.User.RoleId == 1).Select(x => x.Institution).ToListAsync();
            ViewBag.Controller = "Examination";
            ViewBag.ButtonName = "Показать";
            if (inst.HasValue && spec.HasValue && sem.HasValue)
            {
                var intstitution = await _context.Institutions.FindAsync(inst);
                ViewBag.Institution = intstitution?.InstitutionName;
                var speciality = await _context.Specialities.FindAsync(spec);
                ViewBag.Speciality = string.Format("{0} {1}", speciality?.SpecialityCode, speciality?.SpecialityName);
                ViewBag.SpecialityId = speciality?.Id;
                var semester = await _context.Semesters.FindAsync(sem);
                ViewBag.Semester = semester?.Id == 7 ? "8" : semester?.Caption;
                ViewBag.Kurs = semester?.Kurs;
                ViewBag.Marks = new SelectList(_context.Marks.ToList(), "Mark1", "Mark1");
                var disciplines = await _context.Disciplines.Include(d => d.Module).Where(d => d.Module.SpecialityId == spec && d.Semester == sem).ToListAsync();
                var modules = disciplines.Select(x => x.Module).Distinct().ToList();
                var marks = await _context.Examinations.Include(e => e.MarkNavigation).Include(e => e.Student).Include(e => e.Discipline).ThenInclude(m => m.Module).Where(m => m.Discipline.Module.SpecialityId == spec && m.Discipline.Semester == sem && m.Student.Kurs == semester.Kurs).ToListAsync();
                var students = marks.Select(e => e.Student).Distinct().ToList();
                //var model = new { LDisc = disciplines, LMd = modules, LMarks = marks, LSt = students, Editable = false };
                return View(marks);
            }
            return View();
        }

        public async Task<object?> Specialities(int Id)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return null;
            var list = await _context.Specialities.Where(s => s.InstitutionId == Id).Select(s => new { s.Id, Name = string.Format("{0} {1}", s.SpecialityCode, s.SpecialityName) }).ToListAsync();
            return list;
        }

        public async Task<object?> Semesters(int Id)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return null;
            var list = await _context.Disciplines.Include(d => d.SemesterNavigation).Include(d => d.Module).Where(d => d.Module.SpecialityId == Id).Select(d => new { d.SemesterNavigation.Id, Name = d.SemesterNavigation.Caption }).Distinct().ToListAsync();
            return list;
        }

        [HttpPost]
        public async Task<object?> SaveMark(int id, int? Mark)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return null;
            var mark = await _context.Examinations.Include(m => m.Discipline).Where(e => e.Id == id).FirstOrDefaultAsync();
            if (mark != null)
            {
                mark.Mark = Mark;
                await _context.SaveChangesAsync();
                var response = new { id = mark.StudentId, disc = mark.DisciplineId, dm = mark.Discipline.ModuleId, pr = mark.Discipline.PracticeTypeId ?? 0, avgD = await _context.Examinations.Where(e => e.DisciplineId == mark.DisciplineId).AverageAsync(m => m.Mark), avgS = await _context.Examinations.Include(e => e.Discipline).Where(e => e.StudentId == mark.StudentId && e.Discipline.ModuleId == mark.Discipline.ModuleId && e.Discipline.PracticeTypeId.Equals(mark.Discipline.PracticeTypeId)).AverageAsync(m => m.Mark) };
                return response;
            }
            else
                throw new ArgumentNullException();
        }

        [HttpPost]
        public async Task<bool> CheckMarkInput(int? value)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return false;
            try
            {
                return await _context.Marks.FindAsync(value) != null;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public async Task<object?> AddStudent(string lastName, string firstName, string? middleName, int spec, int kurs)
        {
            try
            {
                var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
                if (!access)
                    return null;
                if (await _context.Students.FirstOrDefaultAsync(s => s.SpecialityId.Equals(spec) && s.LastName.Equals(lastName) && s.FirstName.Equals(firstName) && ((string.IsNullOrEmpty(s.MiddleName) && string.IsNullOrEmpty(middleName)) || s.MiddleName.Equals(middleName)) && (s.Kurs.Equals(kurs))) == null)
                {
                    var newStudent = new Student() { SpecialityId = spec, LastName = lastName, FirstName = firstName, MiddleName = middleName, Kurs = kurs };
                    var disc = await _context.Disciplines.Include(d => d.Module).Where(d => d.Module.SpecialityId == spec).ToListAsync();
                    foreach (var d in disc)
                    {
                        newStudent.Examinations.Add(new Models.Examination() { Discipline = d });
                    }
                    await _context.Students.AddAsync(newStudent);
                    await _context.SaveChangesAsync();
                    return new { id = newStudent.Id, fio = string.Format("{0} {1} {2}", newStudent.LastName, newStudent.FirstName, newStudent.MiddleName), exams = newStudent.Examinations.OrderBy(x => x.DisciplineId).Select(x => new { x.Id, DisciplineId = x.Discipline.Id, x.Discipline.ModuleId, PracticeTypeId = x.Discipline.PracticeTypeId ?? 0 }).ToList() };
                }
                else
                    return null;
            }
            catch(Exception ex)
            {
                return null;
            }
            
        }
    }
}
