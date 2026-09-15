import { useInfiniteQuery } from '@tanstack/react-query';
import { useSyncExternalStore } from 'react';
import agent from '../api/agent';
import { getActivitySessionVersion, subscribeActivitySession, getActivityToken } from '../api/activitySession';

export type ActivitySearch = { patientId?: string; practitionerId?: string; category?: string; from?: string; to?: string };
export function useActivityPages(search: ActivitySearch, enabled = true) {
    const version = useSyncExternalStore(subscribeActivitySession, getActivitySessionVersion);
    return useInfiniteQuery({
        queryKey: ['activities', 'list', version, search],
        initialPageParam: 1,
        queryFn: async ({ pageParam, signal }) => (await agent.get<{ items: Activity[]; nextPage: number | null }>('/activities/page', {
            params: { ...search, page: pageParam }, signal,
        })).data,
        getNextPageParam: page => page.nextPage ?? undefined,
        enabled: enabled && !!getActivityToken(),
        staleTime: 60_000,
        gcTime: 5 * 60_000,
    });
}
