import { useFileUpload, FileUpload } from "@arnir/shared";
import { FileText, Laptop, Smartphone, Headphones } from "lucide-react";
import toast from "react-hot-toast";

const SAMPLE_FILES = [
  {
    name: "laptop-catalog.txt",
    description: "Laptop Catalog — 10 Products",
    icon: Laptop,
  },
  {
    name: "mobile-phones.txt",
    description: "Smartphone Catalog — 8 Phones",
    icon: Smartphone,
  },
  {
    name: "electronics-accessories.txt",
    description: "Electronics Accessories — 15 Items",
    icon: Headphones,
  },
];

export default function CatalogUploadPage() {
  const upload = useFileUpload();

  const handleSampleUpload = async (fileName) => {
    try {
      const response = await fetch(`/sample-data/${fileName}`);
      const blob = await response.blob();
      const file = new File([blob], fileName, { type: "text/plain" });
      await upload.uploadFile(file, "ecommerce-demo");
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
          Upload Product Catalog
        </h1>
        <p className="text-gray-500 mt-1">
          Upload product catalogs, spec sheets, or inventory lists to build your product knowledge base.
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
          guidance="Drag and drop product catalogs here, or click to browse"
        />
      </div>

      {/* Sample Data Section */}
      <div>
        <h2 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2">
          <FileText className="text-primary-600" size={20} />
          Try Sample Data
        </h2>
        <p className="text-sm text-gray-500 mb-4">
          No product catalog handy? Use these sample files to test the advisor.
        </p>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {SAMPLE_FILES.map((sample) => (
            <div
              key={sample.name}
              className="bg-white border rounded-xl p-4 hover:shadow-md transition"
            >
              <div className="flex items-start gap-3">
                <div className="w-10 h-10 bg-primary-50 rounded-lg flex items-center justify-center flex-shrink-0">
                  <sample.icon className="text-primary-600" size={20} />
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
                className="mt-3 w-full bg-primary-50 text-primary-700 text-sm py-1.5 rounded-lg hover:bg-primary-100 disabled:opacity-50 transition"
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
