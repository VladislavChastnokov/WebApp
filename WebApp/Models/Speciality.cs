using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Speciality
{
    public int Id { get; set; }

    public string SpecialityCode { get; set; } = null!;

    public string SpecialityName { get; set; } = null!;

    public int InstitutionId { get; set; }

    public virtual ICollection<DisciplineModule> DisciplineModules { get; } = new List<DisciplineModule>();

    public virtual Institution Institution { get; set; } = null!;

    public virtual ICollection<Student> Students { get; } = new List<Student>();
}
