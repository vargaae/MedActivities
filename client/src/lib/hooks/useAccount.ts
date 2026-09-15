import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useActivityAccess } from "./useActivityAccess";
import { changeSession } from "../api/changeSession";

export type CurrentUser = {
  id: string;
  displayName: string;
  imageUrl?: string;
  roles: string[];
  canCreate: boolean;
};

export function useAccount() {
  const access = useActivityAccess();
  const cache = useQueryClient();
  const currentUser: CurrentUser | null =
    access.authenticated && access.data
      ? {
          id: access.data.userId,
          displayName: access.data.userName,
          roles: access.data.roles,
          canCreate: access.data.canCreate,
        }
      : null;
  const logoutUser = useMutation({
    mutationFn: () => changeSession(cache),
    onSettled: async () => {
      sessionStorage.setItem(
        "medactivities.logout-toast",
        "Sikeres kijelentkezés",
      );
      window.location.assign("/");
    },
  });
  return { currentUser, logoutUser };
}
