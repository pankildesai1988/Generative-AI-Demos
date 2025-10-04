var charts = {};

$(document).ready(function () {
    function getFilters() {
        return {
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val(),
            promptStyle: $("#promptStyleFilter").val(),
            slaStatus: $("#slaFilter").val() || null // ✅ include SLA filter if present
        };
    }

    function destroyChart(id) {
        if (charts[id]) {
            charts[id].destroy();
            delete charts[id];
        }
    }

    function loadAnalytics() {
        loadSlaCompliance();
        loadAverageLatencies();
        loadPromptStyleUsage();
        loadTrends();
        loadProviderAnalytics(); // ✅ NEW
    }

    // ------------------------
    // SLA Pie Chart
    // ------------------------
    function loadSlaCompliance() {
        $.getJSON("/Analytics/GetSlaCompliance", getFilters(), function (res) {
            const data = res.data; // ✅ unified response
            if (!data || data.length === 0) return;

            const labels = data.map(x => x.promptStyle);
            const values = data.map(x => x.complianceRate);

            destroyChart("slaChart");
            const canvas = document.getElementById("slaChart");
            if (!canvas) return;

            charts.slaChart = new Chart(canvas.getContext("2d"), {
                type: "pie",
                data: { labels, datasets: [{ data: values }] },
                options: {
                    onClick: function (e, activeEls) {
                        if (activeEls.length > 0) {
                            const idx = activeEls[0].index;
                            const promptStyle = labels[idx];
                            window.location.href = `/RagHistory?promptStyle=${promptStyle}`;
                        }
                    }
                }
            });
        });
    }

    // ------------------------
    // Avg Latencies by Provider/Model
    // ------------------------
    function loadAverageLatencies() {
        $.getJSON("/Analytics/GetAverageLatencies", getFilters(), function (res) {
            const data = res.data;
            if (!data || data.length === 0) return;

            const labels = data.map(x => `${x.provider} (${x.model})`);
            const values = data.map(x => x.avgTotalLatencyMs);

            destroyChart("latencyChart");
            const canvas = document.getElementById("latencyChart");
            if (!canvas) return;

            charts.latencyChart = new Chart(canvas.getContext("2d"), {
                type: "bar",
                data: {
                    labels,
                    datasets: [{ label: "Avg Latency (ms)", data: values }]
                }
            });
        });
    }

    // ------------------------
    // PromptStyle Usage
    // ------------------------
    function loadPromptStyleUsage() {
        $.getJSON("/Analytics/GetPromptStyleUsage", getFilters(), function (res) {
            const data = res.data;
            if (!data || data.length === 0) return;

            const labels = data.map(x => x.promptStyle);
            const values = data.map(x => x.count);

            destroyChart("promptStyleChart");
            const canvas = document.getElementById("promptStyleChart");
            if (!canvas) return;

            charts.promptStyleChart = new Chart(canvas.getContext("2d"), {
                type: "pie",
                data: { labels, datasets: [{ data: values }] },
                options: {
                    onClick: function (e, activeEls) {
                        if (activeEls.length > 0) {
                            const idx = activeEls[0].index;
                            const promptStyle = labels[idx];
                            window.location.href = `/RagHistory?promptStyle=${promptStyle}`;
                        }
                    }
                }
            });
        });
    }

    // ------------------------
    // Trends Line Chart
    // ------------------------
    function loadTrends() {
        $.getJSON("/Analytics/GetTrends", getFilters(), function (res) {
            const data = res.data;
            if (!data || data.length === 0) return;

            const labels = data.map(x => x.date);
            const values = data.map(x => x.avgTotalLatencyMs);

            destroyChart("trendsChart");
            const canvas = document.getElementById("trendsChart");
            if (!canvas) return;

            charts.trendsChart = new Chart(canvas.getContext("2d"), {
                type: "line",
                data: {
                    labels,
                    datasets: [{ label: "Avg Total Latency (ms)", data: values, fill: false, borderColor: "#007bff" }]
                }
            });
        });
    }

    // ------------------------
    // Provider/Model Analytics
    // ------------------------
    function loadProviderAnalytics() {
        $.getJSON("/Analytics/GetProviderAnalytics", getFilters(), function (res) {
            const data = res.data;
            if (!data || data.length === 0) return;

            // ✅ KPI Updates
            const totalRuns = data.reduce((sum, x) => sum + x.totalRuns, 0);
            const avgLatency = (data.reduce((sum, x) => sum + x.avgTotalLatencyMs, 0) / data.length).toFixed(1);
            const totalWithinSla = data.reduce((sum, x) => sum + x.withinSlaCount, 0);
            const slaCompliance = totalRuns > 0 ? ((totalWithinSla * 100) / totalRuns).toFixed(1) : 0;

            $("#totalRuns").text(totalRuns);
            $("#avgLatency").text(avgLatency);
            $("#slaCompliance").text(slaCompliance + "%");

            // ✅ Chart Data
            const labels = data.map(x => `${x.provider} (${x.model})`);
            const latencyValues = data.map(x => x.avgTotalLatencyMs);
            const slaValues = data.map(x => x.slaComplianceRate);

            destroyChart("providerLatencyChart");
            destroyChart("providerSlaChart");

            const latencyCanvas = document.getElementById("providerLatencyChart");
            const slaCanvas = document.getElementById("providerSlaChart");
            if (!latencyCanvas || !slaCanvas) return;

            // Latency Chart
            charts.providerLatencyChart = new Chart(latencyCanvas.getContext("2d"), {
                type: "bar",
                data: { labels, datasets: [{ label: "Avg Latency (ms)", data: latencyValues, backgroundColor: "#007bff" }] },
                options: {
                    onClick: function (e, activeEls) {
                        if (activeEls.length > 0) {
                            const idx = activeEls[0].index;
                            const label = labels[idx];
                            const [provider, modelWithParen] = label.split(" (");
                            const model = modelWithParen.replace(")", "");
                            window.location.href = `/RagHistory?provider=${provider}&model=${model}`;
                        }
                    }
                }
            });

            // SLA Chart
            charts.providerSlaChart = new Chart(slaCanvas.getContext("2d"), {
                type: "bar",
                data: {
                    labels,
                    datasets: [{
                        label: "SLA Compliance (%)",
                        data: data.map(x => x.slaComplianceRate ?? x.SlaComplianceRate), // ✅ fix
                        backgroundColor: "#28a745"
                    }]
                },
                options: {
                    scales: { y: { min: 0, max: 100 } },
                    onClick: function (e, activeEls) {
                        if (activeEls.length > 0) {
                            const idx = activeEls[0].index;
                            const label = labels[idx];
                            const [provider, modelWithParen] = label.split(" (");
                            const model = modelWithParen.replace(")", "");
                            window.location.href = `/RagHistory?provider=${provider}&model=${model}`;
                        }
                    }
                }
            });

        });
    }

    // Load on page load
    loadAnalytics();

    // Reload when filters applied
    $("#applyFilters").click(loadAnalytics);
});
