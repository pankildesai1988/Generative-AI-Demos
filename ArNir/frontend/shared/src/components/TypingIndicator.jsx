import { motion } from "framer-motion";

export default function TypingIndicator() {
  return (
    <div className="flex justify-start">
      <motion.div
        className="flex items-center gap-1.5 px-4 py-3 bg-gray-100 rounded-2xl"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.3 }}
      >
        {[0, 0.15, 0.3].map((delay, i) => (
          <motion.div
            key={i}
            className="w-2 h-2 bg-primary-500 rounded-full"
            animate={{ y: [0, -6, 0] }}
            transition={{
              duration: 0.5,
              repeat: Infinity,
              ease: "easeInOut",
              delay,
            }}
          />
        ))}
        <span className="ml-2 text-sm text-gray-500">Thinking...</span>
      </motion.div>
    </div>
  );
}
