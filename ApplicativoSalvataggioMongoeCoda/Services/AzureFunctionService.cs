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
                            onehour = (float)Convert.ToDouble(mySqlDataReader["onehour"]),
                        };
                        myListBill.Add(myBill);
                    }
                    myBill = DateTime.UtcNow.DayOfWeek switch
                    {
                        DayOfWeek.Sunday => myListBill[0],
                        DayOfWeek.Monday => myListBill[1],
                        DayOfWeek.Tuesday => myListBill[2],
                        DayOfWeek.Wednesday => myListBill[3],
                        DayOfWeek.Thursday => myListBill[4],
                        DayOfWeek.Friday => myListBill[5],
                        DayOfWeek.Saturday => myListBill[6],
                        _ => null,
                    };
                    return myBill;
                }
            });
        }
    }
}
