using System.Data.SqlClient;
using System.Data;

namespace DataTier
{
    public class Globals
    {
        private bool _Persistant;                               //we are persisant when we use a transaction
        private SqlConnection _Conn;                            //sql connection
        private SqlCommand _cmd;                                //sql command - put it here to pass it between functions
        private DataTable _dt = new DataTable();                //datatable used to keep a view of the database schema
        private SqlTransaction _Transaction;                    //carry the transaction here so that we can pass it between functions
        private int _User = -1;                              // the user variable is for error handling, which we'll add later

        public bool bPersistant
        {
            get { return _Persistant; }
            set { _Persistant = value; }
        }

        public SqlCommand cmd
        {
            get { return _cmd; }
            set { _cmd = value; }
        }

        public SqlConnection mConn
        {
            get { return _Conn; }
            set { _Conn = value; }
        }

        public SqlTransaction Transaction
        {
            get { return _Transaction; }
            set { _Transaction = value; }
        }

        public int User
        {
            get { return _User; }
            set { _User = value; }
        }
    }
}
