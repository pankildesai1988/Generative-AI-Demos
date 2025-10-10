$(document).ready(function () {
    // Export buttons
    $("#btnExportExcel").click(() => {
        let start = $("#startDate").val();
        let end = $("#endDate").val();
        window.location.href = `/Reports/ExportProviderAnalytics?format=excel&startDate=${start}&endDate=${end}`;
    });

    $("#btnExportCsv").click(() => {
        let start = $("#startDate").val();
        let end = $("#endDate").val();
        window.location.href = `/Reports/ExportProviderAnalytics?format=csv&startDate=${start}&endDate=${end}`;
    });
});
