import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr'

export default {
    initSignalR: async () => {
        let hub = new HubConnectionBuilder()
                    .withUrl('/parkingHub', { 
                        skipNegotiation: true,
                        transport: HttpTransportType.WebSockets
                    })
                    .build()
        
        hub.on('ReceiveMessage', message => {
            console.log(`new message: ${message}`)
        })
        
        await hub.start()
            .then(() => console.log('connected to signalr'))
            .catch(err => console.error(err.toString()))
    }
}
