using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace cmbAssess
{
    class IPPair
    {
        private string pKey;
        private CBRange cbr;
        private DHRange dhr;

        public CBRange CMBoundary { get { return cbr; } }
        
        public DHRange DHCPSubnet { get { return dhr; } }
        public string IPAddress { get { return pKey; } }

        public IPPair (string key, CBRange cb, DHRange dh)
        {
            this.pKey = key;
            this.cbr = cb;
            this.dhr = dh;
            this.dhr.MatchedBoundaryId(this.cbr.Id);
            this.cbr.MatchedDHCPInfo(this.dhr.State, this.dhr.Comments, this.dhr.ServerName, this.dhr.ServerIP);
            if (ChangedSize())
            {
                this.cbr.AddRemarks("SIZE_CHANGED", "DHCP::SubnetName=" + this.dhr.Name + ", Mask=" + this.dhr.SubnetMask + ", Size=" + this.dhr.Size );
                this.dhr.AddRemarks("SIZE_CHANGED", "SCCM::BoundaryName=" + this.cbr.Name + ", Value=" + this.cbr.Value + ", Size=" + this.cbr.Size);
            }
            else
            {
                this.dhr.AddRemarks("ASSESSMENT_OK","SCCM::BoundaryName="+ this.cbr.Name );
                this.cbr.AddRemarks("ASSESSMENT_OK","DHCP::SubnetName=" + this.dhr.Name );
            }
        }
        public static string _HDR = "\"BoundaryId\",\"SizeChanged\"," + CBRange._HDR + "," + DHRange._HDR + ",\"ServerName\",\"ServerIP\"";
        
        public string ToCSV()
        {
            return "\"" + cbr.Id + "\",\"" + ChangedSize() + "\"," + cbr.ToCSV() + "," + dhr.ToCSV() + ",\"" + dhr.ServerName + "\",\"" + dhr.ServerIP + "\"";
        }
        public bool ChangedSize()
        {
            return cbr.Size != dhr.Size;
        }


    }
}
