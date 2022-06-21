"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/parkingHub").build();

connection.on("ReceiveMessage", function (user, message) {
    //callback
});

connection.start().then(function () {
    console.log('connesso a signalr')
}).catch(function (err) {
    return console.error(err.toString());
});

// document.getElementById("sendButton").addEventListener("click", function (event) {
//     var user = document.getElementById("userInput").value;
//     var message = document.getElementById("messageInput").value;
//     connection.invoke("SendMessage", user, message).catch(function (err) {
//         return console.error(err.toString());
//     });
//     event.preventDefault();
// });