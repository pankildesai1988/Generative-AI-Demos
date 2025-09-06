import { appendMessage, showError } from "./utils.js";

let currentSessionId = null;

export function getCurrentSession() {
    return currentSessionId;
}

export async function loadSessions(API_BASE) {
    const res = await $.get(API_BASE + "/api/chat/sessions");
    if (!res.success) return;

    $("#sessionsList").empty();

    res.data.forEach(session => {
        const isActive = (session.sessionId === currentSessionId) ? "active-session" : "";
        $("#sessionsList").append(`
          <li class="list-group-item d-flex justify-content-between align-items-center ${isActive}">
            <span style="cursor:pointer;" onclick="loadSession(${session.sessionId})">
              ${session.title} <small class="text-muted">(${session.model})</small>
            </span>
            <button class="btn btn-sm btn-outline-primary" onclick="duplicateSession(${session.sessionId}, 'gpt-4o')">Clone GPT-4o</button>
          </li>
        `);
    });
}

export async function loadSession(API_BASE, sessionId) {
    const res = await $.get(API_BASE + "/api/chat/history/" + sessionId);
    if (!res.success) return;

    currentSessionId = res.data.sessionId;
    $("#chatWindow").empty();
    res.data.messages.forEach(msg => appendMessage(msg.role, msg.content));
}

export async function duplicateSession(API_BASE, sessionId, model) {
    const res = await $.post(API_BASE + `/api/chat/duplicate-session?sessionId=${sessionId}&newModel=${model}`);
    if (!res.success) return showError(res.error);
    loadSessions(API_BASE);
}
