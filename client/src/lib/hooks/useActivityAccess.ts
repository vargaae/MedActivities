import { useSyncExternalStore } from 'react';
import { useQuery } from '@tanstack/react-query';
import agent from '../api/agent';
import { getActivityToken, getActivitySessionVersion, subscribeActivitySession } from '../api/activitySession';

export type ActivityAssignmentOptions = {
    userName: string;
    userId: string;
    roles: string[];
    canCreate: boolean;
    canAssign: boolean;
    isPractitioner: boolean;
    ownPractitioner: ActivityPerson | null;
    patients: ActivityPerson[];
    practitioners: ActivityPerson[];
};
export function useActivityAccess() {
    const version = useSyncExternalStore(subscribeActivitySession, getActivitySessionVersion);
    const authenticated = !!getActivityToken();
    const query = useQuery({
        queryKey: ['activity-assignment-options', version],
        queryFn: async ({ signal }) => (await agent.get<ActivityAssignmentOptions>('/activities/assignment-options', { signal })).data,
        enabled: authenticated,
        retry: false,
    });
    return { ...query, authenticated, version };
}
