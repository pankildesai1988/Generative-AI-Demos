import { useFileUpload, FileUpload } from "@arnir/shared";
import { FileText, BarChart3, TrendingUp, Globe } from "lucide-react";
import toast from "react-hot-toast";

const SAMPLE_FILES = [
  {
    name: "annual-report-2024.txt",
    description: "TechCorp Annual Report 2024",
    icon: BarChart3,
  },
  {
    name: "quarterly-earnings-q3.txt",
    description: "Q3 2024 Earnings Summary",
    icon: TrendingUp,
  },
  {
    name: "market-analysis.txt",
    description: "AI Industry Market Analysis",
    icon: Globe,
  },
];

export default function FinancialUploadPage() {
  const upload = useFileUpload();

  const handleSampleUpload = async (fileName) => {
    try {
      const response = await fetch(`/sample-data/${fileName}`);
      const blob = await response.blob();
      const file = new File([blob], fileName, { type: "text/plain" });
      await upload.uploadFile(file, "finance-demo");
      toast.success(`Sample "${fileName}" queued for processing.`);
    } catch (err) {
      toast.error(`Failed to load sample: ${fileName}`);
    }
  };

  return (
    <div className="p-6 max-w-4xl mx-auto space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">
          Upload Financial Reports
        </h1>
        <p className="text-gray-500 mt-1">
          Upload 10-K filings, annual reports, earnings transcripts, or market analysis documents.
        </p>
      </div>

      {/* Upload Zone */}
      <div className="bg-white rounded-xl border p-6">
        <FileUpload
          onUpload={upload.uploadFile}
          uploading={upload.uploading}
          error={upload.error}
          result={upload.result}
          onReset={upload.reset}
          guidance="Drag and drop financial documents here, or click to browse"
        />
      </div>

      {/* Sample Data Section */}
      <div>
        <h2 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2">
          <FileText className="text-primary-500" size={20} />
          Try Sample Data
        </h2>
        <p className="text-sm text-gray-500 mb-4">
          No financial documents handy? Use these sample reports to test the analyzer.
        </p>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {SAMPLE_FILES.map((sample) => (
            <div
              key={sample.name}
              className="bg-white border rounded-xl p-4 hover:shadow-md transition"
            >
              <div className="flex items-start gap-3">
                <div className="w-10 h-10 bg-primary-50 rounded-lg flex items-center justify-center flex-shrink-0">
                  <sample.icon className="text-primary-500" size={20} />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-sm text-gray-800">
                    {sample.description}
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5">{sample.name}</p>
                </div>
              </div>
              <button
                onClick={() => handleSampleUpload(sample.name)}
                disabled={upload.uploading}
                className="mt-3 w-full bg-primary-50 text-primary-500 text-sm py-1.5 rounded-lg hover:bg-primary-100 disabled:opacity-50 transition"
              >
                Upload Sample
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
