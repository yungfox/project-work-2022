const connectionString = process.env.IotHubConnectionString;
const clientFromConnectionString = require('azure-iot-device-amqp').clientFromConnectionString;
const client = clientFromConnectionString(connectionString);

const onConnect = err => {
    console.log('trying to connect to iothub...')
    if(err) {
        console.log(`could not connect to iothub :(\n${err}`)
    } else {
        console.log('connected to iothub!')
        client.on('message', (input, message) => {
            client.complete(message)
            let body = message.getBytes.toString('ascii')

            console.log(`new message: ${body}`)
        })
    }
}

module.exports.connect = () => client.open(onConnect)