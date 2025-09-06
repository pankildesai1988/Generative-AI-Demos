import { loadTemplates, buildPrompt } from "./templates.js";
import { loadSessions, duplicateSession, getCurrentSession } from "./sessions.js";
import { sendMessage, sendMessageStream } from "./chat.js";

//const API_BASE = "https://openaidemo-backend-gehmhsgyf2gzgseq.centralus-01.azurewebsites.net";
const API_BASE = "http://localhost:5000";

$(document).ready(() => {
    loadTemplates(API_BASE);
    loadSessions(API_BASE);

    // Send button
    $("#btnSend").on("click", () => {
        if ($("#sendModeToggle").is(":checked")) {
            sendMessageStream(API_BASE);
        } else {
            sendMessage(API_BASE);
        }
    });

    // Clone session
    $("#btnCloneCurrent").on("click", () => {
        const model = $("#cloneModel").val();
        const currentSessionId = getCurrentSession();
        if (!currentSessionId) {
            alert("No active session selected");
            return;
        }
        duplicateSession(API_BASE, currentSessionId, model);
    });

    // Create new session
    $("#btnNewSession").on("click", async () => {
        await $.post(API_BASE + "/api/chat/new?model=gpt-3.5-turbo");
        loadSessions(API_BASE);
    });

    // Clear all sessions
    $("#btnClearSessions").on("click", async () => {
        await $.ajax({
            url: API_BASE + "/api/chat/sessions",
            type: "DELETE"
        });
        loadSessions(API_BASE);
        $("#chatWindow").empty();
    });

    // Auto-update prompt preview
    $("#templateSelector, #userInput, [id^=param_]").on("change input", () => {
        const userMessage = $("#userInput").val();
        $("#promptPreview").val(buildPrompt(userMessage));
    });
});
