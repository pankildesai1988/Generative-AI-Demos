import { appendMessage, showError } from "./utils.js";
import { getCurrentSessionId, setCurrentSession } from "./chat.js";

export async function loadSessions(API_BASE) {
    const res = await $.get(API_BASE + "/api/chat/sessions");
    if (!res.success) return;

    $("#sessionsList").empty();

    res.data.forEach(session => {
        const isActive = (session.sessionId === getCurrentSessionId()) ? "active-session" : "";

        let timestamp = "No messages yet";
        if (session.lastMessageAt || session.createdAt) {
            const dt = new Date(session.lastMessageAt || session.createdAt);
            timestamp = isNaN(dt.getTime()) ? "Invalid Date" : dt.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        $("#sessionsList").append(`
          <li class="list-group-item d-flex flex-column ${isActive}" 
              data-session-id="${session.sessionId}">
            
            <div class="d-flex justify-content-between align-items-center">
              <span class="session-title" style="cursor:pointer;">
                ${session.title} <small class="text-muted">(${session.model})</small>
              </span>
              <button class="btn btn-sm btn-outline-primary clone-session" 
                      data-session-id="${session.sessionId}" data-model="gpt-4o">
                Clone GPT-4o
              </button>
            </div>

            <div class="text-muted small mt-1">${timestamp}</div>
          </li>
        `);
    });
}

export async function loadSession(API_BASE, sessionId) {
    const res = await $.get(API_BASE + "/api/chat/history/" + sessionId);
    if (!res.success) return;

    setCurrentSession(sessionId);  // ✅ update sessionId globally
    $("#chatWindow").empty();
    console.log(res.data);
    res.data.messages.forEach(msg => appendMessage(msg.role, msg.content));
}

export async function duplicateSession(API_BASE, sessionId, model) {
    const res = await $.post(API_BASE + `/api/chat/duplicate-session?sessionId=${sessionId}&newModel=${model}`);
    if (!res.success) return showError(res.error);

    loadSessions(API_BASE);

    if (res.data && res.data.sessionId) {
        setCurrentSession(res.data.sessionId);
        loadSession(API_BASE, res.data.sessionId);
    }
}
    