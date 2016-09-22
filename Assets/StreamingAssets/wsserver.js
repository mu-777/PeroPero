var ws = require('ws').Server;

var wss = new ws({
    host: process.argv[2],
    port: process.argv[3]
});

console.log("ws://" + process.argv[2] + ":" + process.argv[3]);

wss.broadcast = function (data) {
    for (var i in this.clients) {
        this.clients[i].send(data);
    }
};
wss.on('open', function () {
    ws.send('Hello WebSocket');
});

wss.on('connection', function (ws) {
    ws.on('message', function (message) {
        var now = new Date();
        console.log('Received: %s', message);
        wss.broadcast(message);
    });
});