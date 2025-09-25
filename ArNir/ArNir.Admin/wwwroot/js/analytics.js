$(document).ready(function () {
    let charts = {}; // store chart instances

    // Load initial analytics
    loadAnalytics();

    // Apply filters on button click
    $("#applyFiltersBtn").click(function () {
        loadAnalytics();
    });

    function getFilters() {
        return {
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val(),
            slaStatus: $("#slaFilter").val(),
            promptStyle: $("#promptStyleFilter").val()
        };
    }

    function destroyChart(chartId) {
        if (charts[chartId]) {
            charts[chartId].destroy();
            charts[chartId] = null;
        }
    }

    function loadAnalytics() {
        loadSlaCompliance();
        loadAverageLatencies();
        loadPromptStyleUsage();
        loadTrends();
    }

    function loadSlaCompliance() {
        const filters = getFilters();
        $.getJSON("/Analytics/GetSlaCompliance", filters, function (res) {
            destroyChart("slaChart");
            const ctx = document.getElementById("slaChart").getContext("2d");
            charts.slaChart = new Chart(ctx, {
                type: "pie",
                data: {
                    labels: ["Within SLA", "Slow"],
                    datasets: [{
                        data: [res.withinSlaCount, res.totalRuns - res.withinSlaCount],
                        backgroundColor: ["#28a745", "#dc3545"]
                    }]
                },
                options: {
                    onClick: (evt, elements) => {
                        if (elements.length > 0) {
                            const index = elements[0].index;
                            const sla = index === 0 ? "ok" : "slow";
                            window.location.href = `/RagHistory?slaStatus=${sla}`;
                        }
                    }
                }
            });
        });
    }

    function loadAverageLatencies() {
        const filters = getFilters();
        $.getJSON("/Analytics/GetAverageLatencies", filters, function (res) {
            destroyChart("latencyChart");
            const ctx = document.getElementById("latencyChart").getContext("2d");
            charts.latencyChart = new Chart(ctx, {
                type: "bar",
                data: {
                    labels: ["Retrieval", "LLM", "Total"],
                    datasets: [{
                        label: "Latency (ms)",
                        data: [res.avgRetrievalLatencyMs, res.avgLlmLatencyMs, res.avgTotalLatencyMs],
                        backgroundColor: ["#007bff", "#ffc107", "#17a2b8"]
                    }]
                },
                options: { responsive: true, plugins: { legend: { display: false } } }
            });
        });
    }

    function loadPromptStyleUsage() {
        const filters = getFilters();
        $.getJSON("/Analytics/GetPromptStyleUsage", filters, function (res) {
            destroyChart("promptStyleChart");
            const ctx = document.getElementById("promptStyleChart").getContext("2d");
            charts.promptStyleChart = new Chart(ctx, {
                type: "pie",
                data: {
                    labels: res.map(x => x.promptStyle),
                    datasets: [{
                        data: res.map(x => x.count),
                        backgroundColor: ["#007bff", "#28a745", "#ffc107", "#dc3545", "#17a2b8"]
                    }]
                },
                options: {
                    onClick: (evt, elements) => {
                        if (elements.length > 0) {
                            const index = elements[0].index;
                            const style = res[index].promptStyle;
                            window.location.href = `/RagHistory?promptStyle=${style}`;
                        }
                    }
                }
            });
        });
    }

    function loadTrends() {
        const filters = getFilters();
        $.getJSON("/Analytics/GetTrends", filters, function (res) {
            destroyChart("trendChart");
            const ctx = document.getElementById("trendChart").getContext("2d");
            charts.trendChart = new Chart(ctx, {
                type: "line",
                data: {
                    labels: res.map(x => x.date),
                    datasets: [
                        {
                            label: "Avg Latency (ms)",
                            data: res.map(x => x.avgTotalLatencyMs),
                            borderColor: "#007bff",
                            fill: false,
                            yAxisID: "y1"
                        },
                        {
                            label: "SLA Compliance (%)",
                            data: res.map(x => x.slaComplianceRate),
                            borderColor: "#28a745",
                            fill: false,
                            yAxisID: "y2"
                        }
                    ]
                },
                options: {
                    responsive: true,
                    scales: {
                        y1: { type: "linear", position: "left" },
                        y2: { type: "linear", position: "right", min: 0, max: 100 }
                    },
                    onClick: (evt, elements) => {
                        if (elements.length > 0) {
                            const index = elements[0].index;
                            const date = res[index].date;
                            window.location.href = `/RagHistory?startDate=${date}&endDate=${date}`;
                        }
                    }
                }
            });
        });
    }

});
