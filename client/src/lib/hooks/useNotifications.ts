import { useQuery } from "@tanstack/react-query";
import agent from "../api/agent";
import { useActivityAccess } from "./useActivityAccess";

export function useUnreadNotifications() {
  const session = useActivityAccess();
  return useQuery({
    queryKey: ["notifications", session.version, "unread-count"],
    queryFn: async ({ signal }) => (await agent.get<{ unreadCount: number }>("/notifications/unread-count", { signal })).data,
    enabled: session.authenticated,
    refetchInterval: 15_000,
    staleTime: 5_000,
  });
}
