using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Examination
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int DisciplineId { get; set; }

    public int? Mark { get; set; }

    public virtual Discipline Discipline { get; set; } = null!;

    public virtual Mark? MarkNavigation { get; set; }

    public virtual Student Student { get; set; } = null!;
}
