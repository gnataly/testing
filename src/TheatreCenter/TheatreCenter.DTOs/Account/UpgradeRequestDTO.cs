using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TheatreCenter.DTOs.Account
{
    public class UpgradeRequestDTO
    {
        [JsonPropertyName("isApproved")]
        public bool IsApproved { get; set; }
    }
}
