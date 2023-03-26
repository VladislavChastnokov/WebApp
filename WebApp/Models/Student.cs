using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Student
{
    public int Id { get; set; }

    public int SpecialityId { get; set; }

    public int Kurs { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public virtual ICollection<Examination> Examinations { get; } = new List<Examination>();

    public virtual Speciality Speciality { get; set; } = null!;
}
