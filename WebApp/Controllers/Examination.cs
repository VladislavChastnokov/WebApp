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
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Html;

namespace WebApp.Controllers
{
    public class Examination : Controller
    {
        private readonly string pageName = "Examination";

        private readonly SqliteContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public Examination(SqliteContext context, IWebHostEnvironment environment, IConfiguration config)
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
                var kurs = semester?.Kurs;
                ViewBag.Kurs = kurs;
                var disciplines = await _context.Disciplines.Include(d => d.Module).Where(d => d.Module.SpecialityId == spec && d.Semester == sem).ToListAsync();
                var marks = await _context.Examinations.Include(e => e.MarkNavigation).Include(e => e.Student).Include(e => e.Discipline).ThenInclude(m => m.Module).Where(m => m.Discipline.Module.SpecialityId == spec && m.Discipline.Semester == sem && m.Student.Kurs == kurs).ToListAsync();
                //Если студенты не были заполнены заранее
                if(marks.Count == 0)
                {
                    marks.AddRange(disciplines.Select(x => new Models.Examination() { Discipline = x }));
                }
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
            var access = Helper.ShowPage(HttpContext.Session, pageName, out _);
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

        public async Task<IActionResult> Mark(int id)
        {
            var mark = await _context.Examinations.FindAsync(id);
            var v = PartialView(mark);
            
            return PartialView(mark);
        }

        [HttpPost]
        public async Task<IActionResult> Export(string dt)
        {
            /*var doc = new GcPdfDocument();
            // Add a new page to the document
            var page = doc.Pages.Add();
            // Take the Graphics instance of the page
            var g = page.Graphics;

            //Define GcHtmlBrowser instance
            var path = new BrowserFetcher().GetDownloadedPath();
            using (var browser = new GcHtmlBrowser(path))
            {
                // Render the HTML string on the PDF, using the DrawHtml method
                var ok = g.DrawHtml(browser, string.Format("<html><head><style>\r\n{0}\r\n{1}\r\n</style></head><body>\r\n{2}\r\n</body></html>", System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, "css", "site.css")), System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, "lib/bootstrap/dist/css", "bootstrap.css")), dt), 72, 72, new HtmlToPdfFormat(false) { MaxPageWidth = 6.5f }, out SizeF size);

                doc.Save(Path.Combine(_environment.ContentRootPath,"temp","test.pdf"));
            }*/
            ExpertPdf.HtmlToPdf.PdfConverter pdf = new ExpertPdf.HtmlToPdf.PdfConverter();
            pdf.PdfDocumentOptions.PdfPageOrientation = ExpertPdf.HtmlToPdf.PDFPageOrientation.Landscape;
            var ms = pdf.GetPdfBytesFromHtmlString(string.Format("<html><head><style>\r\n{0}\r\n{1}\r\n</style></head><body>\r\n{2}\r\n</body></html>", System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, "css", "site.css")), System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, "lib/bootstrap/dist/css", "bootstrap.css")), dt));

            return File(ms, "application/pdf", "report.pdf");
        }
    }
}
