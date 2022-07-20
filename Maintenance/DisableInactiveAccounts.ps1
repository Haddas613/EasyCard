# Connection information for Azure AD using Azure Automation account/runbook
$connectionName = AzureRunAsConnection
$servicePrincipalConnection=Get-AutomationConnection -Name $connectionName         
Connect-AzureAD -TenantId $servicePrincipalConnection.TenantId `
     -ApplicationId $servicePrincipalConnection.ApplicationId `
     -CertificateThumbprint $servicePrincipalConnection.CertificateThumbprint
 
# Manual connect to Azure AD
#Connect-AzureAD   
 
# Connection information for Graph API connection - specific to Agency
$clientID = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
$tenantName = "agency.onmicrosoft.com"
$clientSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
$resource = "https://graph.microsoft.com/"
 
$ReqTokenBody = @{
    Grant_Type    = "client_credentials"
    Scope         = "https://graph.microsoft.com/.default"
    client_Id     = $clientID
    Client_Secret = $clientSecret
} 
 
$TokenResponse = Invoke-RestMethod -Uri "https://login.microsoftonline.com/$TenantName/oauth2/v2.0/token" -Method POST -Body $ReqTokenBody
 
# Get all users in the tenant
$uri = 'https://graph.microsoft.com/beta/users?$select=displayName,userPrincipalName,signInActivity'
 
# Get todays date for date test later
$Today=(Get-Date)

# @odata.nextLink is used if results greated than 999 results
$Data = while (-not [string]::IsNullOrEmpty($uri)) {
    $apiCall = try {
        Invoke-RestMethod -Headers @{Authorization = "Bearer $($Tokenresponse.access_token)"} -Uri $uri -Method Get
    }
    catch {
        $errorMessage = $_.ErrorDetails.Message | ConvertFrom-Json
    }
    $uri = $null
    if ($apiCall) {
        $uri = $apiCall.'@odata.nextLink'
        $apiCall
    }
}
 
# Set the result into an variable
$result = ($Data | select-object Value).Value
$Export = $result | select DisplayName,UserPrincipalName,@{n="LastLoginDate";e={$_.signInActivity.lastSignInDateTime}}

# Export user data
$Users = $Export | select DisplayName,UserPrincipalName,@{Name='LastLoginDate';Expression={[datetime]::Parse($_.LastLoginDate)}}

#Disable accounts that are not breakglass that have not been used in 30 days.
Foreach ($User in $Users) {
    if ($User.LastLoginDate) {        
        $LastLogin = $User.LastLoginDate
        $TimeSpan = New-TimeSpan –Start $LastLogin –End $Today
        If ($TimeSpan.Days -gt 30) {
            write-host "User to be disabled true" $User.userPrincipalName "Last logon:"$user.LastLoginDate $TimeSpan.Days "days ago"
            If ($User.userPrincipalName -notlike '*break.glass*') {
                write-host $User.userPrincipalName "-User not breakglass account, proceed with disable of user"
                Set-AzureADUser -ObjectId $User.userPrincipalName -AccountEnabled $false
                Revoke-AzureADUserAllRefreshToken -ObjectId $User.userPrincipalName
            } else {
                write-host $User.userPrincipalName "- User is breakglass account, no action taken on user"
            }               
        }
        } else {
            write-host $User.userPrincipalName "-User is active within 30 day threshold, last logon:"$user.LastLoginDate " " $TimeSpan.Days " days ago"
        }
}