using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class InstitutionAssignment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int InstitutionId { get; set; }

    public virtual Institution Institution { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
