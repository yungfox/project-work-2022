require('dotenv').config({path: __dirname + '/.env'})

const path = require('path')
const express = require('express')
const app = express()
const cors = require('cors')
const router = require('./src/router')
const websocket = require('ws')

const connectionString = process.env.YUNGTEST
//const connectionString = process.env.AZURE_CONNECTION_STRING
const device = require('azure-iot-device').Client
const protocol = require('azure-iot-device-amqp').Amqp

const client = device.fromConnectionString(connectionString, protocol)

const wss = new websocket.Server({ noServer: true })

wss.on('connection', async socket => {
    console.log('client connected')
    
    try {
        client.on('message', message => {
            let body = message.getBytes().toString('ascii')
            
            console.log(`new message: ${body}`)

            socket.send(body)
            client.complete(message)
        })

        client.on('disconnect', () => {
            client.removeAllListeners()
            console.log('client disconnected')
        })

        client.open()
        .then(() => console.log('connected to iothub!'))
        .catch(() => client.close())
    } catch(err) {
        console.log(err)
    }
})

app.use(cors())
app.use(router)
app.use(express.static(path.join(__dirname, '../frontend/dist')))

app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, '../frontend/dist/index.html'))
})

const server = app.listen(3000, () => console.log('app listening on port 3000'))

server.on('upgrade', (req, socket, head) => {
    wss.handleUpgrade(req, socket, head, connsocket => {
        wss.emit('connection', connsocket, req)
    })
})