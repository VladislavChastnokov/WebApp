using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Mark
{
    public int Mark1 { get; set; }

    public virtual ICollection<Examination> Examinations { get; } = new List<Examination>();
}
