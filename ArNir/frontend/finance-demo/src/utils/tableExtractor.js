function isTableSeparator(line) {
  const normalized = line.replace(/\|/g, "").trim();
  if (!normalized) {
    return false;
  }

  return normalized
    .split("")
    .every((char) => char === "-" || char === ":" || char === " ");
}

export function extractMarkdownTable(text = "") {
  const lines = text
    .split("\n")
    .map((line) => line.trim())
    .filter(Boolean);

  for (let index = 0; index < lines.length - 2; index += 1) {
    const header = lines[index];
    const separator = lines[index + 1];

    if (!header.includes("|") || !isTableSeparator(separator)) {
      continue;
    }

    const rows = [];
    let cursor = index + 2;
    while (cursor < lines.length && lines[cursor].includes("|")) {
      rows.push(lines[cursor]);
      cursor += 1;
    }

    const headers = header
      .split("|")
      .map((cell) => cell.trim())
      .filter(Boolean);

    const data = rows.map((row) =>
      row
        .split("|")
        .map((cell) => cell.trim())
        .filter(Boolean)
    );

    if (headers.length > 0 && data.length > 0) {
      return { headers, rows: data };
    }
  }

  return null;
}
