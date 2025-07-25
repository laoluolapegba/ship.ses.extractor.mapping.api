using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Core.Configuration;
using MySql.Data.MySqlClient;
using Npgsql;
using Ship.Ses.Extractor.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    public class EmrPersistenceFactory
    {

        public DbConnection MakeConn()
        {
            return new MySqlConnection();
        }
        public DbContext CreatePersistenceContext()
        {


            var conn = MakeConn();
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

            

            return new DbContext(optionsBuilder.Options);
        }
       


    }

}
