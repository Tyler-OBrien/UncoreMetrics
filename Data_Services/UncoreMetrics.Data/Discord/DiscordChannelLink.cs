using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UncoreMetrics.Data.Discord
{
    public class DiscordChannelLink
    {
        [Required] public Guid ID { get; set; }

        [Required]  public ulong UserID { get; set; }

        [Required]  public Guid GameServerID { get; set; }

        [Required]  public ulong ServerID { get; set; }

        [Required]  public ulong ChannelID { get; set; }

        [Required] public bool Enabled { get; set; }

        public DateTime LastChanged { get; set; }

        [Required] public string LastStatus { get; set; }

        public DateTime Created { get; set; }
    }
}
