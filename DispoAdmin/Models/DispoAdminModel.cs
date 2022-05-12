using Model3DFarm;
using System.Configuration;

namespace DispoAdmin.Models
{
    // this class does admin stuff for connecting a SQL database.
    class DispoAdminModel
    {
        private readonly string _dbConnection;
        static DispoAdminModel _instance;

        static DispoAdminModel()
        {
            _instance = new DispoAdminModel();
        }

        public static DispoAdminModel Default { get { return _instance; } }

        private DispoAdminModel()            // get the DB connection string
        {
            _dbConnection = ConfigurationManager.ConnectionStrings["PrinterFarm"].ConnectionString;
        }
        
        public PrinterfarmContext GetDBContext()            // deliver a new DB context with connection
        {
            return new PrinterfarmContext(_dbConnection);
        }
    }
}
