using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Semester
{
    public int Id { get; set; }

    public string? Caption { get; set; }

    public virtual ICollection<Discipline> Disciplines { get; } = new List<Discipline>();
}
