using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scottxu.Blog.Models.Util
{
    public class AppSettingsUtil
    {
        public IConfiguration Configuration { get; }

        public AppSettingsUtil(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Action<DbContextOptionsBuilder> GetDbContextOptionsBuilder()
        {
            switch (Configuration["DataBaseType"])
            {
                case "SQLServer":
                    return options => options.UseSqlServer(Configuration.GetConnectionString("SQLServerConnection"));
                case "SQLite":
                    return options => options.UseSqlite(Configuration.GetConnectionString("SQLiteConnection"));
                case "MySQL":
                    return options => options.UseMySql(Configuration.GetConnectionString("MySQLConnection"));
                default:
                    return null;
            }
        }
    }
}
