using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    public class PersistenceSettings
    {
        public required string Type { get; set; }
        public required string Var1s { get; set; }
        public required string Var2P { get; set; }
        public required string Var3name { get; set; }
        public required string Var4U { get; set; }
        public required string Var5P { get; set; }
        public required string IntegrityHash { get; set; }
    }
}
