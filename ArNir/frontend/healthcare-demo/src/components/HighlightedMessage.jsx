import { MessageBubble } from "@arnir/shared";
import { extractMedicalTerms } from "../utils/medicalTerms";

const toneClasses = {
  conditions: "bg-rose-50 text-rose-700 dark:bg-rose-900/30 dark:text-rose-300",
  drugs: "bg-sky-50 text-sky-700 dark:bg-sky-900/30 dark:text-sky-300",
  dosages: "bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300",
};

export default function HighlightedMessage({ message }) {
  const terms = message.role === "assistant" ? extractMedicalTerms(message.text) : [];

  return (
    <div className="space-y-2">
      <MessageBubble role={message.role} text={message.text} isError={message.isError} />
      {terms.length > 0 && (
        <div className="flex flex-wrap gap-2 ml-1">
          {terms.map((term) => (
            <span
              key={`${term.category}:${term.label}`}
              className={`inline-flex items-center rounded-full px-2.5 py-1 text-[11px] font-medium ${toneClasses[term.category]}`}
            >
              {term.label}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
