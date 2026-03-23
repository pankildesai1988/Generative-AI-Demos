import FileUpload from "../components/FileUpload";

export default { title: "Components/FileUpload", component: FileUpload };

export const Default = {
  args: {
    onUpload: (file) => console.log("Upload:", file.name),
    uploading: false,
    error: null,
    result: null,
    onReset: () => {},
  },
};

export const Uploading = {
  args: {
    onUpload: () => {},
    uploading: true,
    error: null,
    result: null,
  },
};

export const WithError = {
  args: {
    onUpload: () => {},
    uploading: false,
    error: "File type not supported.",
    result: null,
    onReset: () => {},
  },
};

export const Success = {
  args: {
    onUpload: () => {},
    uploading: false,
    error: null,
    result: { message: "Document ingested successfully!", documentId: 42 },
    onReset: () => {},
  },
};
