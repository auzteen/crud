Tartarus - Get Defender Vulnerabilities
########################################
1. Get Defender Vulnerability and Defender machine assets
2. Map Defender machine id (SourceId) and AssetName to Defender Vulnerability machineId JSON model: SourceId

Note: Vulnerability.machineId = Asset.id (SourceId)


Data Model
############
-- Similar to Citadel DefenderAssests...


#########################################################################################
# JSON SPEC. #
##############
{
"data": [
   {
     "type": "Vulnerabilities",
     "attributes": {
       "AssetName": "",  (from Get Machine Asset)  
       "AssetType": "",
       "CompanyShortName": "",
       "Source": "MS Defender",
       "SourceId": "",   (from Vulnerability.machineId == get defender macine asset id)
       "OS": "",
       "VendorName": "",
       "VendorReference": "",
       "ProductName": "",
       "ProductVersion": "",
       "CVEID": "",
       "IPAddress": "",
       "FQDN": "",
       "Severity": ""
     }
   }
 ]
}

########################################################################################

