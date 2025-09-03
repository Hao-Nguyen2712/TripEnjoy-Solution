using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TripEnjoy.Infrastructure.Persistence
{
    public class TripEnjoyDbContext : IdentityDbContext<ApplicationUser>
    {
        public TripEnjoyDbContext(DbContextOptions<TripEnjoyDbContext> options) : base(options)
        {
        }
    }
}
