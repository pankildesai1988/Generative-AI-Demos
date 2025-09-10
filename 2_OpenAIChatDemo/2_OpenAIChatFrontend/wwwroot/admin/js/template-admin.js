// ✅ now API_BASE_URL comes from Razor
const apiUrl = API_BASE_URL + "/api/PromptTemplate";

// 🔑 Ensure AJAX includes authentication cookies
// Always attach JWT token if available
$.ajaxSetup({
    beforeSend: function (xhr) {
        if (!ensureAuthenticated()) return false; // redirect if missing/expired
        let token = getJwtToken();
        xhr.setRequestHeader("Authorization", "Bearer " + token);
    },
    statusCode: {
        401: function () {
            // Unauthorized → force login
            localStorage.removeItem("jwtToken");
            window.location.href = "/Account/Login";
        }
    }
});

function loadTemplates() {
    $('#templatesTable').DataTable({
        ajax: { url: apiUrl, dataSrc: "" },
        columns: [
            { data: "name" },
            { data: "keyName" },
            { data: "version", render: v => "v" + v },
            { data: "updatedAt", defaultContent: "" },
            { data: "isActive", render: a => a ? "Active" : "Inactive" },
            {
                data: "id",
                render: id => `
                    <a href="/Admin/Templates/Edit/${id}" class="btn btn-sm btn-primary"><i class="fas fa-edit"></i></a>
                    <button onclick="deleteTemplate(${id})" class="btn btn-sm btn-danger"><i class="fas fa-trash"></i></button>
                    <button onclick="showHistory(${id})" class="btn btn-sm btn-info"><i class="fas fa-history"></i></button>`
            }
        ]
    });
}

function loadTemplate(id) {
    $.get(`${apiUrl}/${id}`, function (data) {
        $("[name='Name']").val(data.name);
        $("[name='KeyName']").val(data.keyName);
        $("[name='TemplateText']").val(data.templateText);
        $("#paramsContainer").empty();
        data.parameters.forEach(p => addParam(p));
        updatePreview();
    });
}

function createTemplate() {
    let dto = collectTemplateData();
    $.ajax({
        url: apiUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(dto),
        success: () => Swal.fire({ title: "Created", text: "Template saved!", icon: "success" })
            .then(() => window.location.href = "/Admin/Templates"),
        error: () => showToast("Failed to create template", "error")
    });
}

function updateTemplate(id) {
    let dto = collectTemplateData();
    $.ajax({
        url: `${apiUrl}/${id}`,
        type: "PUT",
        contentType: "application/json",
        data: JSON.stringify(dto),
        success: () => Swal.fire({ title: "Updated", text: "Template updated!", icon: "success" })
            .then(() => window.location.href = "/Admin/Templates"),
        error: () => showToast("Failed to update template", "error")
    });
}

function deleteTemplate(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "This will deactivate the template!",
        icon: "warning",
        showCancelButton: true
    }).then((res) => {
        if (res.isConfirmed) {
            $.ajax({
                url: `${apiUrl}/${id}`,
                type: "DELETE",
                success: () => $('#templatesTable').DataTable().ajax.reload(null, false) & showToast("Template deleted"),
                error: () => showToast("Failed to delete template", "error")
            });
        }
    });
}

function showHistory(templateId) {
    $.get(`${apiUrl}/${templateId}/versions`, function (data) {
        let rows = data.map(v => {
            let date = v.createdAt || v.updatedAt;
            if (date) {
                date = new Date(date).toLocaleString(); // ✅ human readable
            } else {
                date = "N/A";
            }

            return `
            <tr>
                <td>v${v.version}</td>
                <td>${v.name}</td>
                <td>${date}</td>
                <td>
                    <button class="btn btn-sm btn-secondary" onclick="rollbackVersion(${templateId}, ${v.version})">
                        <i class="fas fa-undo"></i> Rollback
                    </button>
                </td>
            </tr>
        `}).join("");
        $("#historyTable tbody").html(rows);
        $("#historyModal").modal("show");
    });
}

