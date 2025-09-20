$(document).ready(function () {
    $("#runBtn").click(function () {
        const query = $("#queryInput").val();
        if (!query) {
            alert("Please enter a query.");
            return;
        }

        // Show spinner
        $("#loadingSpinner").show();
        $("#resultsSection").hide();

        $.post("/RagComparison/Run", { query: query }, function (res) {
            // Hide spinner
            $("#loadingSpinner").hide();
            $("#resultsSection").show();

            $("#baselineOutput").text(res.baselineAnswer);
            $("#ragOutput").text(res.ragAnswer);

            $("#contextList").empty();
            res.retrievedChunks.forEach(c => {
                $("#contextList").append(
                    `<li class="list-group-item">
                        <strong>[${c.retrievalType}]</strong> 
                        (Doc: ${c.documentTitle}, ID: ${c.documentId})<br>
                        ${c.chunkText}
                    </li>`
                );
            });

            $("#retrievalTime").text(res.retrievalLatencyMs);
            $("#llmTime").text(res.llmLatencyMs);
            $("#totalTime").text(res.totalLatencyMs);

            if (res.isWithinSla) {
                $("#slaBadge").removeClass().addClass("badge bg-success").text("✅ OK");
            } else {
                $("#slaBadge").removeClass().addClass("badge bg-danger").text("⚠️ Slow");
            }
        }).fail(function () {
            $("#loadingSpinner").hide();
            alert("Error: could not process query.");
        });
    });
});
