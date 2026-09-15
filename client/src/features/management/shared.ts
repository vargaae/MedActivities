import axios from "axios";
export function errorText(error: unknown) {
  if (Array.isArray(error)) return error.flat().join(" ");
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    return typeof data === "string"
      ? data
      : (data?.message ??
          (data?.errors
            ? Object.values(data.errors).flat().join(" ")
            : "A művelet nem sikerült."));
  }
  return error instanceof Error ? error.message : "A művelet nem sikerült.";
}
export const roleLabels: Record<string, string> = {
  Admin: "Admin",
  AdmissionsOffice: "Felvételi iroda",
  Practitioner: "Kezelő",
  Patient: "Páciens",
};
export type ManagedUser = {
  id: string;
  name?: string;
  userName: string;
  email: string;
  roles: string[];
  disabled: boolean;
};
