require('dotenv').config({path: __dirname + '/.env'})

const express = require('express')
const app = express()
const cors = require('cors')
const router = require('./src/router')
const iothub = require('./src/iothub')
const websocket = require('ws')

const wss = new websocket.Server({ noServer: true })

wss.on('connection', async socket => {
    socket.send('connected to server')
    console.log('client connected')
    //qui dovrebbe essere implementata la logica di connessione ad iothub...
})

app.use(cors())
app.use(router)

const server = app.listen(3000, () => console.log('app listening on port 3000'))

server.on('upgrade', (req, socket, head) => {
    wss.handleUpgrade(req, socket, head, connsocket => {
        wss.emit('connection', connsocket, req)
    })
})