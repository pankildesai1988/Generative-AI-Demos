export function appendMessage(role, content) {
    const chatWindow = $("#chatWindow");
    const msgClass = role === "user" ? "text-end text-primary" : "text-start text-success";
    chatWindow.append(`<div class="${msgClass}"><strong>${role}:</strong> ${content}</div>`);
    chatWindow.scrollTop(chatWindow[0].scrollHeight);
}

export function showError(msg) {
    alert("⚠️ Error: " + msg);
}
