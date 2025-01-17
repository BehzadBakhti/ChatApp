// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
if (window.location.pathname === "/Chat") {
    const protocol = window.location.protocol === "https:" ? "wss://" : "ws://";
    const host = window.location.hostname;
    const port = window.location.port ? `:${window.location.port}` : "";
    const thisUserId = document.getElementById("thisUser").dataset.userId;
    const thisUsername = document.getElementById("thisUser").dataset.username;
    const socket = new WebSocket(`${protocol}${host}${port}/ws?userId=${thisUserId}`);
    const chatCache = new Map();
    var usersData;
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
        const socketMessage = JSON.parse(event.data);

        if (socketMessage.type === "USER_LIST") {
            // Update active user list
            console.log("Received user list:", socketMessage.data);
            updateUsersList(socketMessage);
        } else if (socketMessage.type === "MESSAGE") {
            // Handle incoming message
            console.log("Received message:", socketMessage.data);
            const li = document.createElement("li");
            li.textContent = socketMessage.data.User.Username + ":" + socketMessage.data.Message;
            document.getElementById("messagesList").appendChild(li);
        }
    };

    // Function to send a message
    function sendMessage() {
        const messageBox = document.getElementById("messageInput")

        const messageBody = messageBox.value;
        if (messageBody.trim() == "")
            return;

        const chatMessage = {
            SenderId: thisUserId,
            ReceiverId: receiverId,
            MessageBody: messageBody
        };
        var fullMessage = { Action: "Message", Payload: JSON.stringify(chatMessage) }
        if (socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify(fullMessage));
            messageBox.value = "";
            console.log("Message sent:", fullMessage);
        } else {
            console.error("WebSocket is not open. ReadyState:", socket.readyState);

        }
    }

    // Example usage: sendMessage('receiverId', 'Hello, World!');

    function openUserChat(user) {
        receiverId = user.Id.toString();
        const chatAreaInfo = document.getElementById("ChatAreaInfo")

        // Check if chats are already cached
        if (chatCache.has(user.Id)) {
            console.log("Loading chats from cache...");
            displayChats(user.Id, user.Username, chatCache.get(user.Id));
        } else {
            console.log("Fetching chats from server...");

            fetch(`/Chat/GetUserChats?userId=${user.Id}`)
                .then(response => response.json())
                .then(data => {
                    // Cache the chats
                    chatCache.set(user.Id, data);
                    // Display the chats
                    displayChats(user.Id, user.Username, data);
                    document.getElementById("sendMessageBtn").disabled = false;
                })
                .catch(error => {
                    chatAreaInfo.innerText = "Could not fetch the chats, please try again";
                    console.error("Error fetching chats:", error)
                });
        }
    }

    function displayChats(userId, username, chats) {
        const chatContainer = document.getElementById("messagesList");

        chatContainer.innerHTML = ""; // Clear previous chats

        chats.forEach(chat => {
            const senderName = userId == chat.senderId ? username : thisUsername;
            const isMe = chat.senderId == thisUserId ? true : false;
            addChatMessage(senderName, chat.messageBody, isMe);
        });
    }
    window.addEventListener('beforeunload', function () {
        // Close the WebSocket when the user is leaving the page
        if (socket.readyState === WebSocket.OPEN) {
            socket.close();
        }
    });

    function updateUsersList(socketMessage) {

        usersData = socketMessage.data;
        const userList = document.getElementById('userList');

        userList.innerHTML = ''; // Clear the list
        socketMessage.data.forEach(user => {
            console.log(user.Id + " ===" + thisUserId);
            if (user.Id == thisUserId)
                return;
            const button = document.createElement('button');
            button.className = "list-group-item";
            button.innerText = `${user.Username} (${user.Avatar})`;
            button.setAttribute("onclick", `openUserChat(${JSON.stringify(user)})`);

            userList.appendChild(button);
        });
    }
}


//
/**
 * Adds a chat message to the chat container using Bootstrap classes.
 * @param {string} username - The name of the user.
 * @param {string} message - The chat message.
 * @param {boolean} isMe - True if the sender is the current user.
 */
function addChatMessage(username, message, isMe) {
    // Create the message container
    const messageElement = document.createElement("div");
    messageElement.classList.add(
        "d-flex",
        isMe ? "justify-content-end" : "justify-content-start",
        "mb-2"
    );

    // Create the message card
    const messageCard = document.createElement("div");
    if (isMe) {

        messageCard.classList.add("card", "bg-success", "text-white");
    } else {
        messageCard.classList.add("card", "bg-light");

    }
    messageCard.style.maxWidth = "75%";

    // Add the username
    const usernameElement = document.createElement("div");
    usernameElement.classList.add("card-header", "p-2");
    usernameElement.textContent = username;

    // Add the message
    const messageContent = document.createElement("div");
    messageContent.classList.add("card-body", "p-2");
    messageContent.textContent = message;

    // Append username and message to the card
    messageCard.appendChild(usernameElement);
    messageCard.appendChild(messageContent);

    // Append the card to the message container
    messageElement.appendChild(messageCard);

    // Append the message container to the chat container
    const chatContainer = document.getElementById("messagesList");
    chatContainer.appendChild(messageElement);

    // Optionally scroll to the bottom
    chatContainer.scrollTop = chatContainer.scrollHeight;
}

