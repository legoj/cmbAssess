using System;
using System.Net;
using System.Data;
using System.Data.SqlTypes;

namespace cmbAssess
{
    abstract class IPRange
    {
        public static bool ISV2 = false;
        protected string sName;
        protected Int64 nSubnetIP;    
        protected string sSubnetIP;
        protected Int64 nStartIP;
        protected string sStartIP;
        protected Int64 nEndIP;        
        protected string sEndIP;
        protected long nSize;
        protected int nMaskLen;
        protected string sSubnetMask;
        protected string sValue;
        protected object[] colValues;

        
        public string Name { get { return sName; } }
        public string SubnetIP { get { return sSubnetIP; } }
        public Int64 SubnetIPNum { get { return nSubnetIP; } }
        public string StartIP { get { return sStartIP; } }
        public Int64 StartIPNum { get { return nStartIP; } }
        public string EndIP { get { return sEndIP; } }
        public Int64 EndIPNum { get { return nEndIP; } }        
        public long Size { get { return nSize; } }
        public int MaskLength { get { return nMaskLen; } }        
        public string SubnetMask { get { return sSubnetMask; } }
        public string Value { get { return sValue; } }
        public object[] FieldValues { get { return colValues; } }

        public int CheckOverlap(IPRange d)
        {
            bool sS = nStartIP <= d.StartIPNum && d.StartIPNum <= nEndIP;
            bool eS = nStartIP <= d.EndIPNum && d.EndIPNum <= nEndIP;
            if (!sS && eS) return 1;
            if (sS && !eS) return 2;
            if (sS && eS) return 3;
            return 0;
        }

        protected static string ToStringIP(Int64 numIpAddress)
        {
            return IPAddress.Parse(numIpAddress.ToString()).ToString();
        }
        protected static Int64 ToNumericIP(string strIpAddress)
        {
            return (long)(uint)IPAddress.NetworkToHostOrder((int)IPAddress.Parse(strIpAddress).Address);
        }
        protected static int SizeToLength(long size)
        {
            long n = size;
            long t;
            int z = 0;
            do
            {
                t = n % 2;
                n = n / 2;
                z++;
            } while (n != 1);
            return 32 - z;
        }

        public static IPAddress CreateByHostBitLength(int hostpartLength)
        {
            int hostPartLength = hostpartLength;
            int netPartLength = 32 - hostPartLength;

            if (netPartLength < 2)
                throw new ArgumentException("Number of hosts is to large for IPv4");

            Byte[] binaryMask = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                if (i * 8 + 8 <= netPartLength)
                    binaryMask[i] = (byte)255;
                else if (i * 8 > netPartLength)
                    binaryMask[i] = (byte)0;
                else
                {
                    int oneLength = netPartLength - i * 8;
                    string binaryDigit =
                        String.Empty.PadLeft(oneLength, '1').PadRight(8, '0');
                    binaryMask[i] = Convert.ToByte(binaryDigit, 2);
                }
            }
            return new IPAddress(binaryMask);
        }

        public static IPAddress CreateByNetBitLength(int netpartLength)
        {
            int hostPartLength = 32 - netpartLength;
            return CreateByHostBitLength(hostPartLength);
        }

        public static IPAddress CreateByHostNumber(int numberOfHosts)
        {
            int maxNumber = numberOfHosts + 1;

            string b = Convert.ToString(maxNumber, 2);

            return CreateByHostBitLength(b.Length);
        }
       
        public static string _HDR = "\"SubnetIP\",\"SubnetMask\",\"SubnetIPNum\",\"StartIP\",\"StartIPNum\",\"EndIP\",\"EndIPNum\",\"Size\",\"MaskLength\",\"Value\",\"Name\"";
        
