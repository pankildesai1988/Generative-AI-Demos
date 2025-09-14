const comparisonHistoryApiUrl = API_BASE_URL + "/api/comparison/history";

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

$(document).ready(function () {
    loadComparisonHistory();
});

function loadComparisonHistory() {
    $.get(comparisonHistoryApiUrl, function (res) {
        if (!res.success) {
            showToast("Failed to load history", "error");
            return;
        }

        let table = $("#comparisonHistoryTable").DataTable({
            destroy: true,
            data: res.data,
            columns: [
                { data: "id" },
                { data: "inputText", render: (d) => `<span title="${d}">${d.substring(0, 50)}...</span>` },
                { data: "createdAt", render: (d) => new Date(d).toLocaleString() },
                {
                    data: "results",
                    render: function (results) {
                        // ✅ Deduplicate for summary badges
                        let seen = new Set();
                        let filtered = results.filter(r => {
                            let key = r.provider + ":" + r.modelName;
                            if (seen.has(key)) return false;
                            seen.add(key);
                            return true;
                        });

                        return filtered.map(r => {
                            return `
                                <span class="badge ${r.isError ? "bg-danger" : "bg-success"}" title="${r.isError ? r.errorMessage : "OK"}">
                                    ${r.provider}:${r.modelName}
                                </span>
                            `;
                        }).join(" ");
                    }
                }
            ]
        });

        // ✅ Row click → open modal
        $('#comparisonHistoryTable tbody').off("click").on('click', 'tr', function () {
            const rowData = table.row(this).data();
            if (rowData) {
                loadComparisonDetails(rowData.id);
            }
        });
    }).fail(() => {
        showToast("Error fetching history", "error");
    });
}

function loadComparisonDetails(id) {
    $("#comparisonDetailsContent").html(`<p class="text-muted">Loading...</p>`);
    $("#comparisonDetailsModal").modal("show");

    $.get(`${comparisonHistoryApiUrl}/${id}`, function (res) {
        if (!res.success) {
            $("#comparisonDetailsContent").html(`<div class="alert alert-danger">Error: ${res.error}</div>`);
            return;
        }

        let c = res.data;

        // ✅ Deduplicate provider:model results
        let seen = new Set();
        c.results = c.results.filter(r => {
            let key = r.provider + ":" + r.modelName;
            if (seen.has(key)) return false;
            seen.add(key);
            return true;
        });

        let html = `
            <h6>Prompt:</h6>
            <p>${c.inputText}</p>
            <h6>Created At:</h6>
            <p>${new Date(c.createdAt).toLocaleString()}</p>
            <h6>Results:</h6>
        `;

        c.results.forEach(r => {
            html += `
                <div class="card mb-2">
                    <div class="card-header">
                        <strong>${r.provider}</strong> — ${r.modelName}
                        ${r.isError ? `<span class="badge bg-danger ms-2">Error</span>` : ""}
                    </div>
                    <div class="card-body">
                        ${r.isError
                    ? `<p class="text-danger"><strong>${r.errorCode}</strong>: ${r.errorMessage}</p>`
                    : `<p>${r.responseText || "[No response]"}</p>`}
                    </div>
                </div>
            `;
        });

        $("#comparisonDetailsContent").html(html);
    }).fail(() => {
        $("#comparisonDetailsContent").html(`<div class="alert alert-danger">Failed to load details</div>`);
    });
}
