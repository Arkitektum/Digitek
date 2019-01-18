using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digitek.brannProsjektering.Models;
using Microsoft.EntityFrameworkCore;

namespace digitek.brannProsjektering.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UseRecord> UseRecords { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {

        }
    }
}
