# use az login before running this script

# How do I lock down the access to my backend to only Azure Front Door?
# https://docs.microsoft.com/en-gb/azure/frontdoor/front-door-faq#how-do-i-lock-down-the-access-to-my-backend-to-only-azure-front-door-service
# https://www.microsoft.com/en-us/download/details.aspx?id=56519

$ipRanges = @(
"20.41.4.88/29",
"20.41.64.120/29",
"20.41.192.104/29",
"20.42.4.120/29",
"20.42.129.152/29",
"20.42.224.104/29",
"20.43.41.136/29",
"20.43.65.128/29",
"20.43.130.80/29",
"20.45.112.104/29",
"20.45.192.104/29",
"20.150.160.96/29",
"20.189.106.112/29",
"20.192.161.104/29",
"20.192.225.48/29",
"40.67.48.104/29",
"40.74.30.72/29",
"40.80.56.104/29",
"40.80.168.104/29",
"40.80.184.120/29",
"40.82.248.248/29",
"40.89.16.104/29",
"51.12.41.8/29",
"51.12.193.8/29",
"51.104.25.128/29",
"51.105.80.104/29",
"51.105.88.104/29",
"51.107.48.104/29",
"51.107.144.104/29",
"51.120.40.104/29",
"51.120.224.104/29",
"51.137.160.112/29",
"51.143.192.104/29",
"52.136.48.104/29",
"52.140.104.104/29",
"52.150.136.120/29",
"52.228.80.120/29",
"102.133.56.88/29",
"102.133.216.88/29",
"147.243.0.0/16",
"191.233.9.120/29",
"191.235.225.128/29"
)

$i = 9
$resourceGroup = 'PayDay-ClearingHouse-UAT'
$app = 'PayDay-ClearingHouse-UAT-Merchant'
$rulePrefix = 'FrontDoor'
$orderPrefix = 300

foreach ($ipRange in $ipRanges) {

	$ruleName = $rulePrefix + $i
	$priority = $orderPrefix + $i

	az webapp config access-restriction add --resource-group $resourceGroup --name $app --rule-name $ruleName --action Allow --ip-address $ipRange --priority $priority
	
	$i = $i + 1
}