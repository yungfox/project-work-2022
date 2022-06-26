// carico le variabili salvate in nel file .env
require('dotenv').config({ path: __dirname + '/.env' })

// importo i moduli necessari
const path = require('path')
const express = require('express')
const app = express()
const cors = require('cors')
const router = require('./src/router')
const websocket = require('ws')

// istanzio il client per la connessione con azure iothub
const connectionString = process.env.AZURE_CONNECTION_STRING
const device = require('azure-iot-device').Client
const protocol = require('azure-iot-device-amqp').Amqp

const client = device.fromConnectionString(connectionString, protocol)

// istanzio il server websocket a cui si connetterà il frontend
const wss = new websocket.Server({ noServer: true })

// all'apertura del websocket con il frontend, apro la connessione con iothub
wss.on('connection', async socket => {
    console.log('client connected')

    try {
        client.on('message', message => {
            // ottengo il messaggio proveniente da iothub
            let body = message.getBytes().toString('ascii')

            console.log(`new message: ${body}`)

            // "inoltro" il messaggio al frontend via websockets
            socket.send(body)
                // eseguo l'ack del messaggio
            client.complete(message)
        })

        client.on('disconnect', () => {
            client.removeAllListeners()
            console.log('client disconnected')
        })

        client.open()
            .then(() => console.log('connected to iothub!'))
            .catch(() => client.close())
    } catch (err) {
        console.log(err)
    }
})

app.use(cors())

// uso gli endpoint definiti in src/router.js
app.use(router)

// servo i file statici del frontend sull'endpoint localhost:3000/
app.use(express.static(path.join(__dirname, '../frontend/dist')))

app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, '../frontend/dist/index.html'))
})

const server = app.listen(3000, () => console.log('app listening on port 3000'))

// eseguo l'upgrade del protocollo all'arrivo di una richiesta websockets
server.on('upgrade', (req, socket, head) => {
    wss.handleUpgrade(req, socket, head, connsocket => {
        wss.emit('connection', connsocket, req)
    })
})