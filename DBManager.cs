using System;
using System.Data;
using System.Data.SqlClient;


namespace cmbAssess
{
    class DBManager    
    {        
        
        //static string sConStr = "Server=PWGSDSPSSCCM7;Database=CM_JPP;Trusted_Connection=True";
        private SqlConnection sqlConn = null;
        private string connString = null;

        public DBManager(string dbConnStr)
        {
            this.connString = dbConnStr;
        }

        public void CloseConnection()
        {
            if (sqlConn != null) sqlConn.Close();
            sqlConn = null;
        }

        private SqlConnection OpenConnection()
        {
            if (sqlConn == null)
            {
                sqlConn = new SqlConnection(this.connString);
                sqlConn.Open();
            }
            return sqlConn;
        }

        public void GetDBData(IPManager mgr, string cmSiteCode)
        {
            Console.WriteLine("  Retrieving CMBoundary table records...");
            DataTable cbData = new DataTable();
            string cbQ = "select [BoundaryID],[Name],[BoundaryType],[Value],[NumericValueLow],[NumericValueHigh] from CMBoundary where [Name] LIKE '" + cmSiteCode +"%'";
            SqlCommand ccmd = new SqlCommand(cbQ, this.OpenConnection());
            SqlDataAdapter cda = new SqlDataAdapter(ccmd);
            cda.Fill(cbData);
            cda.Dispose();
            Console.WriteLine("  Retrieved CMBoundary records: " + cbData.Rows.Count);
            foreach (DataRow row in cbData.Rows)
            {
                CBRange cbr = ToCBRange(row);
                if (cbr != null) mgr.AddCBRange(cbr);
            }
            cbData.Dispose();

            Console.WriteLine("  Retrieving DHCPSubnet table records...");
            DataTable dhData = new DataTable();
            //string dhQ = "select [SubnetName],[SubnetIPAddress],[SubnetIPAddressNum],[SubnetMask],[SubnetState],[SubnetComment],[DSIPAddress],[DSShortName] from DHCPSubnet where [SubnetState]=0";
            string dhQ = "select [SubnetName],[SubnetIPAddress],[SubnetIPAddressNum],[SubnetMask],[SubnetState],[SubnetComment],[DSIPAddress],[DSShortName] from DHCPSubnet ORDER BY[SubnetState] ASC";
           SqlCommand dcmd = new SqlCommand(dhQ, this.OpenConnection());
            SqlDataAdapter dda = new SqlDataAdapter(dcmd);
            dda.Fill(dhData);
            dda.Dispose();
            Console.WriteLine("  Retrieved DHCPSubnet records: " + dhData.Rows.Count);
            foreach (DataRow row in dhData.Rows)
            {
                mgr.AddDHRange(ToDHRange(row));
            }
            dhData.Dispose();
            this.CloseConnection();
        }

        private CBRange ToCBRange(DataRow row)
        {
            try
            {
                //int bId, string bNam, int bTyp, string sVal, Int64 nLow, Int64 nHigh
                int id = Convert.ToInt32(row[0]);
                string n = row[1].ToString();
                int ty = Convert.ToInt32(row[2]);
                string v = row[3].ToString();
                Int64 nL = Convert.ToInt64(row[4]);
                Int64 nH = Convert.ToInt64(row[5]);
                return new CBRange(id, n, ty, v, nL, nH);
            }catch(Exception e)
            {
                Console.Out.WriteLine("Error@ToCBRange:" + e.Message);
                return null;
            }
        }

        private DHRange ToDHRange(DataRow row)
        {
            //string sNam, string sIP, Int64 nIP, string sMask, int nState, string sRem, string sSrvIP, string sSrvNm
            string sN = row[0].ToString().Trim();
            string sI = row[1].ToString().Trim();
            Int64 nI = Convert.ToInt64(row[2]);
            string sM = row[3].ToString().Trim();
            int nS = Convert.ToInt32(row[4]);
            string sR = row[5].ToString().Trim();
            string dI = row[6].ToString().Trim();
            string dS = row[7].ToString().Trim();
            return new DHRange(sN, sI, nI, sM, nS, sR, dI, dS);
        }

        //public void CopyDB(string sqlCmd, string destTableName)
        //{
        //    DataTable srcData = new DataTable();            
        //    SqlCommand ccmd = new SqlCommand(sqlCmd, this.OpenConnection(sConStr));
        //    SqlDataAdapter cda = new SqlDataAdapter(ccmd);
        //    cda.Fill(srcData);
        //    cda.Dispose();

        //    this.CloseConnection();

        //    SqlBulkCopy bc = new SqlBulkCopy(this.OpenConnection());
        //    bc.BulkCopyTimeout = 500; //500 seconds
        //    bc.DestinationTableName = destTableName;
        //    bc.WriteToServer(srcData);
        //    bc.Close();
        //    this.CloseConnection();
        //}

        public bool DBTableExists(string tableName)
        {
            string sqlStr = @"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='" + tableName + "') SELECT 1 ELSE SELECT 0";

            SqlCommand sqlCmd = new SqlCommand(sqlStr, this.OpenConnection());
            int x = Convert.ToInt32(sqlCmd.ExecuteScalar());
            return x == 1;
        }

        public int DBTableCreate(string tableName, string colAttr)
        {
            string sqlStr = "CREATE TABLE " + tableName + " (" + colAttr + ")";
            SqlCommand sqlCmd = new SqlCommand(sqlStr, this.OpenConnection());
            return sqlCmd.ExecuteNonQuery();
        }
        public int DBTableTruncate(string tableName)
        {
            string sqlStr = "TRUNCATE TABLE " + tableName;
            SqlCommand sqlCmd = new SqlCommand(sqlStr, this.OpenConnection());
            return sqlCmd.ExecuteNonQuery();
        }


        public bool WriteToTable(DataTable table)
        {
            SqlBulkCopy bc = new SqlBulkCopy(this.OpenConnection());
            try
            {
                bc.BulkCopyTimeout = 500; //500 seconds
                bc.DestinationTableName = table.TableName;
                bc.WriteToServer(table);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("SQLException: " + e);
                return false;
            }
            finally
            {
                bc.Close();
            }
            return true;
        }

    }
}
