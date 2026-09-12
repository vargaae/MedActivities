import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import agent from "../api/agent";
import { useLocation } from "react-router";

export const useActivities = (id?: string) => {
    const queryClient = useQueryClient();
    const location = useLocation();

    const { isLoading: isPending, data: activities } = useQuery({
        queryKey: ['activities'],
        queryFn: async ({ signal }) => {
            const response = await agent.get<Activity[]>('/activities', { signal });
            return response.data;
        },
        enabled: !id && location.pathname === '/activities'
    });

    const { isLoading: isLoadingActivity, data: activity } = useQuery<Activity>({
        queryKey: ['activities', id],
        queryFn: async ({ signal }) => {
            const response = await agent.get<Activity>(`/activities/${id}`, { signal });
            return response.data;
        },
        enabled: !!id
    });

    const updateActivity = useMutation({
        mutationFn: async (activity: Activity) => {
            await agent.put('/activities', activityWritePayload(activity));
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({
                queryKey: ['activities']
            })
        }
    });

    const createActivity = useMutation({
        mutationFn: async (activity: Activity) => {
            const response = await agent.post('/activities', activityWritePayload(activity));
            return response.data;
        },
        onSuccess: async () => {
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
            await queryClient.invalidateQueries({
                queryKey: ['activities']
            })
        }
    })

    return {
        activities,
        isPending,
        updateActivity,
        createActivity,
        deleteActivity,
        activity,
        isLoadingActivity
    }
}
// A névlisták válaszadatok: ne küldjük vissza őket entitásként mentéskor.
function activityWritePayload(a: Activity) {
    return {
        id: a.id, title: a.title, date: a.date, description: a.description,
        category: a.category, city: a.city, venue: a.venue,
        latitude: a.latitude, longitude: a.longitude, isCancelled: a.isCancelled ?? false
    };
}