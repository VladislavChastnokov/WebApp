using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Drawing;
using WebApp.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApp.Models;
using Microsoft.Extensions.Hosting;
using System.Data;
using FastReport.Export.PdfSimple;
using System;

namespace WebApp.Controllers
{
    public class ExaminationController : Controller
    {
        private readonly string pageName = "Examination";

        private readonly DContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ExaminationController(DContext context, IWebHostEnvironment environment, IConfiguration config)
        {
            _context = context;
            _environment = environment;
            _configuration = config;
        }

        public async Task<IActionResult> Index(int? inst, int? spec, int? sem)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return RedirectToAction("", redirect);
            ViewBag.selectFormData = await _context.Institutions.Include(i => i.InstitutionAssignments).ThenInclude(ia => ia.User).Where(i => i.InstitutionAssignments.FirstOrDefault(x => x.InstitutionId == i.Id && x.UserId == HttpContext.Session.GetInt32("userRole")) != null || HttpContext.Session.GetInt32("userRole")==1).ToListAsync();
            ViewBag.ExportError = TempData["ExportError"]?.ToString();
            if (inst.HasValue && spec.HasValue && sem.HasValue)
            {
                var intstitution = await _context.Institutions.FindAsync(inst);
                ViewBag.Institution = intstitution?.InstitutionName;
                var speciality = await _context.Specialities.FindAsync(spec);
                ViewBag.Speciality = string.Format("{0} {1}", speciality?.SpecialityCode, speciality?.SpecialityName);
                ViewBag.SpecialityId = speciality?.Id;
                var semester = await _context.Semesters.FindAsync(sem);
                ViewBag.SemesterId = semester?.Id;
                ViewBag.Semester = semester?.Id == 7 ? "8" : semester?.Caption;
                var kurs = semester?.Kurs;
                ViewBag.Kurs = kurs;
                var marks = await _context.Examinations.Include(e => e.MarkNavigation).Include(e => e.Student).Include(e => e.Discipline).ThenInclude(m => m.Module).Where(m => m.Discipline.Module.SpecialityId == spec && m.Discipline.Semester == sem && m.Student.Kurs == kurs).ToListAsync();
                //Если студенты не были заполнены заранее
                if(marks.Count == 0)
                {
                    marks.AddRange(_context.Disciplines.Include(d => d.Module).Where(d => d.Module.SpecialityId == spec && d.Semester == sem).Select(x => new Models.Examination() { Discipline = x }));
                }
                ViewBag.Header = string.Format("Отчет по {0} практике по профессиональным модулям:", marks.All(x => x.Discipline.PracticeTypeId.HasValue) ? string.Format("{0}{1}{2}", marks.Any(x => x.Discipline.PracticeTypeId == 1) ? "учебной" : "", marks.Any(x => x.Discipline.PracticeTypeId == 1) && marks.Any(x => x.Discipline.PracticeTypeId == 2) ? " и " : "", marks.Any(x => x.Discipline.PracticeTypeId == 2) ? "производственной " : "") : "преддипломной");
                return View(marks);
            }
            return View();
        }
        /// <summary>
        /// Спсок специальностей для ajax функции
        /// </summary>
        /// <param name="Id">Идентификатор учебного заведения, выбранный на форме</param>
        /// <returns>список специальностей</returns>
        public async Task<object?> Specialities(int Id)
        {
            var access = Helper.ShowPage(HttpContext.Session, pageName, out string redirect);
            if (!access)
                return null;
            var list = await _context.Specialities.Where(s => s.InstitutionId == Id).Select(s => new { s.Id, Name = string.Format("{0} {1}", s.SpecialityCode, s.SpecialityName) }).ToListAsync();
            return list;
        }
        /// <summary>
        /// Спсок семестров для ajax функции
        /// </summary>
        /// <param name="Id">Идентификатор специальности, выбранный на форме</param>
        /// <returns>список семестров</returns>
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

        public async Task<IActionResult> Mark(int id)
        {
            var mark = await _context.Examinations.FindAsync(id);
            var v = PartialView(mark);
            
            return PartialView(mark);
        }
        [HttpPost]
        public async Task<IActionResult> Export(int spec, int sem, string header)
        {
            try
            {
                FastReport.Report rep = new FastReport.Report();
                rep.Load(Path.Combine(_environment.ContentRootPath, "App_Data", "report.frx"));
                rep.Dictionary.Connections[0].ConnectionString = _configuration.GetConnectionString("sql");
                rep.SetParameterValue("sem", sem);
                rep.SetParameterValue("spec", spec);
                rep.SetParameterValue("PracticeTypeName", header);
                rep.Prepare();
                PDFSimpleExport pdf = new PDFSimpleExport();
                var ms = new MemoryStream();
                pdf.Export(rep, ms);

                return File(ms.ToArray(), "application/pdf", "отчет.pdf");
            }
            catch(Exception ex)
            {
                TempData["ExportError"] = ex.Message;
                return Redirect(Request.Headers["Referer"].ToString());
            }
            
        }
    }
}
