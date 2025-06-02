# cmbAssess

this is a simple command-line utility used to assess the SCCM boundary entries against the DHCP scopes. the tool takes its data to compare a from database tables DHCPSubnet and CMBoundary imported/updated by separate tools with the pre-defined fields. 
* The DHCPSubnet table is created and filled up with the DHCP scopes info from the DHCP servers using the ***dhcpdump*** utility.
* The CMBoundary table and data is exported from the SCCM database using a separate export tool called ***dbtcp***.

for more info on the other tools, please check on their respective repository. 
