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
                        return results.map(r => {
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
    }).fail(() => {
        showToast("Error fetching history", "error");
    });
}
