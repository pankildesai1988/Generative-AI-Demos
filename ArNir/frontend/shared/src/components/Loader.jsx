import { motion } from "framer-motion";

export default function Loader() {
  return (
    <motion.div
      className="flex items-center justify-center gap-2 text-primary-600"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.3 }}
    >
      <motion.div
        className="w-2 h-2 bg-primary-600 rounded-full"
        animate={{ y: [0, -6, 0] }}
        transition={{ duration: 0.5, repeat: Infinity, ease: "easeInOut", delay: 0 }}
      />
      <motion.div
        className="w-2 h-2 bg-primary-600 rounded-full"
        animate={{ y: [0, -6, 0] }}
        transition={{ duration: 0.5, repeat: Infinity, ease: "easeInOut", delay: 0.15 }}
      />
      <motion.div
        className="w-2 h-2 bg-primary-600 rounded-full"
        animate={{ y: [0, -6, 0] }}
        transition={{ duration: 0.5, repeat: Infinity, ease: "easeInOut", delay: 0.3 }}
      />
    </motion.div>
  );
}
