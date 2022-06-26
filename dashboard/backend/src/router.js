const express = require('express')
const sql = require('mssql')
const router = express.Router()
const sqlConfig = require('./sqlserver')

/* api che viene chiamata dal frontend per popolare i dati al load.
 * restituisce:
 * - lo stato di occupazione delle piazzole del piano terra
 * - lo stato di occupazione delle piazzole del primo piano
 * - tempo di sosta medio
 * - posti liberi totali
 * - tariffa oraria attuale
 * - ingressi totali giornalieri e permanenza media giornaliera degli ultimi 7 giorni
 * - ingressi totali giornalieri e permanenza media giornaliera tra 7 e 14 giorni fa
 */
router.get('/getData', (req, res) => {
    sql.connect(sqlConfig, err => {
        try {
            let request = new sql.Request()
            request.query("SELECT Id, Status FROM tblParkingSpot WHERE IdFloor = 0;" +
                "SELECT Id, Status FROM tblParkingSpot WHERE IdFloor = 1;" +
                "SELECT AVG(DATEDIFF(MINUTE,EntryTime,ExitTime)) AS AvgParkingTime FROM tblTicket;" +
                "SELECT COUNT(Id) AS FreeSpotsCount FROM tblParkingSpot WHERE Status = 1;" +
                "SELECT " +
                "CONVERT(DATE, CONCAT(datepart(year, EntryTime),'-',datepart(MONTH, EntryTime),'-',datepart(day, EntryTime))) AS Date," +
                "AVG(DATEDIFF(MINUTE,EntryTime,ExitTime)) AS AvgParkingTime," +
                "COUNT(IdTicket) AS TotalEntries " +
                "FROM tblTicket WHERE EntryTime BETWEEN (dateadd(day,-7,CONVERT(varchar(10),GETDATE(),111))) AND CONVERT(varchar(10),GETDATE(),111) " +
                "GROUP BY datepart(day, EntryTime),datepart(MONTH, EntryTime),datepart(YEAR, EntryTime);" +
                "SELECT onehour AS CurrentRate FROM tblBilling WHERE day = DATENAME(WEEKDAY, GETDATE());" +
                "SELECT " +
                "CONVERT(DATE, CONCAT(datepart(year, EntryTime),'-',datepart(MONTH, EntryTime),'-',datepart(day, EntryTime))) AS Date," +
                "AVG(DATEDIFF(MINUTE,EntryTime,ExitTime)) AS AvgParkingTime," +
                "COUNT(IdTicket) AS TotalEntries " +
                "FROM tblTicket " +
                "WHERE EntryTime BETWEEN (dateadd(day,-14,CONVERT(varchar(10),GETDATE(),111))) AND (dateadd(day,-7,CONVERT(varchar(10),GETDATE(),111))) " +
                "GROUP BY datepart(day, EntryTime),datepart(MONTH, EntryTime),datepart(YEAR, EntryTime);", (err, recordset) => {
                    res.end(JSON.stringify(recordset.recordsets))
                })
            request.query()
        } catch (err) {
            console.log(err)
            res.sendStatus(500)
            return
        }
    })
})

module.exports = router