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


function addParam(param = {}) {
    let type = param.type || "text";
    let required = param.isRequired ? "checked" : "";

    // Type selector
    let typeField = `
        <select class="form-select w-25 param-type">
            <option value="text" ${type === "text" ? "selected" : ""}>Text</option>
            <option value="number" ${type === "number" ? "selected" : ""}>Number</option>
            <option value="boolean" ${type === "boolean" ? "selected" : ""}>Boolean</option>
            <option value="select" ${type === "select" ? "selected" : ""}>Select</option>
            <option value="multiselect" ${type === "multiselect" ? "selected" : ""}>Multi-Select</option>
        </select>`;

    // Default value based on type
    let defaultField = `<input type="text" class="form-control w-25 param-default" placeholder="Default" value="${param.defaultValue || ""}" />`;
    if (type === "boolean") {
        defaultField = `<input type="checkbox" class="form-check-input param-default" ${param.defaultValue === "true" ? "checked" : ""} />`;
    }

    $("#paramsContainer").append(`
        <div class="param-item mb-2 d-flex gap-2 align-items-center">
            <input type="text" class="form-control w-25 param-name" placeholder="Name" value="${param.name || ""}" />
            <input type="text" class="form-control w-25 param-key" placeholder="KeyName" value="${param.keyName || ""}" />
            ${typeField}
            <input type="text" class="form-control w-25 param-options" placeholder="Options (CSV)" value="${param.options || ""}" />
            ${defaultField}
            <div class="form-check">
                <input type="checkbox" class="form-check-input param-required" ${required} /> Required
            </div>
            <input type="text" class="form-control w-25 param-regex" placeholder="Regex (optional)" value="${param.regexPattern || ""}" />
            <button type="button" class="btn btn-sm btn-danger" onclick="$(this).parent().remove(); updatePreview();">❌</button>
        </div>
    `);

    $("#paramsContainer .param-item:last input, #paramsContainer .param-item:last select")
        .on("input change", updatePreview);

    updatePreview();
}

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

    if (!dto.keyName.match(/^[a-zA-Z0-9_]+$/)) {
        Swal.fire({ title: "Validation Error", text: "KeyName must be alphanumeric and unique.", icon: "error" });
        return;
    }


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



function collectTemplateData() {
    let dto = collectTemplateDataForPreview();

    if (!dto.name || !dto.keyName || !dto.templateText) {
        Swal.fire({ title: "Validation Error", text: "Name, KeyName, and TemplateText are required.", icon: "error" });
        throw new Error("Validation failed");
    }

    // Validate parameters
    dto.parameters.forEach(p => {
        if (p.isRequired && !p.defaultValue) {
            throw new Error(`Parameter '${p.name}' is required.`);
        }
        if (p.regexPattern) {
            try {
                let regex = new RegExp(p.regexPattern);
                if (!regex.test(p.defaultValue || "")) {
                    throw new Error(`Parameter '${p.name}' does not match regex ${p.regexPattern}`);
                }
            } catch {
                throw new Error(`Invalid regex for parameter '${p.name}'`);
            }
        }
    });

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
            type: $(this).find(".param-type").val(),
            options: $(this).find(".param-options").val(),
            defaultValue: $(this).find(".param-default").is(":checkbox")
                ? $(this).find(".param-default").is(":checked").toString()
                : $(this).find(".param-default").val(),
            isRequired: $(this).find(".param-required").is(":checked"),
            regexPattern: $(this).find(".param-regex").val()
        });
    });

    return { name, keyName, templateText, parameters: params };
}


window.updatePreview = function () {
    try {
        let dto = collectTemplateDataForPreview();
        let preview = dto.templateText || "Preview will appear here...";

        dto.parameters.forEach(p => {
            let value = p.defaultValue || `[${p.keyName}]`;
            let errorMsg = null;

            // Boolean handling
            if (p.type === "boolean") {
                value = (p.defaultValue === "true") ? "true" : "false";
            }

            // Multi-select handling (CSV string → array)
            if (p.type === "multiselect" && p.defaultValue) {
                value = "[" + p.defaultValue.split(",").map(v => v.trim()).join(", ") + "]";
            }

            // Regex validation
            if (p.regexPattern) {
                try {
                    let regex = new RegExp(p.regexPattern);
                    if (!regex.test(p.defaultValue || "")) {
                        errorMsg = `⚠️ ${p.name}: value '${p.defaultValue}' does not match regex ${p.regexPattern}`;
                    }
                } catch {
                    errorMsg = `⚠️ ${p.name}: invalid regex ${p.regexPattern}`;
                }
            }

            // Required validation
            if (p.isRequired && !p.defaultValue) {
                errorMsg = `⚠️ ${p.name} is required but missing`;
            }

            // Highlight invalid values with tooltip
            if (errorMsg) {
                value = `<span class="invalid-param" title="${errorMsg}">${value}</span>`;
            }

            // Replace placeholders with value
            preview = preview.replaceAll("{" + p.keyName + "}", value);
        });

        // Inject HTML into preview box
        $("#previewBox").html(preview);

        // Enable tooltips (Bootstrap)
        $(".invalid-param").tooltip({
            placement: "top",
            trigger: "hover"
        });

    } catch (err) {
        console.warn("Preview skipped:", err.message);
    }
};



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
