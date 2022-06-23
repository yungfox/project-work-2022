const express = require('express')
const sql = require('mssql')
const router = express.Router()
const sqlConfig = require('./sqlserver')

router.get('/testapi', ({ res }) => res.send('yep'))

router.get('/sqltest', (req, res) => {
    sql.connect(sqlConfig, err => {
        try {
            let request = new sql.Request()
            request.query('SELECT Id, Status FROM tblParkingSpot;SELECT AVG(DATEDIFF(MINUTE,EntryTime,ExitTime)) AS AVGtimestop FROM tblTicket;SELECT COUNT(Id) AS CountFreeSpot FROM tblParkingSpot WHERE Status = 1', (err, recordset) => {
                res.end(JSON.stringify(recordset.recordsets))
            })
            request.query()
        } 
        catch(err) {
            console.log(err)
            res.sendStatus(500)
            return
        }
    })
})

module.exports = router