$(document).ready(function () {
    var table = $('#historyTable').DataTable({
        ajax: {
            url: '/RagHistory/GetHistory',
            data: function (d) {
                d.slaStatus = $('#slaFilter').val();
                d.startDate = $('#startDate').val();
                d.endDate = $('#endDate').val();
                d.queryText = $('#querySearch').val();
                d.promptStyle = $('#promptStyleFilter').val();
            }
        },
        columns: [
            { data: 'query' },
            { data: 'promptStyle' },
            { data: 'createdAt' },
            { data: 'isWithinSla', render: function (data) { return data ? 'OK' : 'Slow'; } },
            { data: 'retrievalLatencyMs' },
            { data: 'llmLatencyMs' },
            { data: 'totalLatencyMs' }
        ]
    });

    // ✅ Reload on filter change
    $('#slaFilter, #startDate, #endDate, #querySearch, #promptStyleFilter').on('change keyup', function () {
        table.ajax.reload();
    });

    // ✅ Compare Mode toggle
    let compareMode = false;
    let selectedRows = [];

    $('#compareModeToggle').on('change', function () {
        compareMode = $(this).is(':checked');
        selectedRows = [];
        $('#historyTable tbody tr').removeClass('table-active');
        if (compareMode) {
            alert("Compare Mode enabled: select multiple rows to compare.");
        }
    });

    // ✅ Row click behavior
    $('#historyTable tbody').on('click', 'tr', function () {
        var data = table.row(this).data();

        if (compareMode) {
            if ($(this).hasClass('table-active')) {
                $(this).removeClass('table-active');
                selectedRows = selectedRows.filter(x => x.id !== data.id);
            } else {
                $(this).addClass('table-active');
                selectedRows.push(data);
            }

            if (selectedRows.length >= 2) {
                renderComparison(selectedRows);
            }
        } else {
            $.getJSON('/RagHistory/GetDetails/' + data.id, function (details) {
                showDetailsModal(details);
            });
        }
    });

    // ✅ Render Comparison
    function renderComparison(rows) {
        $('#compareResults').empty();

        rows.forEach((row, idx) => {
            $.getJSON('/RagHistory/GetDetails/' + row.id, function (details) {
                $('#compareResults').append(`
                    <div class="col-md-6 border p-2">
                        <h6>Run #${idx + 1} (${details.promptStyle})</h6>
                        <p><strong>Query:</strong> ${details.query}</p>
                        <p><strong>Baseline Answer:</strong></p>
                        <pre>${details.baselineAnswer}</pre>
                        <p><strong>RAG Answer:</strong></p>
                        <pre>${details.ragAnswer}</pre>
                        <h6>Retrieved Chunks</h6>
                        <pre>${details.retrievedChunksJson}</pre>
                        <p><strong>Latency:</strong> Retrieval ${details.retrievalLatencyMs} ms | 
                           LLM ${details.llmLatencyMs} ms | Total ${details.totalLatencyMs} ms</p>
                    </div>
                `);

                $('#compareModal').modal('show');
            });
        });
    }

    // ✅ Single details modal renderer
    function showDetailsModal(details) {
        $('#modalQuery').text(details.query);
        $('#modalBaseline').text(details.baselineAnswer);
        $('#modalRag').text(details.ragAnswer);
        $('#modalPromptStyle').text(details.promptStyle);

        var chunks = [];
        try {
            chunks = JSON.parse(details.retrievedChunksJson);
        } catch (e) {
            console.error("Invalid JSON in retrieved chunks", e);
        }

        var $chunksList = $('#modalChunks');
        $chunksList.empty();

        if (chunks.length > 0) {
            chunks.forEach(function (chunk, index) {
                var preview = chunk.ChunkText.substring(0, 200);
                var full = chunk.ChunkText;

                var listItem = `
                    <li class="list-group-item">
                        <strong>${index + 1}. ${chunk.DocumentTitle}</strong><br/>
                        <small class="text-muted">Doc ID: ${chunk.DocumentId} | Retrieval: ${chunk.RetrievalType}</small>
                        <p class="mt-2 chunk-preview" id="preview-${index}">${preview}...</p>
                        <p class="mt-2 chunk-full d-none" id="full-${index}">${full}</p>
                        <button class="btn btn-link p-0 toggle-chunk" data-index="${index}">View Full</button>
                    </li>`;
                $chunksList.append(listItem);
            });
        } else {
            $chunksList.append("<li class='list-group-item text-muted'>No chunks retrieved</li>");
        }

        $('#modalLatency').text(
            `Retrieval: ${details.retrievalLatencyMs} ms | LLM: ${details.llmLatencyMs} ms | Total: ${details.totalLatencyMs} ms`
        );

        $('#historyModal').modal('show');
    }

    // ✅ Expand/collapse chunks
    $(document).on('click', '.toggle-chunk', function () {
        var index = $(this).data('index');
        var $preview = $('#preview-' + index);
        var $full = $('#full-' + index);
        if ($full.hasClass('d-none')) {
            $preview.addClass('d-none');
            $full.removeClass('d-none');
            $(this).text('Show Less');
        } else {
            $full.addClass('d-none');
            $preview.removeClass('d-none');
            $(this).text('View Full');
        }
    });

    // ✅ Export All (CSV) with chunks
    $(document).on('click', '#exportAllCsvBtn', function () {
        $.getJSON('/RagHistory/GetHistory', {
            slaStatus: $('#slaFilter').val(),
            startDate: $('#startDate').val(),
            endDate: $('#endDate').val(),
            queryText: $('#querySearch').val(),
            promptStyle: $('#promptStyleFilter').val()
        }, function (res) {
            let csvContent = "data:text/csv;charset=utf-8,";
            csvContent += "Query,PromptStyle,BaselineAnswer,RagAnswer,RetrievalLatency,LLMLatency,TotalLatency,CreatedAt,Chunks\n";

            res.data.forEach(row => {
                let line = `"${row.query.replace(/"/g, '""')}",` +
                    `"${row.promptStyle}",` +
                    `"${row.baselineAnswer ? row.baselineAnswer.replace(/"/g, '""') : ""}",` +
                    `"${row.ragAnswer ? row.ragAnswer.replace(/"/g, '""') : ""}",` +
                    `${row.retrievalLatencyMs},${row.llmLatencyMs},${row.totalLatencyMs},${row.createdAt},` +
                    `"${row.retrievedChunksJson ? row.retrievedChunksJson.replace(/"/g, '""') : ""}"`;
                csvContent += line + "\n";
            });

            const encodedUri = encodeURI(csvContent);
            const link = document.createElement("a");
            link.setAttribute("href", encodedUri);
            link.setAttribute("download", "rag_history.csv");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        });
    });

    // ✅ Export All (Excel) with chunks
    $(document).on('click', '#exportAllExcelBtn', function () {
        $.getJSON('/RagHistory/GetHistory', {
            slaStatus: $('#slaFilter').val(),
            startDate: $('#startDate').val(),
            endDate: $('#endDate').val(),
            queryText: $('#querySearch').val(),
            promptStyle: $('#promptStyleFilter').val()
        }, function (res) {
            let tableHtml = "<table border='1'><tr><th>Query</th><th>PromptStyle</th><th>BaselineAnswer</th><th>RagAnswer</th><th>RetrievalLatency</th><th>LLMLatency</th><th>TotalLatency</th><th>CreatedAt</th><th>Chunks</th></tr>";

            res.data.forEach(row => {
                tableHtml += `<tr>
                    <td>${row.query}</td>
                    <td>${row.promptStyle}</td>
                    <td>${row.baselineAnswer || ""}</td>
                    <td>${row.ragAnswer || ""}</td>
                    <td>${row.retrievalLatencyMs}</td>
                    <td>${row.llmLatencyMs}</td>
                    <td>${row.totalLatencyMs}</td>
                    <td>${row.createdAt}</td>
                    <td>${row.retrievedChunksJson || ""}</td>
                </tr>`;
            });

            tableHtml += "</table>";
            const blob = new Blob([tableHtml], { type: "application/vnd.ms-excel" });
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = "rag_history.xls";
            link.click();
        });
    });
});
