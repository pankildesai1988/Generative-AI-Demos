import type { AnalyticsEvent, AnalyticsBackend } from "../types";

const consoleBackend: AnalyticsBackend = {
  track(event: AnalyticsEvent) {
    console.info("[analytics]", event);
  },
};

let activeBackend: AnalyticsBackend = consoleBackend;

function normalizeMetadata(metadata: unknown): Record<string, unknown> {
  return metadata && typeof metadata === "object" ? (metadata as Record<string, unknown>) : {};
}

export function setAnalyticsBackend(backend: AnalyticsBackend): AnalyticsBackend {
  const previous = activeBackend;
  activeBackend =
    backend && typeof backend.track === "function" ? backend : consoleBackend;
  return previous;
}

export function createConsoleBackend(logger: (...args: unknown[]) => void = console.info): AnalyticsBackend {
  return {
    track(event: AnalyticsEvent) {
      logger("[analytics]", event);
    },
  };
}

export function trackEvent(
  category: string,
  action: string,
  label?: string,
  metadata: Record<string, unknown> = {},
): AnalyticsEvent {
  const event: AnalyticsEvent = {
    category,
    action,
    label: label ?? null,
    metadata: normalizeMetadata(metadata),
    timestamp: new Date().toISOString(),
  };

  activeBackend.track(event);
  return event;
}