function rollbackVersion(templateId, version) {
    Swal.fire({
        title: "Rollback Confirmation",
        text: `Rollback to version ${version}?`,
        icon: "question",
        showCancelButton: true
    }).then((res) => {
        if (res.isConfirmed) {
            $.post(`${apiUrl}/${templateId}/rollback/${version}`, function () {
                $("#historyModal").modal("hide");
                $('#templatesTable').DataTable().ajax.reload(null, false);
                showToast(`Rolled back to v${version}`);
            }).fail(() => showToast("Rollback failed", "error"));
        }
    });
}

function addParam(param = {}) {
    let options = param.options ? param.options.split(",") : [];
    let defaultField = "";

    if (options.length > 0) {
        defaultField = `<select class="form-select w-25 param-default">` +
            options.map(opt => `<option value="${opt}" ${opt === param.defaultValue ? "selected" : ""}>${opt}</option>`).join("") +
            `</select>`;
    } else {
        defaultField = `<input type="text" class="form-control w-25 param-default" placeholder="Default" value="${param.defaultValue || ""}" />`;
    }

    $("#paramsContainer").append(`
        <div class="param-item mb-2 d-flex gap-2">
            <input type="text" class="form-control w-25 param-name" placeholder="Name" value="${param.name || ""}" />
            <input type="text" class="form-control w-25 param-key" placeholder="KeyName" value="${param.keyName || ""}" />
            <input type="text" class="form-control w-25 param-options" placeholder="Options (CSV)" value="${param.options || ""}" />
            ${defaultField}
            <button type="button" class="btn btn-sm btn-danger" onclick="$(this).parent().remove(); updatePreview();">❌</button>
        </div>
    `);

    $("#paramsContainer .param-item:last input, #paramsContainer .param-item:last select").on("input change", updatePreview);
    updatePreview();
}

function collectTemplateData() {
    let dto = collectTemplateDataForPreview();

    if (!dto.name || !dto.keyName || !dto.templateText) {
        Swal.fire({
            title: "Validation Error",
            text: "Name, KeyName, and TemplateText are required.",
            icon: "error"
        });
        throw new Error("Validation failed");
    }

    return dto;
}


function collectTemplateDataForPreview() {
    let name = $("[name='Name']").val()?.trim() || "";
    let keyName = $("[name='KeyName']").val()?.trim() || "";
    let templateText = $("[name='TemplateText']").val()?.trim() || "";

    let params = [];
    $("#paramsContainer .param-item").each(function () {
        params.push({
            name: $(this).find(".param-name").val(),
            keyName: $(this).find(".param-key").val(),
            options: $(this).find(".param-options").val(),
            defaultValue: $(this).find(".param-default").val()
        });
    });

    return { name, keyName, templateText, parameters: params };
}

function updatePreview() {
    try {
        let dto = collectTemplateDataForPreview(); // ✅ safe, no alerts
        if (!dto.templateText) return;

        // Replace placeholders
        let preview = dto.templateText;
        dto.parameters.forEach(p => {
            let value = p.defaultValue || `[${p.keyName}]`;
            preview = preview.replaceAll("{" + p.keyName + "}", value);
        });

        $("#previewBox").text(preview);
    } catch (err) {
        console.warn("Preview skipped:", err.message);
    }
}


function previewTemplate(id) {
    let dto = collectTemplateData();

    let paramValues = {};
    dto.parameters.forEach(p => {
        paramValues[p.keyName] = p.defaultValue || "";
    });

    // Only call API if template text is filled
    if (!dto.templateText || dto.templateText.trim() === "") {
        $("#previewBox").text("Template text is empty...");
        return;
    }

    $.ajax({
        url: `${apiUrl}/0/preview`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            templateText: dto.templateText,
            parameters: paramValues
        }),
        success: function (res) {
            $("#previewBox").text(res.renderedPrompt);
        },
        error: function () {
            showToast("Failed to preview template", "error");
        }
    });
}

$(document).on("input", "[name='Name'], [name='KeyName'], [name='TemplateText'], #paramsContainer input", updatePreview);
