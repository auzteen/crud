- work on Tartarus/app/azfunc
+ DefenderScore.cs
+ VulnerabilityDiscovery.cs



Tartarus - Get Defender Vulnerabilities
########################################
1. Get Defender Vulnerability and Defender machine assets ( GET https://api.securitycenter.microsoft.com/api/machines )
2. Map Defender machine id (SourceId) and AssetName to Defender Vulnerability machineId JSON model: SourceId

Note: Vulnerability.machineId = Asset.id (SourceId)

Data Model
############
-- Similar to Citadel DefenderAssests on Citadel/app/azfunc..
* Model/DefenderHost.cs, Service/AssetService, Interface/IAssetService.cs, Constants.cs, AssetDefender.cs

https://learn.microsoft.com/en-us/microsoft-365/security/defender-endpoint/get-all-vulnerabilities-by-machines?view=o365-worldwide

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

(2.) Get Device Score Data ; Score: 451
https://learn.microsoft.com/en-us/microsoft-365/security/defender-endpoint/get-device-secure-score?view=o365-worldwide
