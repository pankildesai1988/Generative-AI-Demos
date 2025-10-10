import { useState } from "react";
import { submitFeedback } from "../api/client";

export default function FeedbackModal({ historyId, onClose }) {
  const [rating, setRating] = useState(0);
  const [comments, setComments] = useState("");
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async () => {
    setLoading(true);
    try {
      await submitFeedback({
        historyId,
        rating,
        comments
      });
      setSubmitted(true);
      setTimeout(onClose, 1000);
    } catch (err) {
      alert("Error submitting feedback");
    } finally {
      setLoading(false);
    }
  };

  if (submitted) {
    return (
      <div className="p-6 text-center">
        <p className="text-green-600 font-semibold">✅ Feedback saved!</p>
      </div>
    );
  }

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-40">
      <div className="bg-white p-6 rounded-2xl w-96 shadow-xl space-y-4">
        <h2 className="text-lg font-semibold text-gray-800">
          Rate this answer
        </h2>

        <div className="flex justify-center space-x-2">
          {[1, 2, 3, 4, 5].map((r) => (
            <button
              key={r}
              onClick={() => setRating(r)}
              className={`w-10 h-10 rounded-full ${
                rating >= r ? "bg-yellow-400" : "bg-gray-200"
              }`}
            >
              ⭐
            </button>
          ))}
        </div>

        <textarea
          className="w-full border rounded-xl p-2 text-sm"
          rows="3"
          placeholder="Add a comment..."
          value={comments}
          onChange={(e) => setComments(e.target.value)}
        />

        <div className="flex justify-end space-x-2">
          <button
            onClick={onClose}
            className="px-3 py-1 rounded-xl bg-gray-200 text-gray-700"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={loading || !rating}
            className="px-4 py-1 rounded-xl bg-blue-600 text-white disabled:opacity-50"
          >
            {loading ? "Saving..." : "Submit"}
          </button>
        </div>
      </div>
    </div>
  );
}
