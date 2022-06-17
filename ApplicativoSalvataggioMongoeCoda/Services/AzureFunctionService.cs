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
                    CommandText = "SELECT * FROM tblPrezzi"
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
                            halfanhour =(float)Convert.ToDouble(mySqlDataReader["mezzora"]),
                            onehour = (float)Convert.ToDouble(mySqlDataReader["unora"]),
                            threehours = (float)Convert.ToDouble(mySqlDataReader["treore"]),
                            sixhours = (float)Convert.ToDouble(mySqlDataReader["seiore"]),
                            daily = (float)Convert.ToDouble(mySqlDataReader["giornaliero"]),
                        };
                        myListBill.Add(myBill);
                    }
                    return myListBill[0];
                }
            });
        }
    }
}
