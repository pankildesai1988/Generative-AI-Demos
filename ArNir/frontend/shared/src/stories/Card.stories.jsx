import { Card, CardHeader, CardContent } from "../ui/card";

export default { title: "UI/Card", component: Card };

export const Default = {
  render: () => (
    <Card>
      <CardHeader>Card Title</CardHeader>
      <CardContent>This is the card content area.</CardContent>
    </Card>
  ),
};
