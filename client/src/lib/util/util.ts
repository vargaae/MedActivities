import { format, formatDistanceToNow, isValid, parseISO } from "date-fns";
import z from "zod";

export function formatDate(date: Date | string) {
  return format(
    date instanceof Date ? date : new Date(date),
    "yyyy.MM.dd. HH:mm",
  );
}

export function formatDateOnly(value: string | Date | null | undefined) {
  if (value == null || (typeof value === "string" && !value.trim())) return "—";
  const date = value instanceof Date ? value : parseISO(value);
  return isValid(date) ? format(date, "yyyy.MM.dd.") : "—";
}

export function formatTaj(value: string) {
  const digits = value.replace(/\D/g, "");
  return digits.replace(/(\d{3})(?=\d)/g, "$1 ").trim();
}

export const requiredString = (fieldName: string) =>
  z
    .string({ error: `${fieldName} megadása kötelező` })
    .min(1, { error: `${fieldName} megadása kötelező` });

export const timeAgo = (date: Date) => {
  return formatDistanceToNow(date) + " ago";
};
