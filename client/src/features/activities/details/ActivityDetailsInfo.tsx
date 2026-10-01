import ActivityPeople from "./ActivityPeople";
import { CalendarToday, Info, Place } from "@mui/icons-material";
import { Box, Button, Divider, Grid, Paper, Typography } from "@mui/material";
import { formatDate } from "../../../lib/util/util";
import { useState } from "react";
import MapComponent from "../../../app/shared/components/MapComponent";

type Props = {
  activity: Activity;
};

export default function ActivityInfo({ activity }: Props) {
  const [mapOpen, setMapOpen] = useState(true);
  return (
    <Paper sx={{ mb: 2 }}>
      <Box sx={{ px: 2 }}>
        <ActivityPeople activity={activity} />
      </Box>
      <Divider />

      <Grid container alignItems="center" px={{ xs: 1.5, sm: 2 }} py={1}>
        <Grid size={{ xs: 2, sm: 1 }}>
          <Info color="info" fontSize="large" />
        </Grid>
        <Grid size={{ xs: 10, sm: 11 }} minWidth={0}>
          <Typography sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}>{activity.description}</Typography>
        </Grid>
      </Grid>
      <Divider />
      <Grid container alignItems="center" px={{ xs: 1.5, sm: 2 }} py={1}>
        <Grid size={{ xs: 2, sm: 1 }}>
          <CalendarToday color="info" fontSize="large" />
        </Grid>
        <Grid size={{ xs: 10, sm: 11 }} minWidth={0}>
          <Typography>{formatDate(activity.date)}</Typography>
        </Grid>
      </Grid>
      <Divider />

      <Grid container alignItems="center" px={{ xs: 1.5, sm: 2 }} py={1}>
        <Grid size={{ xs: 2, sm: 1 }}>
          <Place color="info" fontSize="large" />
        </Grid>
        <Grid
          size={{ xs: 10, sm: 11 }}
          display="flex"
          justifyContent="space-between"
          alignItems="center"
          gap={1}
          minWidth={0}
          sx={{ flexWrap: { xs: "wrap", sm: "nowrap" } }}
        >
          <Typography sx={{ minWidth: 0, overflowWrap: "anywhere", flex: 1 }}>
            {activity.venue}, {activity.city}
          </Typography>
          <Button onClick={() => setMapOpen(!mapOpen)}>
            {mapOpen ? "Hide Map" : "Show Map"}
          </Button>
        </Grid>
      </Grid>
      {mapOpen && (
        <Box sx={{ height: { xs: 280, sm: 400 }, zIndex: 1000, display: "block" }}>
          <MapComponent
            position={[activity.latitude, activity.longitude]}
            venue={activity.venue}
          />
        </Box>
      )}
    </Paper>
  );
}
