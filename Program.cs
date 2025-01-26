using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace cmbAssess
{
    class Program
    {
        const string DEFCONSTRING = "Server=rkda0137\\SQLEXP;Database=JGDB;Trusted_Connection=True";
        const string DEFOUTDIR = @"C:\TEMP\CMBCHK\cbAssess";
        const string DEFSITECODE = "JPN";
        const string DEFCOUNTRY = "Japan";
        const string JPNPREFIX = DEFSITECODE + " - " + DEFCOUNTRY;
        static Dictionary<string, string> opts = null;
        const string _DB = "db"; //db connection string
        const string _OD = "od"; //output directory
        const string _NP = "np"; //boundary name prefix
        const string _SC = "sc"; //site code
        const string _CN = "cn"; //country or region name
        const string _UP = "upd"; //update database
        const string _CF = "csv"; //create CSV/input files
        const string _RE = "ref"; //don't run assessment, dumps all dhcp subnets for boundary refresh.        
        const char _DEL = ':';
        
        static void Main(string[] args) {
            if (args.Length == 0)
            {
                PrintHelp();
            }
            else
            {
                opts = new Dictionary<string, string>();
                ProcessArgs(args);

                bool bRef = opts.ContainsKey(_RE);

                string dbcStr = opts.ContainsKey(_DB) ? opts[_DB] : DEFCONSTRING;
                string sCode = opts.ContainsKey(_SC) ? opts[_SC] : DEFSITECODE;
                if (sCode.Equals(DEFSITECODE, StringComparison.CurrentCultureIgnoreCase)) IPRange.ISV2 = true;

                Console.WriteLine("Retrieving records from DB:" + dbcStr);
                DBManager dbm = new DBManager(dbcStr);
                IPManager ipm = new IPManager();
                dbm.GetDBData(ipm, sCode);
                Console.WriteLine("Retrieved unique records:");
                Console.WriteLine("  #CMBoundaryCount: " + ipm.CBCount);
                Console.WriteLine("  #DHCPSubnetCount: " + ipm.DHCount);
                if (bRef)
                {
                    Console.WriteLine("Skipping assessment; all active subnets considered as new...");
                    Console.WriteLine("  #DHCPSubnetCount: " + ipm.DHCount);
                }
                else { 
                    Console.WriteLine("Running assessment...");
                    ipm.RunAssessment();
                    Console.WriteLine("Assessment Result:");
                    Console.WriteLine("  #CMBSubnetPairs: " + ipm.PairCount);
                    Console.WriteLine("  #CMBoundaryLeft: " + ipm.CBCount);
                    Console.WriteLine("  #DHCPSubnetLeft: " + ipm.DHCount);
                    if (opts.ContainsKey(_UP))
                    {
                        Console.WriteLine("Updating assessment DB table...");
                        UpdateDB(ipm, dbm);
                    }
                }

                if (opts.ContainsKey(_CF) || bRef)                 
                {
                    Console.WriteLine("Writing result CSV/TXT files...");
                    string oDir = opts.ContainsKey(_OD) ? opts[_OD] : DEFOUTDIR;
                    string sPfx = sCode + " - " + (opts.ContainsKey(_CN) ? opts[_CN] : DEFCOUNTRY);
                    if (!Directory.Exists(oDir)) Directory.CreateDirectory(oDir); 
                    ipm.PrintToFile(oDir + "\\cbdhPairs_" + sCode + ".csv", typeof(IPPair));
                    ipm.PrintToFile(oDir + "\\cbRemains_" + sCode + ".csv", typeof(CBRange));
                    ipm.PrintToFile(oDir + "\\dhRemains_" + sCode + ".csv", typeof(DHRange));
                    ipm.CreateCMBAddParamFile(oDir + "\\NewBoundary_" + sCode + ".txt", sPfx);
                    ipm.CreateCMBValidationParamFile(oDir + "\\ChkBoundary_" + sCode + ".txt", sPfx);
                }
                Console.WriteLine("Closing DB connection...");
                dbm.CloseConnection();
            }
        }

        static void UpdateDB(IPManager ipm , DBManager dbm)
        {
            if (dbm.DBTableExists(IPRange.SQLTABLENAME))
                dbm.DBTableTruncate(IPRange.SQLTABLENAME);
            else
                dbm.DBTableCreate(IPRange.SQLTABLENAME, IPRange.SQLTABLEATTR);
            DataTable dat = ipm.GetDataTable();
            dbm.WriteToTable(dat);
            dat.Dispose();
        }
        static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("$> cmbAssess [upd] [csv] [db:DBConnString] [od:OutputDir] [sc:SiteCode] [cn:CountryName]");
            Console.WriteLine("\t" + _UP + " - updates the DB table of the assessment results.");            
            Console.WriteLine("\t" + _CF + " - dumps the assesment result into CSV files and");
            Console.WriteLine("\t      creates input text file list of new boundaries.");
            Console.WriteLine("\t" + _RE + " - dumps all active dhcp subnets for boundary refresh.");
            Console.WriteLine("\t      ignores upd option if specified.");
            Console.WriteLine("");
            Console.WriteLine("Default values if not specified:");
            Console.WriteLine("\t\"" + _DB + _DEL + DEFCONSTRING + "\"" );
            Console.WriteLine("\t\"" + _CF + _DEL + DEFOUTDIR + "\"");
            Console.WriteLine("\t\"" + _SC + _DEL + DEFSITECODE + "\"");
            Console.WriteLine("\t\"" + _CN + _DEL + DEFCOUNTRY + "\"");
        }

        static void ProcessArgs(string[] args)
        {
            char[] sep = new char[] { _DEL };
            foreach(string s in args)
            {
                string[] x = s.Split(sep,2);
                if (x.Length == 2)
                    opts.Add(x[0], x[1]);
                else
                    opts.Add(x[0], x[0]);
            }
        }
    }
}
