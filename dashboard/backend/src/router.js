const express = require('express')
const router = express.Router()
const sql = require('./sqlserver')

// qui vanno definite le API. dovrebbero interrogare il 
// database sql di azure e restituire i dati. verranno 
// poi chiamate da vue per mostrare i dati nella pagina

// questa è  solo un'api di test per vedere che funzioni
router.get('/testapi', ({ res }) => res.send('yep'))

module.exports = router