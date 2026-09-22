import { useQuery } from "@tanstack/react-query";
import agent from "../api/agent";
import { useActivityAccess } from "./useActivityAccess";

export type ActivitySearch = { patientId?: string; practitionerId?: string; category?: string; from?: string; to?: string };
export function useActivityPages(search: ActivitySearch, page: number, pageSize: number, enabled = true) {
  const session = useActivityAccess();
  return useQuery({
    queryKey: ["activities", "list", session.version, search, page, pageSize],
    queryFn: async ({ signal }) => (await agent.get<{ items: Activity[]; totalCount: number; page: number; pageSize: number }>(
      "/activities/page", { params: { ...search, page, pageSize }, signal })).data,
    enabled: enabled && session.authenticated,
    staleTime: 60_000,
    gcTime: 5 * 60_000,
  });
}
