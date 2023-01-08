#Get Bearer token
##################
$tenantId = 'xxxxxxx' # Paste your own tenant ID here
$appId = 'xxxxxxx' # Paste your own app ID here
$appSecr = 'xxxxxxxx' # Paste your own app secret here

$resourceAppIdUri = 'https://api.securitycenter.microsoft.com'
$oAuthUri = "https://login.microsoftonline.com/$TenantId/oauth2/token"
$authBody = [Ordered] @{
    resource = "$resourceAppIdUri"
    client_id = "$appId"
    client_secret = "$appSecr"
    grant_type = 'client_credentials'
}
$authResponse = Invoke-RestMethod -Method Post -Uri $oAuthUri -Body $authBody -ErrorAction Stop
$aadToken = $authResponse.access_token
$aadToken

#Get latest machines
####################
$alertUrl = "https://api-eu.securitycenter.microsoft.com/api/machines"
$headers = @{
    'Content-Type' = 'application/json'
    Accept = 'application/json'
    Authorization = "Bearer $aadToken"
}
$Responses = Invoke-WebRequest -Method Get -Uri $alertUrl -Headers $headers -ErrorAction Stop
$machines =  ($Responses | ConvertFrom-Json).value
# $Responses
$machines

#View each result
#################
Foreach($machine in $machines)
{
    $AssetName = $machine.computerDnsName
    $IpAddress = $machine.lastIpAddress
    $Status = $machine.exposureLevel
#json
########
$jsonBase = @{}
$list = New-Object System.Collections.ArrayList
$list.Add(@{"asset_name"="$AssetName"; "asset_tier"="null"; "domain"="null"; "ip_address"=$IPAddress; "ip_location"="null"; "manufacturer"="null"; "os"="null"; "comment"="null"; "asset_criticality"=@{"status"=$Status; "score"=3} })

$data = @(@{"type"="assets"; "id"="1"; "attributes"=@{"asset_type"="assets"; "assetList"=$list}})
$jsonBase.Add("data", $data)
$jbody = $jsonBase | convertTo-Json -Depth 10
$jbody

}

#PUT Assets
#############
$jwt_token = 'xxxxx'
$headers = @{
    'Content-Type' = 'application/json'
    Accept = 'application/json'
    Authorization = "Bearer $jwt_token"
    'X-Company-Short' = "test"
}
$queryUrl = "https://xxxxxxx.azurewebsites.net/v1/customers/test/assets/"
$queryResponse = Invoke-WebRequest -Method Put -Uri $queryUrl -Headers $headers -Body $jbody -ErrorAction Stop
$queryResponse
