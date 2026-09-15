import { useSyncExternalStore } from 'react';
import { subscribeActivitySession, getActivitySessionVersion, getActivityToken } from '../api/activitySession';
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import agent from "../api/agent";
import { useLocation } from "react-router";

export const useActivities = (id?: string) => {
    const queryClient = useQueryClient();
    const sessionVersion = useSyncExternalStore(subscribeActivitySession, getActivitySessionVersion);
    const authenticated = !!getActivityToken();
    const location = useLocation();

    const { isLoading: isPending, data: activities, isError } = useQuery({
        queryKey: ['activities', 'list', sessionVersion],
        queryFn: async ({ signal }) => {
            const response = await agent.get<Activity[]>('/activities', { signal });
            return response.data;
        },
        enabled: authenticated && !id && location.pathname === '/activities'
    });

    const { isLoading: isLoadingActivity, data: activity } = useQuery<Activity>({
        queryKey: ['activities', 'detail', id, sessionVersion],
        queryFn: async ({ signal }) => {
            const response = await agent.get<Activity>(`/activities/${id}`, { signal });
            return response.data;
        },
        enabled: authenticated && !!id
    });

    const updateActivity = useMutation({
        mutationFn: async (activity: ActivityWrite) => {
            await agent.put('/activities', activityWritePayload(activity));
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['booking'] });
            await queryClient.invalidateQueries({
                queryKey: ['activities']
            })
        }
    });

    const createActivity = useMutation({
        mutationFn: async (activity: ActivityWrite) => {
            const response = await agent.post<string>('/activities', activityWritePayload(activity));
            return response.data;
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['booking'] });
            await queryClient.invalidateQueries({
                queryKey: ['activities']
            })
        }
    })

    const deleteActivity = useMutation({
        mutationFn: async (id: string) => {
            await agent.delete(`/activities/${id}`);
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ['booking'] });
            await queryClient.invalidateQueries({
                queryKey: ['activities', 'list']
            })
        }
    })

    return {
        activities, isError,
        isPending,
        updateActivity,
        createActivity,
        deleteActivity,
        activity,
        isLoadingActivity
    }
}
function activityWritePayload(a: ActivityWrite) {
    return {
        status: a.status, id: a.id, title: a.title, date: a.date instanceof Date ? localDateTime(a.date) : a.date, description: a.description,
        category: a.category, city: a.city, venue: a.venue,
        latitude: a.latitude, longitude: a.longitude,
        patientId: a.patientId, practitionerIds: a.practitionerIds
    };
}
function localDateTime(date: Date) {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth()+1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:00`;
}
