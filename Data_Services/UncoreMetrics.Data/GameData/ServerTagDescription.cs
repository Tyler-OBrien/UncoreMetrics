using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UncoreMetrics.Data.GameData
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ServerTagDescription : Attribute
    {
        public ServerTagDescription(string name, string description, string toolTipDescription, string versionAdded)
        {
            Name = name;
            Description = description;
            ToolTipDescription = toolTipDescription;
            VersionAdded = versionAdded;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ToolTipDescription { get; set; }

        public string VersionAdded { get; set; }
    }
}
