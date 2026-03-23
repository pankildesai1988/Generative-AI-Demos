import { Button } from "../ui/button";

export default { title: "UI/Button", component: Button };

export const Primary = { args: { children: "Primary", variant: "primary" } };
export const Secondary = { args: { children: "Secondary", variant: "secondary" } };
export const Accent = { args: { children: "Accent", variant: "accent" } };
export const Ghost = { args: { children: "Ghost", variant: "ghost" } };
export const Disabled = { args: { children: "Disabled", variant: "primary", disabled: true } };
