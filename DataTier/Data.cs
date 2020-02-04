using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using Utilities;

namespace DataTier
{
    public class Data : DataGlobals, IDisposable
    {
        protected string ConnectionString { get; set; }
        protected string Page { get; set; }
        protected string Process { get; set; }
        private bool disposed = false;
        public string Source { get { return source; } set { source = value; } }
        public string Result { get; set; }
        private string source = "Data";
        public bool Persistant { get; set; }
        private SqlCommand cmd { get; set; }
        private SqlConnection Connection { get; set; }
        private SqlTransaction Transaction { get; set; }
        private int connectionTime = 30;
        private SqlDataReader reader;

        public int UserID { get; set; }
        private List<SqlParameter> parameters;

        public int ConnectionTime
        {
            get { return connectionTime; }
            set { connectionTime = value; }
        }

        public SqlDataReader Reader
        {
            get { return reader; }
            set { reader = value; }
        }

        public Data(string configuration, string page, string process)
        {
            this.ConnectionString = GetConnectionString(configuration);
            this.Page = page;
            this.Process = process;
        }
        public bool AttachFileIn(string sName, byte[] ByteArr)
        {
            //used solely for inputting files
            bool bRet = false;

            try
            {
                SqlParameter p = new SqlParameter();
                p.Direction = ParameterDirection.Input;
                p.SqlDbType = SqlDbType.VarBinary;
                p.Value = ByteArr;
                p.Size = ByteArr.Length;
                p.ParameterName = sName;


                cmd.Parameters.Add(p);
            }
            catch (Exception ex)
            {
                MakeError(ex, "AttachFileIn", cmd.CommandText);
            }
            return bRet;

        }
        public string GetConnectionString(string connection)
        {
            string conn = "";
            try
            {
                conn = System.Configuration.ConfigurationManager.ConnectionStrings[connection].StringSafe();
            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            return conn;
        }

        private SqlConnection ConnectToSQL()
        {

            if (Persistant)
            {
                if (Connection != null)
                {
                    if (Connection.State == ConnectionState.Open)
                    {
                        return Connection;
                    }
                }
            }
            //the connection isn't established - open it
            Connection = new SqlConnection(ConnectionString);
            if (Persistant)
            {
                try
                {
                    Connection.Open();
                    Connection.ConnectionTimeout.Equals(6000);
                    Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                catch (Exception ex)
                {
                    MakeError(ex, "ConnectToSQL", "");
                }
            }
            return Connection;
        }
        public void SetGlobals(Globals globals)
        {
            Persistant = globals.bPersistant;
            cmd = globals.cmd;
            Connection = globals.mConn;
            Transaction = globals.Transaction;
            UserID = globals.User;

        }

        public void DoWait()
        {
            System.Threading.Thread.Sleep(2000);
        }
        public void MakeError(Exception ex, string Proc, string SQL)
        {
            try
            {
                if (SQL == null || SQL.Length == 0)
                {
                    SQL = cmd.CommandText.StringSafe();
                }
            }
            catch
            {
                if (cmd != null)
                    SQL = cmd.CommandText.StringSafe();
            }
            Errors error = new Errors(ConnectionString, Source);
            if (parameters != null)
            {
                Dictionary<string, object> prams = new Dictionary<string, object>();
                foreach (var item in parameters)
                {
                    prams.Add(item.ParameterName, item.Value);
                }
                error.Parameters = prams;
            }
            error.LogError(ex, UserID, Page, Proc, "An error caught in the Data class", SQL);
            this.Result = ex.Message;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (!disposed)
            {
                try
                {
                    if (Connection != null)
                    {
                        if (Connection.State == ConnectionState.Open)
                            Connection.Close();
                        Connection.Dispose();
                    }
                }
                catch
                {

                }
            }
            disposed = true;
        }
        #region "Public Functions"
        public void AddCommand(CommandType type, string sSQL)
        {
            try
            {
                cmd = new SqlCommand();
                cmd.CommandType = type;
                cmd.CommandTimeout = ConnectionTime;
                if (Connection == null)
                    if (ConnectToSQL() == null)
                    {
                        return;
                    }
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }
                if (Persistant)
                    cmd.Transaction = Transaction;
                cmd.Connection = Connection;
                cmd.CommandText = sSQL;
                Result = "Command Added";
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, sSQL);
            }
        }
        public void AttachParameterByValue(string sName, object Value)
        {
            cmd.Parameters.AddWithValue(sName, Value);
            if (parameters == null)
                parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter(sName, Value));

        }
        public bool AttachParameterOut(string Name, SqlDbType type, int Size)
        {
            SqlParameter p = new SqlParameter();
            p.ParameterName = Name;
            p.SqlDbType = type;
            p.Size = Size;
            p.Direction = ParameterDirection.Output;
            try
            {
                cmd.Parameters.Add(p);
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
                return false;
            }
            return true;
        }
        public bool CancelTransaction()
        {
            bool bRet = false;
            if (Persistant)
                if (Connection.State == ConnectionState.Open)
                    try
                    {
                        Transaction.Rollback();
                        bRet = true;
                    }
                    catch (Exception ex)
                    {
                        MakeError(ex, Process, cmd.CommandText);
                    }
            return bRet;
        }
        public bool CloseReader()
        {
            SqlDataReader dr = Reader;
            bool bRet = false;
            try
            {
                dr.Close();
                bRet = true;
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, "");
            }
            return bRet;
        }
        public bool CommitPersistant()
        {
            bool bRet = false;
            if (Persistant)
            {
                try
                {
                    Transaction.Commit();
                    bRet = true;
                    Connection.Close();
                    Connection.Dispose();
                    Transaction.Dispose();
                }
                catch (Exception ex)
                {
                    MakeError(ex, Process, cmd.CommandText);
                }
            }
            return bRet;
        }
        //this is a workhorse function.  Just execute the SQL Command as written, and return a -1 if it doesn't work
        public int ExecuteCommand()
        {
            int iRet = -1;
            try
            {
                iRet = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            return iRet;
        }
        public Array ExecuteCommandForArray()
        {
            SqlDataReader dr;
            ArrayList arr = new ArrayList();
            try
            {
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    arr.Add(dr[0].ToString());
                }
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return arr.ToArray();
        }
        public DataSet ExecuteCommandForDS()
        {
            DataSet ds = new DataSet();
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return ds;
        }
        public DataTable ExecuteCommandForDT()
        {
            SqlDataAdapter da;
            DataTable dt = new DataTable();
            try
            {
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return dt;
        }
        public MemoryStream ExecuteCommandGetFile()
        {
            MemoryStream mem = new MemoryStream();
            try
            {
                object img = cmd.ExecuteScalar();
                mem = new MemoryStream((byte[])img);
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return mem;
        }
        public bool ExecuteDataReader()
        {
            SqlDataReader dr;
            bool bRet = false;
            try
            {
                dr = cmd.ExecuteReader();
                if (!dr.Read())
                    Result = "DataReader found no data";
                else
                    bRet = true;
                Reader = dr;
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return bRet;
        }
        public object ExecuteScalar()
        {
            object iret = -1;
            try
            {
                iret = cmd.ExecuteScalar();
                if (iret == null)
                    iret = -1;
            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return iret;
        }
        //use baseValue for a value that indicates that the query failed
        public int ExecuteCommandForInt(int baseValue)
        {
            int iRet = baseValue;
            try
            {
                Utils.dfault = baseValue;
                iRet = (int)Utils.ParseNumControlledReturn(cmd.ExecuteScalar());

            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return iRet;
        }

        public string ExecuteCommandForString()
        {
            string sRet = "";
            try
            {
                sRet = cmd.ExecuteScalar().StringSafe();

            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return sRet;
        }
        public DataTable LongRun()
        {
            SqlDataAdapter da;
            DataTable dt = new DataTable();
            try
            {
                cmd.CommandTimeout = cmd.CommandTimeout * 2;
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);

            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return dt;
        }
        public DataTable WaitRun()
        {
            SqlDataAdapter da;
            DataTable dt = new DataTable();
            try
            {
                DoWait();
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);

            }
            catch (Exception ex)
            {
                MakeError(ex, Process, cmd.CommandText);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
                cmd.Dispose();
            }
            return dt;
        }
        #endregion
    }
}
