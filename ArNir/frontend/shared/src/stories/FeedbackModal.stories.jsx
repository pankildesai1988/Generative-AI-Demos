import FeedbackModal from "../components/FeedbackModal";

export default { title: "Components/FeedbackModal", component: FeedbackModal };

export const Default = {
  args: {
    historyId: 1,
    onClose: () => console.log("Close modal"),
  },
};
