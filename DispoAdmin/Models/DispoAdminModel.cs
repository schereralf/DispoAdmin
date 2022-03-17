using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model3DFarm;
using System.Configuration;

namespace DispoAdmin.Models
{
    class DispoAdminModel
    {
        private string _dbConnection;
        static DispoAdminModel _instance;

        static DispoAdminModel()
        {
            _instance = new DispoAdminModel();
        }

        public static DispoAdminModel Default { get { return _instance; } }

        private DispoAdminModel()            // get the DB connection string
        {
            _dbConnection = ConfigurationManager.ConnectionStrings["PrinterFarm"].ConnectionString;
            //_dbConnection = @"Data Source=DESKTOP-23QVBVH\SQLEXPRESS;Initial Catalog=PrinterFarm;Integrated Security=True";
        }
        
        public PrinterfarmContext GetDBContext()            // deliver a new DB context with connection
        {
            return new PrinterfarmContext(_dbConnection);
        }
    }
}
