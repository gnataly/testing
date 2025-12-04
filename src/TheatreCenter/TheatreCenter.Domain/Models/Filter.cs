using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Models;

public class BaseFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Search { get; set; }
}

public class ActorFilter : BaseFilter
{
    public VoiceType? VoiceType { get; set; }
    public Gender? Gender { get; set; }
    public bool OnlyFavorites { get; set; }
    public string Sort { get; set; } = "id_asc";
}

public class MusicalFilter : BaseFilter
{
    public AgeRestriction? AgeRestriction { get; set; }
    public int? TheatreId { get; set; }
    public int? ThemeId { get; set; }
    public bool OnlyFavorites { get; set; }
    public string Sort { get; set; } = "id_asc";
}

public class TheatreFilter : BaseFilter
{
    public bool OnlyFavorites { get; set; }
    public string Sort { get; set; } = "id_asc";
}

public class RoleFilter : BaseFilter
{
    public RoleType? RoleType { get; set; }
    public int? MusicalId { get; set; }
    public int? ActorId { get; set; }
    public string Sort { get; set; } = "id_asc";
}

public class ShowFilter : BaseFilter
{
    public int? MusicalId { get; set; }
    public int? ActorId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string Sort { get; set; } = "date_asc";
}

public class ThemeFilter : BaseFilter
{
    public string Sort { get; set; } = "id_asc";
}

public class AccountFilter : BaseFilter
{
    public bool? UpgradeRequest { get; set; }
}

public class CastMemberFilter : BaseFilter
{
    public int? ShowId { get; set; }
    public int? RoleId { get; set; }
    public int? ActorId { get; set; }
}
