import SourceViewer from "../components/SourceViewer";

export default { title: "Components/SourceViewer", component: SourceViewer };

const sampleChunks = [
  { documentTitle: "Medical Guidelines.pdf", chunkText: "Patients with hypertension should maintain a diet low in sodium...", rank: 1, retrievalType: "vector" },
  { documentTitle: "Clinical Study 2024.pdf", chunkText: "The study found a 23% reduction in cardiovascular events...", rank: 2, retrievalType: "hybrid" },
];

export const Default = { args: { chunks: sampleChunks, title: "Sources" } };
export const Empty = { args: { chunks: [], title: "Sources" } };
export const SingleSource = { args: { chunks: [sampleChunks[0]], title: "Source Documents" } };
