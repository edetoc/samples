var i = 0;

function timedCount() {    

    i = i + 1;
    postMessage(i);
    setTimeout("timedCount()", 500);
}

function onMessage(event) {             // handler code
    console.log('message', event.data);
}

self.addEventListener('message', onMessage, false);  // hook up an event handler for received messages
timedCount();
