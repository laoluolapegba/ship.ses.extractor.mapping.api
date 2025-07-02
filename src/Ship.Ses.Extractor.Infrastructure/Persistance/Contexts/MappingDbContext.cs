using Microsoft.EntityFrameworkCore;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Contexts
{
    public class MappingDbContext : DbContext
    {
        public MappingDbContext(DbContextOptions<MappingDbContext> options) : base(options) { }

        public DbSet<Mapping> Mappings { get; set; }
    }

}
