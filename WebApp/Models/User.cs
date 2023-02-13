using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<InstitutionAssignment> InstitutionAssignments { get; } = new List<InstitutionAssignment>();

    public virtual Role Role { get; set; } = null!;
}
