# cmbAssess

simple command-line Utility used assess the SCCM boundary entries against the DHCP scopes. this tool takes its data to compare a from database tables DHCPSubnet and CMBoundary imported/updated by other tools dhcpdump with the pre-defined fields. The DHCPSubnet table is created and filled up with the DHCP scopes info from the DHCP servers using the dhcpdump utility. The CMBoundary table and data is exported from the SCCM database using a separate export tool dbtcp. 
