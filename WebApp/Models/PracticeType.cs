using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class PracticeType
{
    public int Id { get; set; }

    public string PracticeTypeName { get; set; } = null!;

    public virtual ICollection<Discipline> Disciplines { get; } = new List<Discipline>();
}
