import { appendMessage, showError } from "./utils.js";
import { getCurrentSession, loadSessions } from "./sessions.js";
import { buildPrompt } from "./templates.js";

export async function sendMessage(API_BASE) {
    const userMessage = $("#userInput").val();
    if (!userMessage) return;

    const model = $("#modelSelector").val();
    const formattedMessage = buildPrompt(userMessage);

    const messageObj = {
        sessionId: getCurrentSession() || 0,
        model: model,
        messages: [{ role: "user", content: formattedMessage }]
    };

    const res = await $.ajax({
        url: API_BASE + "/api/chat/send",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(messageObj)
    });

    if (!res.success) return showError(res.error);

    appendMessage("user", userMessage);
    appendMessage("assistant", res.data);
    loadSessions(API_BASE);
}

export async function sendMessageStream(API_BASE) {
    const userMessage = $("#userInput").val();
    if (!userMessage) return;

    const model = $("#modelSelector").val();
    const formattedMessage = buildPrompt(userMessage);

    const messageObj = {
        sessionId: getCurrentSession() || 0,
        model: model,
        messages: [{ role: "user", content: formattedMessage }]
    };

    appendMessage("user", userMessage);

    const response = await fetch(API_BASE + "/api/chat/stream", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(messageObj)
    });

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let assistantMessage = "";

    while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        const lines = chunk.split("\n\n");

        for (const line of lines) {
            if (!line.trim() || !line.startsWith("data:")) continue;

            if (line.includes("[DONE]")) {
                appendMessage("assistant", assistantMessage);
                loadSessions(API_BASE);
                return;
            }

            try {
                const data = JSON.parse(line.replace("data:", "").trim());
                if (data.text) {
                    assistantMessage += data.text;
                    $("#chatWindow").find(".assistant-streaming").last().remove();
                    $("#chatWindow").append(`<div class="text-start text-success assistant-streaming"><strong>assistant:</strong> ${assistantMessage}</div>`);
                }
            } catch { }
        }
    }
}
