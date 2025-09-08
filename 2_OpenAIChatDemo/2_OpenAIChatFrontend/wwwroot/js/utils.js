function getCurrentTime() {
    return new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

export function appendMessage(role, content) {
    const chatWindow = $("#chatWindow");
    const timestamp = getCurrentTime();

    if (role === "user") {
        chatWindow.append(`
            <div class="d-flex justify-content-end mb-2">
                <div>
                    <div class="p-2 rounded bg-primary text-white" style="max-width: 70%;">${content}</div>
                    <div class="text-end text-muted small">${timestamp}</div>
                </div>
            </div>
        `);
    }
    else if (role === "assistant") {
        chatWindow.append(`
            <div class="d-flex justify-content-start mb-2">
                <div>
                    <div class="p-2 rounded bg-light border" style="max-width: 70%;">${content}</div>
                    <div class="text-start text-muted small">${timestamp}</div>
                </div>
            </div>
        `);
    }
    else if (role === "system") {
        chatWindow.append(`
            <div class="d-flex justify-content-center mb-2">
                <div class="p-2 rounded bg-warning text-dark fw-bold" style="max-width: 80%;">⚠️ ${content}</div>
            </div>
            <div class="d-flex justify-content-center text-muted small">${timestamp}</div>
        `);
    }

    chatWindow.scrollTop(chatWindow[0].scrollHeight);
}

export function showError(msg) {
    appendMessage("system", msg);
}

// ✅ Typing bubble
export function showTyping() {
    const chatWindow = $("#chatWindow");

    if (!chatWindow.find(".typing-bubble").length) {
        chatWindow.append(`
            <div class="d-flex justify-content-start mb-2 typing-bubble">
                <div class="p-2 rounded bg-light border" style="max-width: 70%;">
                    <span class="typing-dots">
                        <span>.</span><span>.</span><span>.</span>
                    </span>
                </div>
            </div>
        `);

        chatWindow.scrollTop(chatWindow[0].scrollHeight);
    }
}

export function hideTyping() {
    $("#chatWindow").find(".typing-bubble").remove();
}
