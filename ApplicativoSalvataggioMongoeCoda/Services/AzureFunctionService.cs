using ApplicativoSalvataggioMongoeCoda.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicativoSalvataggioMongoeCoda.Services
{
    class AzureFunctionService
    {
        public Task<Billing> GetUpdatedBillings()
        {
            return Task.Run(() =>
            {
                using (SqlConnection con = new SqlConnection(Secrets.SQL_CONNECTION_STRING))
                using (SqlCommand cmd = new SqlCommand()
                {
                    Connection = con,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "SELECT * FROM tblBilling"
                })
                {
                    List<Billing> myListBill = new List<Billing>();
                    Billing myBill;
                    con.Open();
                    SqlDataReader mySqlDataReader = cmd.ExecuteReader();
                    while (mySqlDataReader.Read())//finchè c'è qualcosa da leggere, il metodo Read() restituisce true se trova una riga
                    {
                        myBill = new Billing()
                        {
                            halfanhour =(float)Convert.ToDouble(mySqlDataReader["halfanhour"]),
                            onehour = (float)Convert.ToDouble(mySqlDataReader["onehour"]),
                            threehours = (float)Convert.ToDouble(mySqlDataReader["threehours"]),
                            sixhours = (float)Convert.ToDouble(mySqlDataReader["sixhours"]),
                            daily = (float)Convert.ToDouble(mySqlDataReader["daily"]),
                        };
                        myListBill.Add(myBill);
                    }
                    return myListBill[0];
                }
            });
        }
    }
}
