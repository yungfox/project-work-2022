using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FunctionBillWeb
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 30 4 * * *")]TimerInfo myTimer, ILogger log)
        {

            double guadagnivoluti = 900;
            double guadagniprecedenti = 0;
            double billprecedente = 0;
            try
            {
                //calcolo il totale dei guadagni di 7 giorni fa
                using (SqlConnection con = new SqlConnection("Server=tcp:brunale.database.windows.net,1433;Initial Catalog=dbParking;Persist Security Info=False;User ID=nicola;Password=passwordsicura!1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                using (SqlCommand cmd = new SqlCommand()
                {
                    Connection = con,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "SELECT SUM(Bill) AS Totale FROM [dbo].[tblTicket] WHERE EntryTime BETWEEN dateadd(day,-7,CONVERT(varchar(10),GETDATE(),111)) AND dateadd(day,-6,CONVERT(varchar(10),GETDATE(),111))"
                })
                {
                    con.Open();
                    SqlDataReader mySqlDataReader = cmd.ExecuteReader();

                    if (mySqlDataReader.HasRows)
                    {
                        mySqlDataReader.Read();
                        guadagniprecedenti = (double)mySqlDataReader["Totale"];
                    }
                    con.Close();

                }
                //prendo il valore del costo orario di 7 giorni fa
                using (SqlConnection con = new SqlConnection("Server=tcp:brunale.database.windows.net,1433;Initial Catalog=dbParking;Persist Security Info=False;User ID=nicola;Password=passwordsicura!1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                using (SqlCommand cmd = new SqlCommand()
                {
                    Connection = con,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "SELECT onehour AS Bill FROM tblBilling WHERE day = DATENAME(WEEKDAY, GETDATE())"
                })
                {
                    con.Open();
                    SqlDataReader mySqlDataReader = cmd.ExecuteReader();

                    if (mySqlDataReader.HasRows)
                    {
                        mySqlDataReader.Read();
                        billprecedente = (double)mySqlDataReader["Bill"];
                    }
                    con.Close();

                }

                double billodierno = billprecedente * guadagnivoluti / guadagniprecedenti;

                if (billodierno > 2.5)
                {
                    billodierno = 2.5;
                }else if (billodierno < 0.5)
                {
                    billodierno = 0.5;
                }
                billodierno = Math.Round(billodierno, 2);

                //modifico il costo orario del giorno odierno
                using (SqlConnection con = new SqlConnection("Server=tcp:brunale.database.windows.net,1433;Initial Catalog=dbParking;Persist Security Info=False;User ID=nicola;Password=passwordsicura!1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                using (SqlCommand cmd = new SqlCommand()
                {
                    Connection = con,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "UPDATE tblBilling SET onehour = @bill WHERE day = DATENAME(WEEKDAY, GETDATE())"
                })
                {
                    cmd.Parameters.AddWithValue("bill", billodierno);

                    con.Open();

                    int result = cmd.ExecuteNonQuery();

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
            }

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
