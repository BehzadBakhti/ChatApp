// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
if (window.location.pathname === "/Chat") {
    const protocol = window.location.protocol === "https:" ? "wss://" : "ws://";
    const host = window.location.hostname;
    const port = window.location.port ? `:${window.location.port}` : "";
    const socket = new WebSocket(`${protocol}${host}${port}/ws`);

    socket.onopen = () => console.log("WebSocket connection established.");
    socket.onclose = () => console.log("WebSocket connection closed.");
    socket.onmessage = function (event) {
        const li = document.createElement("li");
        li.textContent = event.data;
        document.getElementById("messagesList").appendChild(li);
    };

    function sendMessage() {
        const message = document.getElementById("messageInput").value;

        socket.send(message);

    }
    window.addEventListener('beforeunload', function () {
        // Close the WebSocket when the user is leaving the page
        if (socket.readyState === WebSocket.OPEN) {
            socket.close();
        }
    });
}