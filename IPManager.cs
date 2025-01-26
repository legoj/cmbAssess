using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace cmbAssess
{
    class IPManager
    {

        private Dictionary<string, CBRange> cbIP;
        private Dictionary<string, DHRange> dhIP;
        private Dictionary<string, IPPair> pairIP;
        const string _DEL = "[DEL]";
        const string _ADD = "[ADD]";
        const string _MOD = "[MOD]";
        const string _DIS = "[DIS]";

        public IPManager()
        {
            this.cbIP = new Dictionary<string, CBRange>();
            this.dhIP = new Dictionary<string, DHRange>();
            this.pairIP = new Dictionary<string, IPPair>();
            //this.dupIP = new List<DHRange>();
        }
        public void AddCBRange(CBRange cb)
        {
            if (this.cbIP.ContainsKey(cb.SubnetIP))
            {
                Console.WriteLine("WARNING: Duplicate boundary!!! -> " + cb.SubnetIP);
            }
            else
            {
                this.cbIP.Add(cb.SubnetIP, cb);
            }
        }
        public void AddDHRange(DHRange dh)
        {
            if (this.dhIP.ContainsKey(dh.SubnetIP))
            {
                dhIP[dh.SubnetIP].AddDuplicate(dh);
            }
            else
            {
                this.dhIP.Add(dh.SubnetIP, dh);
            }
        }
        public int CBCount { get { return this.cbIP.Count; } }
        public int DHCount { get { return this.dhIP.Count; } }
        public int PairCount { get { return this.pairIP.Count; } }
        //public int DupCount { get { return this.dupIP.Count; } }

        public void RunAssessment()
        {
            foreach(string key in cbIP.Keys)
            {
                if (this.dhIP.ContainsKey(key))
                {
                    this.pairIP.Add(key, new IPPair(key, cbIP[key], dhIP[key]));
                    this.dhIP.Remove(key);  //remove the dhcp subnet item from the dhIP list
                }
            }
            //remove the matched boundary items from the cbIP list
            foreach(string k in pairIP.Keys)
            {
                this.cbIP.Remove(k);
            }

            //loop into the leftover of DHCP ranges
            foreach (DHRange d in dhIP.Values)
            {
                if(d.State == 0) 
                    d.AddRemarks("NEW_DHCPSCOPE","DHCP.State is ENABLED!");
                else
                    d.AddRemarks("OLD_DHCPSCOPE", "DHCP.State is DISABLED!");
            }
            //loop into the leftover of CB ranges
            foreach (CBRange c in cbIP.Values)
            {
                c.AddRemarks("OLD_BOUNDARY","No DHCPSubnet match found!");
            }


        }
        public void PrintPairs()
        {
            foreach (IPPair p in pairIP.Values)
            {
                Console.WriteLine(p.ToCSV());
            }                
        }
        public void PrintLeftDH()
        {
            foreach (DHRange d in dhIP.Values)
            {
                Console.WriteLine( d.ToCSV() + ",\"" + d.ServerName + "\",\"" + d.ServerIP + "\",\"" + d.Comments + "\"");
            }
        }
        public void PrintLeftCB()
        {
            foreach (CBRange c in cbIP.Values)
            {
                Console.WriteLine("\"" + c.Id + "\"," + c.ToCSV());
            }
        }

        public void PrintToFile(string outFilePath, Type type)
        {
            Console.WriteLine("Writing output file: " + outFilePath);
            StreamWriter sw = new StreamWriter(outFilePath, false, System.Text.Encoding.UTF8);
            if(type==typeof(DHRange))
            {
                sw.WriteLine(DHRange._HDR + ",\"ServerName\",\"ServerIP\",\"Comments\"");
                foreach (DHRange d in dhIP.Values)
                {
                    sw.WriteLine(d.ToCSV() + ",\"" + d.ServerName + "\",\"" + d.ServerIP + "\",\"" + d.Comments + "\"");
                }
            }
            if (type == typeof(CBRange))
            {
                sw.WriteLine("\"BoundaryId\"," + CBRange._HDR);
                foreach (CBRange c in cbIP.Values)
                {
                    sw.WriteLine("\"" + c.Id + "\"," + c.ToCSV());
                }
            }
            if (type == typeof(IPPair))
            {
                sw.WriteLine(IPPair._HDR);
                foreach (IPPair p in pairIP.Values)
                {
                    sw.WriteLine(p.ToCSV());
                }
            }

            sw.Close();
        }
        /*
        public void PrintDuplicateSubnets(string outFilePath)
        {
            StreamWriter sw = new StreamWriter(outFilePath, false, System.Text.Encoding.UTF8);
            sw.WriteLine(IPRange._HDR + ",\"Remarks\"");
            foreach (DHRange d in dupIP)
            {
                sw.WriteLine(d.ToCSV() + ",\"" + d.State + "\",\"" + d.ServerName + "\",\"" + d.ServerIP + "\",\"" + d.Comments + "\"");
            }
            sw.Close();
        }
        public void FindDHOverlap(string outFilePath)
        {
            StreamWriter sw = new StreamWriter(outFilePath, false, System.Text.Encoding.UTF8);
            sw.WriteLine("\"OverlapCheck\"," + IPRange._HDR + ",\"State\",\"ServerName\",\"ServerIP\",\"CMBoundaryId\"");
            foreach (DHRange d in dupIP)
            {
                int oC = 0;
                string dS = "\"[SubjectIPRange]\"," + d.ToCSV() + ",\"" + d.ServerName + "\"";
                foreach (IPPair p in pairIP.Values)
                {
                    oC = p.DHCPSubnet.CheckOverlap(d);
                    if (oC > 0)
                    {
                        dS = dS + "\r\n\"" + oC + "\"" + p.DHCPSubnet.ToCSV() + ",\"" + p.DHCPSubnet.ServerName + ";"  + p.CMBoundary.Id  + "\"";
                    }
                }
                foreach (CBRange c in cbIP.Values)
                {
                    oC = c.CheckOverlap(d);
                    if (oC > 0)
                    {
                        dS = dS + "\r\n\"" + oC + "\"" + c.ToCSV() + ",\"" + c.Id + "\"";
                    }
                }
                if (oC > 0)
                    sw.WriteLine(dS);
            }
            sw.Close();
        }
        */

        public void CreateCMBAddParamFile(string outFilePath, string namePrefix)
        {
            Console.WriteLine("Creating CMAddBoundary input file: " + outFilePath);
            StreamWriter sw = new StreamWriter(outFilePath, false, System.Text.Encoding.UTF8);
            int cnt = 0;
            foreach (DHRange d in dhIP.Values)
            {
                if (d.State == 0)
                {
                    cnt++;
                    string x = namePrefix + " - " + d.Name + "\t" + d.StartIP + "\t" + d.EndIP;
                    sw.WriteLine(x);
                    Console.WriteLine("  [" + cnt + "]\t" + x);
                }
            }
            Console.WriteLine("New boundary entries: " + cnt);
            sw.Close();
        }

        public void CreateCMBValidationParamFile(string outFilePath, string namePrefix)
        {
            Console.WriteLine("Creating CMBoundaryValidation input file: " + outFilePath);
            StreamWriter sw = new StreamWriter(outFilePath, false, System.Text.Encoding.UTF8);


            Console.WriteLine("Checking new active DCHP subnets... ");
            int addCnt = 0;
            foreach (DHRange d in dhIP.Values)
            {
                if (d.State == 0)
                {
                    addCnt++;
                    string x = _ADD + "\t" + namePrefix + " - " + d.Name + "\t" + d.StartIP + "\t" + d.EndIP + "\t" + d.SubnetIP + "\t" + d.SubnetMask + "\t" + d.ServerName;
                    sw.WriteLine(x);
                }
            }
            Console.WriteLine("AddCount: " + addCnt);

            Console.WriteLine("Checking modified/disabled subnets... ");
            int modCnt = 0;
            int delCnt = 0;
            foreach (IPPair p in pairIP.Values)
            {
                string x = p.CMBoundary.Id + "\t" + p.CMBoundary.Name + "\t" + p.CMBoundary.Value;
                if (p.DHCPSubnet.State == 0)
                {
                    string xN = namePrefix + " - " + p.DHCPSubnet.Name;
                    if(!p.CMBoundary.Name.Equals(xN,StringComparison.CurrentCultureIgnoreCase) || p.ChangedSize())
                    {
                        modCnt++;
                        x = _MOD + "\t" + x + "\t" + xN + "\t" + p.DHCPSubnet.Value + "\t" + p.DHCPSubnet.ServerName;
                        sw.WriteLine(x);
                    }
                }
                else
                {
                    delCnt++;
                    x = _DIS + "\t" + x + "\t" + p.DHCPSubnet.Name + "\t" + p.DHCPSubnet.SubnetIP + "\t" + p.DHCPSubnet.ServerName;
                    sw.WriteLine(x);
                } 
            }
            Console.WriteLine("ModCount: " + modCnt);

            Console.WriteLine("Checking orphaned boundaries... ");
            foreach (CBRange c in cbIP.Values)
            {
                delCnt++;
                sw.WriteLine(_DEL + "\t" + c.Id + "\t" + c.Name + "\t" + c.Value + "\t" + c.SubnetIP + "\t" + c.SubnetMask + "\t" + c.Size);
            }
            Console.WriteLine("DelCount: " + delCnt);
            sw.Close();
        }

        public DataTable GetDataTable()
        {
            DataTable tab = IPRange.createUpdateTable();
            foreach(IPPair p in pairIP.Values)
            {
                tab.Rows.Add(p.CMBoundary.FieldValues);
                AddDHRangeToTable(tab,p.DHCPSubnet);
            }
            //loop into the leftover of DHCP ranges
            foreach (DHRange d in dhIP.Values)
            {
                AddDHRangeToTable(tab, d);
            }
            //loop into the leftover of CB ranges
            foreach (CBRange c in cbIP.Values)
            {
                tab.Rows.Add(c.FieldValues);
            }
            return tab;
        }

        private void AddDHRangeToTable(DataTable tab, DHRange d)
        {
            tab.Rows.Add(d.FieldValues);
            if (d.HasDuplicates())
            {
                foreach (DHRange dd in d.GetDuplicates())
                    tab.Rows.Add(dd.FieldValues);
            }
        }


    }
}
