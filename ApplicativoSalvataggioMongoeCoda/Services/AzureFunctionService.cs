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
        public Task<Prezzi> GetUpdatedBillings()
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
                    List<Prezzi> myListPrezzi = new List<Prezzi>();
                    Prezzi myPrezzi;
                    con.Open();
                    SqlDataReader mySqlDataReader = cmd.ExecuteReader();
                    while (mySqlDataReader.Read())//finchè c'è qualcosa da leggere, il metodo Read() restituisce true se trova una riga
                    {
                        myPrezzi = new Prezzi()
                        {
                            mezzora =(float)Convert.ToDouble(mySqlDataReader["mezzora"]),
                            unora = (float)Convert.ToDouble(mySqlDataReader["unora"]),
                            treore = (float)Convert.ToDouble(mySqlDataReader["treore"]),
                            seiore = (float)Convert.ToDouble(mySqlDataReader["seiore"]),
                            giornaliero = (float)Convert.ToDouble(mySqlDataReader["giornaliero"]),
                        };
                        myListPrezzi.Add(myPrezzi);
                    }
                    return myListPrezzi[0];
                }
            });
        }
    }
}
