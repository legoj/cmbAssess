using System;
using System.Collections.Generic;

namespace cmbAssess
{
    class DHRange : IPRange
    {
        public int State { get; }
        public string Comments { get; }
        public string ServerIP { get; }
        public string ServerName { get; }
        private List<DHRange> dupScopes;
        

        public DHRange(string sNam, string sIP, Int64 nIP, string sMask, int nState, string sRem, string sSrvIP, string sSrvNm)
        {            
            this.sName = sNam;
            this.sSubnetIP = sIP;
            this.nSubnetIP = nIP;
            this.sSubnetMask = sMask;
            this.State = nState;
            this.Comments = sRem;
            this.ServerIP = sSrvIP;
            this.ServerName = sSrvNm;

            IPNetwork ip = IPNetwork.Parse(sIP, sMask);
            this.sStartIP = ip.FirstUsable.ToString();
            this.sEndIP = ISV2 ? ip.LastIPAddress.ToString() : ip.LastUsable.ToString();
            this.nStartIP = ToNumericIP(sStartIP);
            this.nEndIP = ToNumericIP(sEndIP);
            this.nSize = ip.Usable + 2;
            this.sValue = sStartIP + "-" + sEndIP;
            this.nMaskLen = SizeToLength(nSize);
            ip = null;
            this.FillBaseValues(1);
            this.colValues[13] = this.State;
            this.colValues[14] = this.Comments;
            this.colValues[15] = this.ServerName;
            this.colValues[16] = this.ServerIP;
        }
        
        public void MatchedBoundaryId(int boundId)
        {
            this.colValues[12] = boundId;
        }
        public void AddDuplicate(DHRange dhr)
        {
            if (this.dupScopes == null) this.dupScopes = new List<DHRange>();
            dhr.AddRemarks("HAS_DUPLICATE","DHCP::SubnetName=" + this.Name + ", Mask=" + this.SubnetMask + ", Size=" + this.Size);
            this.dupScopes.Add(dhr);
        }
        public bool HasDuplicates()
        {
            return this.dupScopes != null && this.dupScopes.Count > 0;
        }
        public List<DHRange> GetDuplicates()
        {
            return this.dupScopes;
        }
        public override string ToString()
        {
            return "len: " + SizeToLength(nSize);
            //return Value + "::" + ValueLow + ", " + ValueHigh + ", " + StartIP + ", " + ToNumericIP(StartIP) + ", " + EndIP + ", " + ToNumericIP(EndIP); 
            //return Id + ", " + Name + ", " + Type + ", " + Value + ", " + ValueLow + ", " + ValueHigh + ", " + StartIP + ", " + EndIP
        }

        public static DHRange testData()
        {
            string sName = "Saisentan Takamatsu CiscoIPP V401 (AIU/FFM) Sec";
            string sIP = "10.28.79.0";
            string sMask = "255.255.255.0";
            Int64 nIP = 169627392;
            int nState = 0;
            string sCmt = "Scope Comments des'";
            string dSIP = "170.105.29.207";
            string dSHN = "skdhcat2";
            return new DHRange(sName, sIP, nIP, sMask, nState, sCmt, dSIP, dSHN);
        }
    }

}
