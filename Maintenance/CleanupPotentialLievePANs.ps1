$Server = "ecng-sql.database.windows.net"
$Database = "ecng-transactions"
$SQLQueryBase = @'
declare @tc table (
c varchar(4)
)

insert into @tc (c) values ('5903')
insert into @tc (c) values ('5895')
insert into @tc (c) values ('5846')
insert into @tc (c) values ('3720')
insert into @tc (c) values ('3712')
insert into @tc (c) values ('3704')
insert into @tc (c) values ('2649')
insert into @tc (c) values ('7999')
insert into @tc (c) values ('8054')
insert into @tc (c) values ('9479')
insert into @tc (c) values ('1847')
insert into @tc (c) values ('8960')
insert into @tc (c) values ('3717')
insert into @tc (c) values ('0443')
insert into @tc (c) values ('3229')
insert into @tc (c) values ('0427')
insert into @tc (c) values ('6793')
insert into @tc (c) values ('9146')
insert into @tc (c) values ('5241')
insert into @tc (c) values ('5627')
insert into @tc (c) values ('5043')
insert into @tc (c) values ('0062')
insert into @tc (c) values ('0062')
insert into @tc (c) values ('0037')
insert into @tc (c) values ('0052')
insert into @tc (c) values ('0144')
insert into @tc (c) values ('0152')
insert into @tc (c) values ('9322')
insert into @tc (c) values ('8548')
insert into @tc (c) values ('8241')
insert into @tc (c) values ('2546')
insert into @tc (c) values ('2595')
insert into @tc (c) values ('2553')
insert into @tc (c) values ('2637')
insert into @tc (c) values ('9223')
insert into @tc (c) values ('2611')
insert into @tc (c) values ('2637')
insert into @tc (c) values ('5023')
insert into @tc (c) values ('1934')
insert into @tc (c) values ('1926')
insert into @tc (c) values ('1918')
insert into @tc (c) values ('3719')
insert into @tc (c) values ('9103')
insert into @tc (c) values ('6075')
insert into @tc (c) values ('1418')
insert into @tc (c) values ('1523')
insert into @tc (c) values ('1473')
insert into @tc (c) values ('0005')
insert into @tc (c) values ('8431')
insert into @tc (c) values ('1000')
insert into @tc (c) values ('8250')
insert into @tc (c) values ('5904')
insert into @tc (c) values ('3237')
insert into @tc (c) values ('1117')
insert into @tc (c) values ('9424')
insert into @tc (c) values ('0000')
insert into @tc (c) values ('0505')
insert into @tc (c) values ('4444')
insert into @tc (c) values ('5100')
insert into @tc (c) values ('1111')
insert into @tc (c) values ('1881')
insert into @tc (c) values ('2222')
insert into @tc (c) values ('4561')
insert into @tc (c) values ('3742')
insert into @tc (c) values ('0016')
insert into @tc (c) values ('5555')
insert into @tc (c) values ('5556')
insert into @tc (c) values ('1111')
insert into @tc (c) values ('5100')
insert into @tc (c) values ('4242')
insert into @tc (c) values ('0347')
insert into @tc (c) values ('9299')
insert into @tc (c) values ('3222')
insert into @tc (c) values ('4580')
insert into @tc (c) values ('1142')
insert into @tc (c) values ('0006')
insert into @tc (c) values ('5879')
insert into @tc (c) values ('5887')
insert into @tc (c) values ('5911')
insert into @tc (c) values ('6839')
insert into @tc (c) values ('2017')
insert into @tc (c) values ('0126')
insert into @tc (c) values ('1816')
insert into @tc (c) values ('4620')
insert into @tc (c) values ('6666')
insert into @tc (c) values ('8886')

'@

$SQLQuerySelect = @'

select a.CreditCardTokenID from (
select c.CreditCardTokenID, c.CardNumber, t.c from CreditCardTokenDetails as c left outer join @tc as t on RIGHT(c.CardNumber, 4) = t.c
where c.Active = 1
) as a where a.c is null 
order by a.CreditCardTokenID desc

'@

$SqlQueryUpdate = @'

update c set c.CardNumber = '000000****0000', c.Active = 0 
from CreditCardTokenDetails as c left outer join @tc as t on RIGHT(c.CardNumber, 4) = t.c
where c.Active = 1 and t.c is null

'@



$Connection = New-Object System.Data.SQLClient.SQLConnection
$Connection.ConnectionString = "server='$Server';database='$Database';Authentication=Active Directory Integrated;"
$Connection.Open()

$DataTable = New-Object System.Data.DataTable

$Command = New-Object System.Data.SQLClient.SQLCommand
$Command.Connection = $Connection
$Command.CommandText = $SQLQueryBase + $SQLQuerySelect
$Reader = $Command.ExecuteReader()
$DataTable.Load($Reader)

foreach ($r in $DataTable.Rows)
{
	try {
		$cckey = $r.CreditCardTokenID
		Write-Host $cckey
		az keyvault secret delete --vault-name ecng-cctokens -n $cckey
	}
	catch {
		Write-Host $_
	}
}


# while ($Reader.Read()) {
# 	try {
# 		$cckey = $Reader.GetValue(0)
# 		Write-Host $cckey
# 		az keyvault secret delete --vault-name ecng-cctokens -n $cckey
# 	}
# 	catch {
# 		Write-Host $_
# 	}
# }

# $Command2 = New-Object System.Data.SQLClient.SQLCommand
# $Command2.Connection = $Connection
# $Command2.CommandText = $SQLQueryBase + $SqlQueryUpdate
# $res = $Command2.ExecuteNonQuery()
# Write-Host $res

$Connection.Close()


