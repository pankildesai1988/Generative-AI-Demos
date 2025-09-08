import { sendMessage, sendMessageStream, getCurrentSessionId, setCurrentSession } from "./chat.js";
import { loadSessions, loadSession, duplicateSession } from "./sessions.js";
import { loadTemplates, buildPromptPreview } from "./templates.js";
import { showError } from "./utils.js";

const API_BASE = "http://localhost:5000"; // ✅ adjust for Azure deployment

$(document).ready(async function () {
    // Load sessions + templates on startup
    await loadSessions(API_BASE);
    await loadTemplates(API_BASE);

    // Preview updates when template/params change
    $("#templateSelector").on("change", buildPromptPreview);
    $(document).on("input", ".template-param", buildPromptPreview);

    // Send message (normal / stream toggle)
    $("#btnSend").on("click", function () {
        if ($("#sendModeToggle").is(":checked")) {
            sendMessageStream(API_BASE);
        } else {
            sendMessage(API_BASE);
        }
    });

    // Enter key to send
    $("#userInput").on("keypress", function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $("#btnSend").click();
        }
    });

    // New session
    $("#btnNewSession").on("click", async function () {
        try {
            const model = $("#modelSelector").val();
            const res = await $.post(API_BASE + `/api/chat/new?model=${model}`);
            if (!res.success) return showError(res.error);

            setCurrentSession(res.data.sessionId);
            await loadSessions(API_BASE);
            await loadSession(API_BASE, res.data.sessionId);
        } catch (err) {
            showError("Failed to create new session.");
        }
    });

    // Clear all sessions
    $("#btnClearSessions").on("click", async function () {
        try {
            await $.ajax({
                url: API_BASE + "/api/chat/sessions",
                type: "DELETE"
            });
            setCurrentSession(null);
            await loadSessions(API_BASE);
            $("#chatWindow").empty();
        } catch (err) {
            showError("Failed to clear sessions.");
        }
    });

    // Session click → load history
    $(document).on("click", ".session-title", function () {
        const sessionId = $(this).closest("li").data("session-id");
        if (!sessionId) return showError("Invalid session.");
        setCurrentSession(sessionId);
        loadSession(API_BASE, sessionId);
    });

    // Clone session
    $(document).on("click", ".clone-session", function () {
        const sessionId = $(this).data("session-id");
        const model = $(this).data("model");
        if (!sessionId) return showError("Invalid session.");
        duplicateSession(API_BASE, sessionId, model);
    });

    // Clone current session (from dropdown)
    $("#btnCloneCurrent").on("click", function () {
        const sessionId = getCurrentSessionId();
        const model = $("#cloneModel").val();
        if (!sessionId) return showError("No active session to clone.");
        duplicateSession(API_BASE, sessionId, model);
    });
});
