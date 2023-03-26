using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Discipline
{
    public int Id { get; set; }

    public string DisciplineName { get; set; } = null!;

    public int ModuleId { get; set; }

    public int Semester { get; set; }

    public int? PracticeTypeId { get; set; }

    public virtual ICollection<Examination> Examinations { get; } = new List<Examination>();

    public virtual DisciplineModule Module { get; set; } = null!;

    public virtual PracticeType? PracticeType { get; set; }

    public virtual Semester SemesterNavigation { get; set; } = null!;
}
