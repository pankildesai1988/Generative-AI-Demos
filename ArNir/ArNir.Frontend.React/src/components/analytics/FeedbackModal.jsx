import { useState } from "react";

export default function FeedbackModal() {
  const [open, setOpen] = useState(false);
  const [feedback, setFeedback] = useState("");

  const handleSubmit = () => {
    console.log("Feedback submitted:", feedback);
    setOpen(false);
    setFeedback("");
  };

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
      >
        Feedback
      </button>

      {open && (
        <div className="fixed inset-0 bg-black bg-opacity-40 flex justify-center items-center z-50">
          <div className="bg-white p-6 rounded shadow-lg w-96">
            <h4 className="text-lg font-semibold mb-2">Share Feedback</h4>
            <textarea
              rows="4"
              value={feedback}
              onChange={(e) => setFeedback(e.target.value)}
              className="w-full border rounded p-2 mb-3"
              placeholder="Enter your thoughts..."
            />
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setOpen(false)}
                className="px-3 py-1 bg-gray-300 rounded hover:bg-gray-400"
              >
                Cancel
              </button>
              <button
                onClick={handleSubmit}
                className="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700"
              >
                Submit
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
