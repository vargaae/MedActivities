import { z } from "zod";
import { requiredString } from "../util/util";

export const activitySchema = z.object({
  title: requiredString("Title"),
  description: requiredString("Description"),
  category: requiredString("Category"),
  date: z.coerce
    .date({ error: "Dátum megadása kötelező" })
    .refine((d) => d > new Date(2000, 0, 1), { message: "A dátum túl régi" }),
  location: z.object({
    venue: requiredString("Venue"),
    city: z.string().optional(),
    latitude: z.coerce.number(),
    longitude: z.coerce.number(),
  }),
});

export type ActivitySchema = z.input<typeof activitySchema>;
