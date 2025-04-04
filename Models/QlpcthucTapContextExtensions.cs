using Microsoft.EntityFrameworkCore;
using ABC.Models;

namespace ABC.Models
{
    public static class QlpcthucTapContextExtensions
    {
        public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString, bool isPostgres)
        {
            if (isPostgres)
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
