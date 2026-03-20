/** @type { import('@storybook/react-vite').StorybookConfig } */
export default {
  stories: ["../src/stories/**/*.stories.@(js|jsx)"],
  framework: {
    name: "@storybook/react-vite",
    options: {},
  },
  addons: ["@storybook/addon-essentials"],
};
