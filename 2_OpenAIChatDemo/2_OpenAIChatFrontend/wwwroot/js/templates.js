let TEMPLATES = {};

export async function loadTemplates(API_BASE) {
    const res = await $.get(API_BASE + "/api/PromptTemplates");
    if (!res.success) return;

    $("#templateSelector").empty();
    TEMPLATES = {};

    res.data.forEach(t => {
        TEMPLATES[t.keyName] = t;
        $("#templateSelector").append(`<option value="${t.keyName}">${t.name}</option>`);
    });

    renderTemplateParameters($("#templateSelector").val());
}

export function renderTemplateParameters(templateKey) {
    const template = TEMPLATES[templateKey];
    const container = $("#templateParameters");
    container.empty();

    if (!template || !template.parameters) return;

    template.parameters.forEach(p => {
        const optionsHtml = p.options
            ? p.options.map(opt => `<option value="${opt}" ${opt === p.defaultValue ? "selected" : ""}>${opt}</option>`).join("")
            : "";

        container.append(`
          <div class="mb-2">
            <label for="param_${p.keyName}">${p.name}:</label>
            <select id="param_${p.keyName}" class="form-select">${optionsHtml}</select>
          </div>
        `);
    });
}

export function buildPrompt(userMessage) {
    const templateKey = $("#templateSelector").val();
    const template = TEMPLATES[templateKey];
    if (!template) return userMessage;

    let formattedMessage = template.templateText.replace("{input}", userMessage);

    if (template.parameters) {
        template.parameters.forEach(p => {
            const value = $("#param_" + p.keyName).val() || p.defaultValue;
            formattedMessage = formattedMessage.replace(`{${p.keyName}}`, value);
        });
    }

    return formattedMessage;
}
