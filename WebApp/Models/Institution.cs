using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Institution
{
    public int Id { get; set; }

    public string InstitutionName { get; set; } = null!;

    public virtual ICollection<InstitutionAssignment> InstitutionAssignments { get; } = new List<InstitutionAssignment>();

    public virtual ICollection<Speciality> Specialities { get; } = new List<Speciality>();
}
