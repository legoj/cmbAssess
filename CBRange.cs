using System;


namespace cmbAssess
{
    class CBRange : IPRange
    {
        public int Id { get; }
        public int Type { get; }
        

        public CBRange(int bId, string bNam, int bTyp, string sVal, Int64 nLow, Int64 nHigh)
        {
            this.Id = bId;
            this.sName = bNam;
            this.Type = bTyp;
            this.sValue = sVal;
            this.nStartIP = nLow;
            this.nEndIP = nHigh;
            this.sStartIP = ToStringIP(nLow);
            this.sEndIP = ToStringIP(nHigh);
            this.nSize = nEndIP - nStartIP + 3;
            if (ISV2) this.nSize--;
            this.nSubnetIP = nStartIP - 1;
            this.sSubnetIP = ToStringIP(nSubnetIP);            
            this.nMaskLen = SizeToLength(nSize);
            this.sSubnetMask = CreateByNetBitLength(nMaskLen).ToString();
            this.FillBaseValues(0); //0 for boundary
            this.colValues[12] = this.Id;
        }
        public void MatchedDHCPInfo(int nState, string sComm, string sSrvName, string sSvrIP)
        {
            this.colValues[13] = nState;
            this.colValues[14] = sComm;
            this.colValues[15] = sSrvName;
            this.colValues[16] = sSvrIP;
        }

        public static CBRange testData()
        {

            int bId = 50333224;
            string sName = "JPP - Japan - Hyogo - Kobe - 8-6-26 Motoyama-Minami, Higashi-Nada";
            int nType = 3;
            string sValue = "10.28.79.1-10.28.79.254";
            Int64 nLow = 169627393;
            Int64 nHigh = 169627646;
            return new CBRange(bId, sName, nType, sValue, nLow, nHigh);
        }
    }
}