        public string ToCSV()
        {
            return "\"" +SubnetIP + "\",\"" + SubnetMask + "\",\"" + SubnetIPNum + "\",\"" + StartIP + "\",\"" + StartIPNum + "\",\"" + EndIP + "\",\"" + EndIPNum + "\",\"" + Size + "\",\"" + MaskLength + "\",\"" + Value + "\",\"" + Name + "\"";
        }

        public const string SQLTABLENAME = "IPRange";
        public const string SQLTABLEATTR = "RecordType int NOT NULL, SubnetIP nvarchar(20) NOT NULL, SubnetIPNum bigint NOT NULL, StartIP nvarchar(20) NOT NULL, StartIPNum bigint NOT NULL, LastIP nvarchar(20) NOT NULL, LastIPNum bigint NOT NULL, " +
                                           "SubnetMask nvarchar(20) NOT NULL, SubnetSize bigint NOT NULL, MaskLength int NOT NULL, Value nvarchar(255) NOT NULL, Name nvarchar(255) NOT NULL, " +
                                           "BoundaryId int NULL, SubnetState int NULL, SubnetComment nvarchar(255) NULL, DHCPServerName nvarchar(25) NULL, DHCPServerIP nvarchar(20) NULL, Assessment nvarchar(15) NULL, Remarks nvarchar(255) NULL, CheckedDate datetime DEFAULT(getdate())";

        public static DataTable createUpdateTable()
        {
            DataTable tab = new DataTable(SQLTABLENAME);
            tab.Columns.Add(new DataColumn("RecordType", typeof(SqlInt32)));   //either, boundary=0 or dhcp=1
            tab.Columns.Add(new DataColumn("SubnetIP"));
            tab.Columns.Add(new DataColumn("SubnetIPNum", typeof(SqlInt64)));                        
            tab.Columns.Add(new DataColumn("StartIP"));
            tab.Columns.Add(new DataColumn("StartIPNum", typeof(SqlInt64)));
            tab.Columns.Add(new DataColumn("LastIP"));
            tab.Columns.Add(new DataColumn("LastIPNum", typeof(SqlInt64)));
            tab.Columns.Add(new DataColumn("SubnetMask"));
            tab.Columns.Add(new DataColumn("SubnetSize", typeof(SqlInt64)));
            tab.Columns.Add(new DataColumn("MaskLength", typeof(SqlInt32)));
            tab.Columns.Add(new DataColumn("Value"));                     //<startip>-<lastip> format
            tab.Columns.Add(new DataColumn("Name"));
            tab.Columns.Add(new DataColumn("BoundaryId", typeof(SqlInt32)));  //only for boundary
            tab.Columns.Add(new DataColumn("SubnetState", typeof(SqlInt32)));   //only for dhcp scopes
            tab.Columns.Add(new DataColumn("SubnetComment"));   //only for dhcp scopes
            tab.Columns.Add(new DataColumn("DHCPServerName"));
            tab.Columns.Add(new DataColumn("DHCPServerIP"));
            tab.Columns.Add(new DataColumn("Assessment"));  //sizechanged, name changed, etc... 
            tab.Columns.Add(new DataColumn("Remarks"));  //sizechanged, name changed, etc... 
            return tab;
        }
        protected void FillBaseValues(int recType)
        {
            if (this.colValues == null)
            {
                this.colValues = new object[19];
                this.colValues[0] = recType;
                this.colValues[1] = this.SubnetIP;
                this.colValues[2] = this.SubnetIPNum;
                this.colValues[3] = this.StartIP;
                this.colValues[4] = this.StartIPNum;
                this.colValues[5] = this.EndIP;
                this.colValues[6] = this.EndIPNum;
                this.colValues[7] = this.SubnetMask;
                this.colValues[8] = this.Size;
                this.colValues[9] = this.MaskLength;
                this.colValues[10] = this.Value;
                this.colValues[11] = this.Name;
            }
        }
        public void AddRemarks(string ass, string rem)
        {
            this.colValues[17] = ass;
            this.colValues[18] = rem;
        }

    }

}
