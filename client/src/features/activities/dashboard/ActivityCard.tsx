import { AccessTime, Place, ArrowForwardRounded } from "@mui/icons-material";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Typography,
} from "@mui/material";
import { Link } from "react-router";
import ActivityPeople from "../details/ActivityPeople";
import { categoryImage } from "./categoryImage";

export default function ActivityCard({ activity }: { activity: Activity }) {
  const image = categoryImage(activity.category);
  const status =
    (
      {
        Scheduled: "Tervezett",
        Completed: "Befejezett",
        Cancelled: "Lemondva",
        NoShow: "Nem jelent meg",
      } as Record<string, string>
    )[activity.status] ?? activity.status;
  return (
    <Card
      variant="outlined"
      sx={{
        borderRadius: 4,
        overflow: "hidden",
        borderColor: "#dce8e4",
        transition: "box-shadow .2s",
        a: { color: "inherit", textDecoration: "none" },
        "&:hover": { boxShadow: "0 12px 32px #1c5a4a18" },
      }}
    >
      <Link to={`/activities/${activity.id}`}>
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "1fr", sm: "180px 1fr" },
          }}
        >
          <Box
            component="img"
            src={image.src}
            alt=""
            loading="lazy"
            sx={{
              width: "100%",
              height: { xs: 150, sm: "100%" },
              minHeight: { sm: 210 },
              objectFit: "cover",
            }}
          />
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap", mb: 1.5 }}>
              <Chip
                label={image.label}
                size="small"
                sx={{ bgcolor: "#e5f3ee", color: "#256b59" }}
              />
              <Chip
                label={activity.isCancelled ? "Lemondva" : status}
                size="small"
                variant="outlined"
                color={activity.isCancelled ? "error" : "default"}
              />
            </Box>
            <Typography
              variant="h6"
              component="h2"
              sx={{ fontWeight: 700, mb: 1 }}
            >
              {activity.title}
            </Typography>
            <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
              <AccessTime fontSize="small" color="action" />
              <Typography variant="body2">
                {new Date(activity.date).toLocaleString("hu-HU", {
                  dateStyle: "medium",
                  timeStyle: "short",
                })}
              </Typography>
            </Box>
            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
              <Place fontSize="small" color="action" />
              <Typography variant="body2">
                {activity.city} · {activity.venue}
              </Typography>
            </Box>
          </CardContent>
        </Box>
        <Box
          sx={{
            px: 3,
            py: 2,
            bgcolor: "#f6faf8",
            borderTop: "1px solid #e5eeea",
          }}
        >
          <ActivityPeople activity={activity} />
        </Box>
        <Box
          sx={{
            px: 3,
            py: 2,
            display: "flex",
            alignItems: "center",
            gap: 2,
            flexWrap: "wrap",
          }}
        >
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              flex: 1,
              minWidth: 180,
              display: "-webkit-box",
              WebkitLineClamp: 2,
              WebkitBoxOrient: "vertical",
              overflow: "hidden",
            }}
          >
            {activity.description}
          </Typography>
          <Button
            component={Link}
            to={`/activities/${activity.id}`}
            variant="contained"
            endIcon={<ArrowForwardRounded />}
            sx={{
              borderRadius: 2,
              backgroundColor: "#a0c9bf",
              "&:hover": { backgroundColor: "#769b93" },
            }}
          >
            Részletek
          </Button>
        </Box>
      </Link>
    </Card>
  );
}
