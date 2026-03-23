const consoleBackend = {
  track(event) {
    console.info("[analytics]", event);
  },
};

let activeBackend = consoleBackend;

function normalizeMetadata(metadata) {
  return metadata && typeof metadata === "object" ? metadata : {};
}

export function setAnalyticsBackend(backend) {
  const previous = activeBackend;
  activeBackend =
    backend && typeof backend.track === "function" ? backend : consoleBackend;
  return previous;
}

export function createConsoleBackend(logger = console.info) {
  return {
    track(event) {
      logger("[analytics]", event);
    },
  };
}

export function trackEvent(category, action, label, metadata = {}) {
  const event = {
    category,
    action,
    label: label ?? null,
    metadata: normalizeMetadata(metadata),
    timestamp: new Date().toISOString(),
  };

  activeBackend.track(event);
  return event;
}
