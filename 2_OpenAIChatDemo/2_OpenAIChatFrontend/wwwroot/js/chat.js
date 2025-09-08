import { appendMessage, showError, showTyping, hideTyping } from "./utils.js";
import { loadSessions } from "./sessions.js";
import { buildPrompt } from "./templates.js";

let currentSessionId = null;

export function setCurrentSession(sessionId) {
    currentSessionId = sessionId;
}

export function getCurrentSessionId() {
    return currentSessionId;
}
    

export async function sendMessage(API_BASE) {
    const userMessage = $("#userInput").val();
    if (!userMessage) return;

    const model = $("#modelSelector").val();
    const formattedMessage = buildPrompt(userMessage);

    const messageObj = {
        sessionId: getCurrentSessionId() || 0,
        model: model,
        messages: [{ role: "user", content: formattedMessage }]
    };

    appendMessage("user", userMessage);
    showTyping(); // ✅ typing dots

    const res = await $.ajax({
        url: API_BASE + "/api/chat/send",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(messageObj)
    });

    hideTyping(); // ✅ remove dots

    if (!res.success) return showError(res.error);

    if (res.sessionId) {
        setCurrentSession(res.sessionId);
    }

    appendMessage("assistant", res.data);
    loadSessions(API_BASE);
}

export async function sendMessageStream(API_BASE) {
    const userMessage = $("#userInput").val();
    if (!userMessage) return;

    const model = $("#modelSelector").val();
    const formattedMessage = buildPrompt(userMessage);

    const messageObj = {
        sessionId: getCurrentSessionId() || 0,
        model: model,
        messages: [{ role: "user", content: formattedMessage }]
    };

    appendMessage("user", userMessage);
    showTyping(); // ✅ typing dots

    const response = await fetch(API_BASE + "/api/chat/stream", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(messageObj)
    });

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let assistantMessage = "";
    let firstChunk = true;

    while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        const lines = chunk.split("\n\n");

        for (const line of lines) {
            if (!line.trim() || !line.startsWith("data:")) continue;

            if (line.includes("[DONE]")) {
                $("#chatWindow").find(".assistant-streaming").removeClass("assistant-streaming");
                loadSessions(API_BASE);
                return;
            }

            try {
                const data = JSON.parse(line.replace("data:", "").trim());

                if (data.sessionId) {
                    setCurrentSession(data.sessionId);
                }

                if (data.text) {
                    if (firstChunk) {
                        hideTyping(); // ✅ remove dots once response starts
                        firstChunk = false;

                        // Create streaming bubble
                        if (!$("#chatWindow").find(".assistant-streaming").length) {
                            $("#chatWindow").append(`
                                <div class="d-flex justify-content-start mb-2 assistant-streaming">
                                    <div>
                                        <div class="p-2 rounded bg-light border" style="max-width: 70%;"></div>
                                        <div class="text-start text-muted small">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
                                    </div>
                                </div>
                            `);
                        }
                    }

                    assistantMessage += data.text;
                    $("#chatWindow").find(".assistant-streaming div div").first().text(assistantMessage);
                }
            } catch (err) {
                console.error("Streaming parse error:", err);
            }
        }
    }
}
