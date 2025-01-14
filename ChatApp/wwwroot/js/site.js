// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
if (window.location.pathname === "/Chat") {
    const protocol = window.location.protocol === "https:" ? "wss://" : "ws://";
    const host = window.location.hostname;
    const port = window.location.port ? `:${window.location.port}` : "";
    const userId = document.getElementById("userId").dataset.userId;
    const socket = new WebSocket(`${protocol}${host}${port}/ws?userId=${userId}`);
    var receiverId;

    socket.onopen = () => console.log("WebSocket connection established.");
    socket.onclose = (event) => {
        console.log("WebSocket connection closed.", event);
        if (event.wasClean) {
            console.log(`Closed cleanly, code=${event.code}, reason=${event.reason}`);
        } else {
            console.error('Connection died');
        }
    };
    socket.onerror = (error) => console.error("WebSocket error observed:", error);
    socket.onmessage = function (event) {
        console.log("Received data:", event.data);
        const message = JSON.parse(event.data);

        if (message.type === "USER_LIST") {
            // Update active user list
            console.log("Received user list:", message.data);
            const userList = document.getElementById('userList');
            userList.innerHTML = ''; // Clear the list
            message.data.forEach(user => {
                const button = document.createElement('button');
                button.className = "list-group-item";
                button.innerText = `${user.Username} (${user.Avatar})`;

                // Add an onclick event to the button
                button.onclick = () => openUserChat(user.Id);
                userList.appendChild(button);
            });
        } else if (message.type === "MESSAGE") {
            // Handle incoming message
            console.log("Received message:", message.data);
            const li = document.createElement("li");
            li.textContent = message.data.User.Username + ":" + message.data.Message;
            document.getElementById("messagesList").appendChild(li);
        }
    };

    // Function to send a message
    function sendMessage() {
        const messageBody = document.getElementById("messageInput").value;
        const chatMessage = {
            SenderId: userId,
            ReceiverId: receiverId,
            MessageBody: messageBody
        };
        var fullMessage = { Action: "Message", Payload: JSON.stringify(chatMessage) }
        if (socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify(fullMessage));
            console.log("Message sent:", fullMessage);
        } else {
            console.error("WebSocket is not open. ReadyState:", socket.readyState);
        }
    }

    // Example usage: sendMessage('receiverId', 'Hello, World!');

    function openUserChat(id) {
        receiverId = id.toString();
    }

    window.addEventListener('beforeunload', function () {
        // Close the WebSocket when the user is leaving the page
        if (socket.readyState === WebSocket.OPEN) {
            socket.close();
        }
    });
}