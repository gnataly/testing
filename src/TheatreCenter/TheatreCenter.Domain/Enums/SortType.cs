using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TheatreCenter.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortType
{
    name_asc,
    name_desc,
    birthDate_asc,
    birthDate_desc,
    id_asc,
    id_desc,
    title_asc,
    title_desc,
    duration_asc,
    duration_desc,
    date_asc,
    date_desc,
    roleType_asc,
    roleType_desc
}
