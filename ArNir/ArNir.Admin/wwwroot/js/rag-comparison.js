$(document).ready(function () {
    $("#runBtn").click(function (e) {
        e.preventDefault(); // ✅ Prevent form submission/page reload

        const query = $("#queryInput").val();
        const promptStyle = $("#promptStyle").val();

        if (!query) {
            alert("Please enter a query.");
            return;
        }

        // Show spinner, hide results
        $("#loadingSpinner").removeClass("d-none");
        $("#resultsSection").addClass("d-none");

        $.post("/RagComparison/Run", { query: query, promptStyle: promptStyle }, function (res) {
            // Hide spinner, show results
            $("#loadingSpinner").addClass("d-none");
            $("#resultsSection").removeClass("d-none");

            // Fill answers
            $("#baselineOutput").text(res.baselineAnswer || "No baseline answer.");
            $("#ragOutput").text(res.ragAnswer || "No RAG answer.");

            // Fill retrieved chunks
            $("#contextList").empty();
            if (res.retrievedChunks && res.retrievedChunks.length > 0) {
                res.retrievedChunks.forEach((c, i) => {
                    const preview = c.chunkText.length > 200 ? c.chunkText.substring(0, 200) + "..." : c.chunkText;

                    $("#contextList").append(`
                        <li class="list-group-item">
                            <strong>[${i + 1}] ${c.documentTitle}</strong> 
                            <small class="text-muted">(Doc ID: ${c.documentId}, Retrieval: ${c.retrievalType})</small>
                            <p class="mt-2 chunk-preview" id="preview-${i}">${preview}</p>
                            <p class="mt-2 chunk-full d-none" id="full-${i}">${c.chunkText}</p>
                            <button class="btn btn-link p-0 toggle-chunk" data-index="${i}">View Full</button>
                        </li>
                    `);
                });
            } else {
                $("#contextList").append("<li class='list-group-item text-muted'>No context retrieved</li>");
            }

            // Fill timings
            $("#retrievalTime").text(res.retrievalLatencyMs ?? "-");
            $("#llmTime").text(res.llmLatencyMs ?? "-");
            $("#totalTime").text(res.totalLatencyMs ?? "-");

            // SLA badge
            if (res.isWithinSla) {
                $("#slaBadge").removeClass().addClass("badge bg-success").text("✅ OK");
            } else {
                $("#slaBadge").removeClass().addClass("badge bg-danger").text("⚠️ Slow");
            }
        }).fail(function () {
            $("#loadingSpinner").addClass("d-none");
            alert("Error: could not process query.");
        });
    });

    // ✅ Expand/collapse chunks
    $(document).on("click", ".toggle-chunk", function () {
        const index = $(this).data("index");
        const $preview = $("#preview-" + index);
        const $full = $("#full-" + index);

        if ($full.hasClass("d-none")) {
            $preview.addClass("d-none");
            $full.removeClass("d-none");
            $(this).text("Show Less");
        } else {
            $full.addClass("d-none");
            $preview.removeClass("d-none");
            $(this).text("View Full");
        }
    });
});
