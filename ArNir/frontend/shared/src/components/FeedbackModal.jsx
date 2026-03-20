import { useState } from "react";
import { Star, X } from "lucide-react";
import { submitFeedback } from "../api/feedback";
import toast from "react-hot-toast";

/**
 * Feedback modal with 5-star rating + comment.
 * Actually calls the backend API (unlike the original version).
 */
export default function FeedbackModal({ historyId, onClose }) {
  const [rating, setRating] = useState(0);
  const [hoverRating, setHoverRating] = useState(0);
  const [comment, setComment] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    if (rating === 0) {
      toast.error("Please select a rating.");
      return;
    }

    setSubmitting(true);
    try {
      await submitFeedback({
        historyId,
        rating,
        comment: comment.trim() || null,
      });
      toast.success("Thank you for your feedback!");
      onClose();
    } catch (err) {
      toast.error("Failed to submit feedback. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex justify-center items-center z-50">
      <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-xl w-96 relative">
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
        >
          <X size={18} />
        </button>

        <h4 className="text-lg font-semibold mb-4 dark:text-gray-100">Rate this Response</h4>

        {/* Star Rating */}
        <div className="flex justify-center gap-1 mb-4">
          {[1, 2, 3, 4, 5].map((star) => (
            <button
              key={star}
              onClick={() => setRating(star)}
              onMouseEnter={() => setHoverRating(star)}
              onMouseLeave={() => setHoverRating(0)}
              className="transition-transform hover:scale-110"
            >
              <Star
                size={32}
                className={
                  star <= (hoverRating || rating)
                    ? "fill-amber-400 text-amber-400"
                    : "text-gray-300 dark:text-gray-600"
                }
              />
            </button>
          ))}
        </div>
        <p className="text-center text-sm text-gray-500 dark:text-gray-400 mb-3">
          {rating > 0
            ? ["", "Poor", "Fair", "Good", "Very Good", "Excellent"][rating]
            : "Select a rating"}
        </p>

        {/* Comment */}
        <textarea
          rows="3"
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          className="w-full border dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 rounded-lg p-2.5 text-sm mb-4 outline-none focus:ring-2 focus:ring-primary-300 dark:focus:ring-primary-600 placeholder:text-gray-400 dark:placeholder:text-gray-500"
          placeholder="Optional: share your thoughts..."
        />

        {/* Actions */}
        <div className="flex justify-end gap-2">
          <button
            onClick={onClose}
            className="px-4 py-2 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 text-sm transition"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={submitting || rating === 0}
            className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50 text-sm transition"
          >
            {submitting ? "Submitting..." : "Submit"}
          </button>
        </div>
      </div>
    </div>
  );
}
