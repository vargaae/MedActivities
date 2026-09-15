import { categoryOptions } from "../form/categoryOptions";
export function categoryImage(category: string) {
  const option = categoryOptions.find(
    (c) => c.value === category || c.text === category,
  );
  const key =
    option?.value === "Időpontfoglalás"
      ? "booking"
      : (option?.value ?? "examination");
  return {
    src: `/images/categories/${key}.svg`,
    label: option?.text ?? category,
  };
}
