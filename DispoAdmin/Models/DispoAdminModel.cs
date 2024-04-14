
using ModelSQLLiteFarm;
using System.Configuration;

namespace DispoAdmin.Models
{
    // this class does admin stuff for connecting the SQLite database.
    class DispoAdminModel
    {
        private readonly string _dbConnection;
        static readonly DispoAdminModel _instance;

        static DispoAdminModel()
        {
            _instance = new DispoAdminModel();
        }

        public static DispoAdminModel Default { get { return _instance; } }

        private DispoAdminModel()            // get the DB connection string
        {
            _dbConnection = ConfigurationManager.ConnectionStrings["PrinterFarmB"].ConnectionString;
        }
        
        public PrinterfarmContext GetDBContext()     // deliver a new DB context with connection
        {
            return new PrinterfarmContext(_dbConnection);
        }
    }
}
