import { useState } from "react";
import { Star } from "lucide-react";
import toast from "react-hot-toast";
import { submitFeedback } from "../../api/feedback";
import useFocusTrap from "../../hooks/useFocusTrap";
import useKeyboardNav from "../../hooks/useKeyboardNav";
import { trackEvent } from "../../analytics/tracker";

export default function FeedbackModal({ historyId, onClose }) {
  const [open, setOpen] = useState(!onClose);
  const [rating, setRating] = useState(0);
  const [hovered, setHovered] = useState(0);
  const [comment, setComment] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleClose = () => {
    setOpen(false);
    onClose?.();
  };

  useKeyboardNav("Escape", handleClose, open);
  const containerRef = useFocusTrap(open);

  const handleSubmit = async () => {
    if (rating === 0) {
      toast.error("Select a star rating first.");
      return;
    }
    setSubmitting(true);
    trackEvent("feedback", "submit", null, { historyId, rating });
    try {
      await submitFeedback({ historyId, rating, comment });
      toast.success("Feedback submitted — thank you!");
      trackEvent("feedback", "success", null, { historyId });
      handleClose();
    } catch {
      toast.error("Failed to submit feedback.");
      trackEvent("feedback", "error", null, { historyId });
    } finally {
      setSubmitting(false);
    }
  };

  if (!open) {
    return (
      <button
        onClick={() => setOpen(true)}
        className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 text-sm"
      >
        Feedback
      </button>
    );
  }

  return (
    <div
      className="fixed inset-0 bg-black/40 flex justify-center items-center z-50"
      role="dialog"
      aria-modal="true"
      aria-labelledby="feedback-title"
      onClick={handleClose}
    >
      <div
        ref={containerRef}
        onClick={(e) => e.stopPropagation()}
        className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-xl w-96"
      >
        <h4
          id="feedback-title"
          className="text-lg font-semibold mb-1 text-gray-900 dark:text-gray-100"
        >
          Rate this response
        </h4>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
          How helpful was this answer?
        </p>

        <div className="flex gap-2 mb-4">
          {[1, 2, 3, 4, 5].map((star) => (
            <button
              key={star}
              onMouseEnter={() => setHovered(star)}
              onMouseLeave={() => setHovered(0)}
              onClick={() => setRating(star)}
              className="transition-transform hover:scale-110"
              aria-label={`Rate ${star} star`}
            >
              <Star
                size={28}
                className={
                  star <= (hovered || rating)
                    ? "fill-yellow-400 text-yellow-400"
                    : "text-gray-300 dark:text-gray-600"
                }
              />
            </button>
          ))}
        </div>

        <textarea
          rows="3"
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          className="w-full border rounded-lg p-2 mb-4 text-sm dark:bg-gray-700 dark:border-gray-600 dark:text-gray-100"
          placeholder="Optional comment…"
        />

        <div className="flex justify-end gap-2">
          <button
            onClick={handleClose}
            className="px-3 py-1.5 bg-gray-200 dark:bg-gray-600 dark:text-gray-100 rounded hover:bg-gray-300 text-sm"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={submitting}
            className="px-3 py-1.5 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 text-sm"
          >
            {submitting ? "Submitting…" : "Submit"}
          </button>
        </div>
      </div>
    </div>
  );
}
