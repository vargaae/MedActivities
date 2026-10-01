import { Grid } from "@mui/material";
import { useParams } from "react-router";
import { useActivities } from "../../../lib/hooks/useActivities";
import ActivityDetailsChat from "./ActivityDetailsChat";
import ActivityDetailsHeader from "./ActivityDetailsHeader";
import ActivityDetailsInfo from "./ActivityDetailsInfo";
import ActivityDetailsSidebar from "./ActivityDetailsSidebar";

export default function ActivityDetailsPage() {
    const {id} = useParams();
    const {activity, isLoadingActivity} = useActivities(id);

    if (isLoadingActivity) return <div>Loading activity...</div>;

    if (!activity) return <div>Activity not found</div>;
    
    return (
        <Grid container spacing={{ xs: 2, sm: 3 }}>
            <Grid size={{ xs: 12, lg: 8 }}>
                <ActivityDetailsHeader activity={activity}  />
                <ActivityDetailsInfo activity={activity}  />
                <ActivityDetailsChat activityId={activity.id} />
            </Grid>
            <Grid size={{ xs: 12, lg: 4 }}>
                <ActivityDetailsSidebar activity={activity} />
            </Grid>
        </Grid>
    )
}
