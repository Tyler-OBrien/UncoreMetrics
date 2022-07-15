using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Collectors.Databases.Entities
{
    public class GenericServer
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ServerID { get; set; }

        public string Name { get; set; }

        public string Game { get; set; }

        public int AppID { get; set; }

        public string Address { get; set; }
        public string QueryAddress { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }

        public string ASN { get; set; }

        public string ISP { get; set; }

        public string Location { get; set; }

        public string Country { get; set; }

        public string Continent { get; set; }

        public bool IsOnline { get; set; }

        public bool LastCheckOnline { get; set; }

        public DateTime LastCheck { get; set; }

        public DateTime FoundAt { get; set; }
    }
}
