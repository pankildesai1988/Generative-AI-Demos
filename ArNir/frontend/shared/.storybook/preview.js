import React from "react";
import { ThemeProvider } from "../src/theme/themeContext";

/** @type { import('@storybook/react').Preview } */
export default {
  decorators: [
    (Story) => (
      <ThemeProvider demoType="healthcare">
        <Story />
      </ThemeProvider>
    ),
  ],
  parameters: {
    controls: { matchers: { color: /(background|color)$/i, date: /Date$/i } },
  },
};
