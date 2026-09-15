import { Grid } from "@mui/material";
import { useState } from "react";
import ActivityList from "./ActivityList";
import ActivityFilters, { type ActivityFilter } from "./ActivityFilters";
import { useActivityAccess } from "../../../lib/hooks/useActivityAccess";
import ActivityLogin from "../form/ActivityLogin";
export default function ActivityDashboard() {
  const [filter, setFilter] = useState<ActivityFilter>("all");
  const [category, setCategory] = useState("");
  const [date, setDate] = useState<[Date, Date] | null>(null);
  const [patientId, setPatientId] = useState("");
  const [practitionerId, setPractitionerId] = useState("");
  const session = useActivityAccess();
  const canFilterPeople =
    session.data?.roles.some((r) =>
      ["Admin", "AdmissionsOffice", "Practitioner"].includes(r),
    ) ?? false;
  const people = (items: ActivityPerson[]) =>
    [...new Map(items.map((p) => [p.id, p])).values()].sort((a, b) =>
      a.name.localeCompare(b.name, "hu"),
    );
  const patients = people(session.data?.patients ?? []);
  const practitioners = people(session.data?.practitioners ?? []);
  if (!session.authenticated) return <ActivityLogin />;
  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, md: 8 }}>
        <ActivityList
          filter={filter}
          date={date}
          category={category}
          patientId={patientId}
          practitionerId={practitionerId}
        />
      </Grid>
      <Grid size={{ xs: 12, md: 4 }}>
        <ActivityFilters
          category={category}
          onCategory={setCategory}
          filter={filter}
          date={date}
          onDate={setDate}
          onFilter={setFilter}
          canFilterPeople={canFilterPeople}
          patients={patients}
          practitioners={practitioners}
          patientId={patientId}
          practitionerId={practitionerId}
          onPatient={setPatientId}
          onPractitioner={setPractitionerId}
        />
      </Grid>
    </Grid>
  );
}
