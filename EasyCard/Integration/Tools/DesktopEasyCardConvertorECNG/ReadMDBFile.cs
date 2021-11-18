using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Common.Models.MDBSource;
using Common.Models;
using System.Data.OleDb;

namespace DesktopEasyCardConvertorECNG
{
    public static class ReadMDBFile
    {
        public static async Task<DataFromMDBFile> ReadDataFromMDBFile(string pathToMDBFile)
        {
            try
            {
                string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\vvs\\Downloads\\EasyCardBilling.mdb;Jet OLEDB:Database Password=D09h3c;";
                using (OleDbConnection myConnection = new OleDbConnection(connectionString))
                {
                    myConnection.Open();
                    //execute queries, etc
                    var billingSettings = await myConnection.QueryAsync<BillingSettings>("SELECT * FROM tblOptions");
                    var items = await myConnection.QueryAsync<Product>("SELECT * FROM tblRivName");
                    var itemsnotActive = await myConnection.QueryAsync<NotActiveProduct>("SELECT CusProd.DealText as ProductName,DealSum/DealCount as ProdSum,'notActive',CusProd.rivid as DesktopRecordID,ProdTab.RivCode as RivCoded from tblDealProp   as CusProd inner join tblrivName  as ProdTab on  CusProd.rivid = ProdTab.revid where    ProdTab.rivName<> CusProd.DealText");
                    var customers = await myConnection.QueryAsync<Customer>("SELECT * FROM tblDeal");
                    var productsPerCustomer = await myConnection.QueryAsync<ProductPerCustomer>("SELECT DealID, DealText, DealSum/ DealCount as ProdSum,DealCount,RivID FROM tblDealProp");
                    var rowndsProduct = await myConnection.QueryAsync<RowndsProductsPerCustomer>("select customers.dealid as DealID, customers.totalsum, sums.customerTotalSum, sums.customerTotalSum * 1.17 as sumprodWithMaam from tblDeal as customers inner join(select DEALID, SUM(DealSum) as customerTotalSum from tbldealPROP GROUP BY DEALID) as sums on sums.dealid = customers.dealid");
                    DataFromMDBFile data = new DataFromMDBFile() { BillingSetting = billingSettings.AsList()[0], Products = items.AsList(), NotActiveProducts = itemsnotActive.AsList(), Customers = customers.AsList(), ProductsPerCustomer = productsPerCustomer.AsList(), RowndsProductsPerCustomer = rowndsProduct.AsList() };
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OLEDB Connection FAILED: " + ex.Message);
                throw;
            }
        }
    }
}
