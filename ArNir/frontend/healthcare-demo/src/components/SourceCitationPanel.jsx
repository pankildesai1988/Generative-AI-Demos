import { SourceViewer } from "@arnir/shared";
import { BookOpen, FileText } from "lucide-react";

export default function SourceCitationPanel({ chunks = [] }) {
  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center gap-2">
        <BookOpen className="text-primary-600" size={20} />
        <h3 className="text-lg font-semibold text-gray-800">
          Medical Sources
        </h3>
      </div>

      {chunks.length === 0 ? (
        <div className="text-center py-12">
          <FileText className="mx-auto text-gray-300 mb-3" size={40} />
          <p className="text-sm text-gray-400">
            Source citations will appear here after you ask a question.
          </p>
          <p className="text-xs text-gray-300 mt-1">
            Upload medical documents first to enable RAG retrieval.
          </p>
        </div>
      ) : (
        <>
          <div className="bg-primary-50 border border-primary-200 rounded-lg p-3">
            <p className="text-xs text-primary-700">
              <span className="font-semibold">{chunks.length} source{chunks.length !== 1 ? "s" : ""}</span>{" "}
              retrieved from your medical knowledge base.
            </p>
          </div>
          <SourceViewer chunks={chunks} title="Retrieved Citations" />
        </>
      )}
    </div>
  );
}
