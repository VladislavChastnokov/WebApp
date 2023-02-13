using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class DisciplineModule
{
    public int Id { get; set; }

    public int SpecialityId { get; set; }

    public string ModuleCode { get; set; } = null!;

    public string ModuleName { get; set; } = null!;

    public virtual ICollection<Discipline> Disciplines { get; } = new List<Discipline>();

    public virtual Speciality Speciality { get; set; } = null!;
}
