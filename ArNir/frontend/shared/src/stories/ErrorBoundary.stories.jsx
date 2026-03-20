import ErrorBoundary from "../components/ErrorBoundary";

function BrokenComponent() {
  throw new Error("Test error for Storybook");
}

export default { title: "Components/ErrorBoundary", component: ErrorBoundary };

export const WithError = {
  render: () => (
    <ErrorBoundary>
      <BrokenComponent />
    </ErrorBoundary>
  ),
};

export const WithChildren = {
  render: () => (
    <ErrorBoundary>
      <p>This content renders normally.</p>
    </ErrorBoundary>
  ),
};
