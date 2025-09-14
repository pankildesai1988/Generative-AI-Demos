// ✅ API endpoints for comparison
const comparisonApiUrl = API_BASE_URL + "/api/comparison";
const providerApiUrl = API_BASE_URL + "/api/provider/models";

// 🔑 Ensure AJAX includes authentication cookies + JWT
$.ajaxSetup({
    beforeSend: function (xhr) {
        if (!ensureAuthenticated()) return false;
        let token = getJwtToken();
        xhr.setRequestHeader("Authorization", "Bearer " + token);
    },
    statusCode: {
        401: function () {
            localStorage.removeItem("jwtToken");
            window.location.href = "/Account/Login";
        }
    }
});

// ✅ Load providers + models into <select>
window.loadProviders = function () {
    $.get(providerApiUrl, function (res) {
        if (!res.success) {
            showToast("Failed to load providers", "error");
            return;
        }

        let select = $("#providerSelect");
        select.empty();

        res.data.forEach(provider => {
            let optGroup = $(`<optgroup label="${provider.provider}"></optgroup>`);
            provider.models.forEach(model => {
                let option = $(`<option></option>`)
                    .val(`${provider.provider}:${model}`)
                    .text(`${provider.provider} → ${model}`);
                optGroup.append(option);
            });
            select.append(optGroup);
        });
    }).fail(() => {
        showToast("Error loading providers", "error");
    });
};

// ✅ Run a comparison request
window.runComparison = function () {
    let inputText = $("#comparisonInput").val();
    let selectedModels = $("#providerSelect").val();

    if (!inputText || !selectedModels || selectedModels.length === 0) {
        Swal.fire({ title: "Validation Error", text: "Enter input and select at least one model", icon: "error" });
        return;
    }

    let dto = {
        originalSessionId: 0,
        inputText: inputText,
        modelNames: selectedModels
    };

    // ✅ Show loading
    $("#comparisonLoading").show();
    $("#comparisonResults").empty();

    $.ajax({
        url: comparisonApiUrl + "/run",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(dto),
        success: function (res) {
            $("#comparisonLoading").hide();

            let container = $("#comparisonResults");
            container.empty();

            if (!res.success) {
                container.html(`<div class="alert alert-danger">Error: ${res.error}</div>`);
                return;
            }

            // ✅ Deduplicate backend results
            let seen = new Set();
            res.data.results = res.data.results.filter(r => {
                let key = r.provider + ":" + r.modelName;
                if (seen.has(key)) return false;
                seen.add(key);
                return true;
            });

            // ✅ Build grid layout (columns per provider)
            let headerRow = `<div class="row mb-3">`;
            res.data.results.forEach(r => {
                headerRow += `
                    <div class="col border p-2 text-center fw-bold">
                        ${r.provider} <br> <small>${r.modelName}</small>
                        ${r.isError ? `<span class="badge bg-danger ms-2">Error</span>` : ""}
                    </div>`;
            });
            headerRow += `</div>`;

            let bodyRow = `<div class="row">`;
            res.data.results.forEach(r => {
                bodyRow += `
                    <div class="col border p-3" style="min-height: 200px; overflow-y: auto;">
                        ${r.isError
                        ? `<p class="text-danger"><strong>${r.errorCode}</strong>: ${r.errorMessage}</p>`
                        : `<p>${r.responseText || "[No response]"}</p>`}
                        <small class="text-muted d-block mt-2">
                            ${r.latencyMs ? `Latency: ${r.latencyMs.toFixed(2)} ms` : ""}
                        </small>
                    </div>`;
            });
            bodyRow += `</div>`;

            container.append(headerRow + bodyRow);

            // ✅ Auto-scroll to results
            $('html, body').animate({
                scrollTop: container.offset().top - 50
            }, 500);
        },
        error: function () {
            $("#comparisonLoading").hide();
            showToast("Comparison request failed", "error");
        }
    });
};

// ✅ Bind events once
$(document).ready(function () {
    loadProviders();
    $("#runComparisonBtn").off("click").on("click", runComparison);
});
