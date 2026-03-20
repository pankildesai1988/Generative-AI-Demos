import ErrorBanner from "../components/ErrorBanner";

export default { title: "Components/ErrorBanner", component: ErrorBanner };

export const Default = { args: { message: "Something went wrong. Please try again." } };
export const WithRetry = { args: { message: "Network error occurred.", onRetry: () => alert("Retrying...") } };
export const NoMessage = { args: { message: null } };
