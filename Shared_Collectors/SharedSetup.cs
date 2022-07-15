using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared_Collectors.Databases.DataAccess;
using Shared_Collectors.Models;
using Shared_Collectors.Tools.Maxmind;

namespace Shared_Collectors
{
    public static class SharedSetup
    {
        public static void ConfigureSharedServices(this IServiceCollection services, BaseConfiguration configuration)
        {
            services.AddDbContext<GenericServersContext>(options =>
                options.UseNpgsql(configuration.PostgresConnectionString));
            services.AddSingleton<IGeoIPService, MaxMindService>();
        }
    }
}
