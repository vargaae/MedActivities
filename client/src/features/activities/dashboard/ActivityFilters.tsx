import {
  Box,
  Button,
  MenuItem,
  MenuList,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import Calendar from "react-calendar";
import "react-calendar/dist/Calendar.css";
import { categoryOptions } from "../form/categoryOptions";
import PersonSearch from "../../../app/shared/components/PersonSearch";
export type ActivityFilter = "all" | "future" | "history";
type Props = {
  filter: ActivityFilter;
  date: [Date, Date] | null;
  category: string;
  onFilter: (value: ActivityFilter) => void;
  onDate: (value: [Date, Date] | null) => void;
  onCategory: (value: string) => void;
  canFilterPeople: boolean;
  patients: ActivityPerson[];
  practitioners: ActivityPerson[];
  patientId: string;
  practitionerId: string;
  onPatient: (id: string) => void;
  onPractitioner: (id: string) => void;
};
export default function ActivityFilters({
  filter,
  date,
  category,
  onFilter,
  onDate,
  onCategory,
  canFilterPeople,
  patients,
  practitioners,
  patientId,
  practitionerId,
  onPatient,
  onPractitioner,
}: Props) {
  return (
    <Box sx={{ display: "grid", gap: 3 }}>
      <Paper sx={{ p: 3, borderRadius: 3 }}>
        <Typography variant="h6">Szűrők</Typography>
        <MenuList>
          {(
            [
              { value: "all", label: "Összes esemény" },
              { value: "future", label: "Közelgő események" },
              { value: "history", label: "Korábbi események" },
            ] as const
          ).map((item) => (
            <MenuItem
              key={item.value}
              selected={filter === item.value}
              onClick={() => onFilter(item.value)}
            >
              {item.label}
            </MenuItem>
          ))}
        </MenuList>
        <TextField
          fullWidth
          select
          label="Kategória"
          value={category}
          onChange={(e) => onCategory(e.target.value)}
        >
          <MenuItem value="">Minden kategória</MenuItem>
          {categoryOptions.map((c) => (
            <MenuItem key={c.value} value={c.value}>
              {c.text}
            </MenuItem>
          ))}
        </TextField>
        {canFilterPeople && (
          <>
            <Box sx={{ display: "grid", gap: 2, mt: 2 }}>
              <PersonSearch
                label="Páciens szerinti szűrés"
                options={patients}
                value={patientId}
                onChange={onPatient}
              />
              <PersonSearch
                label="Kezelőorvos szerinti szűrés"
                options={practitioners}
                value={practitionerId}
                onChange={onPractitioner}
              />
            </Box>
          </>
        )}
      </Paper>
      <Paper
        sx={{
          p: 3,
          borderRadius: 3,
          "& .react-calendar": { width: "100%", border: 0 },
        }}
      >
        <Typography variant="h6" sx={{ mb: 2 }}>
          Dátum: időintervallum
        </Typography>
        <Typography variant="body2">
          Válaszd ki a kezdő-, majd a zárónapot.
        </Typography>
        <Calendar
          locale="hu-HU"
          selectRange
          value={date}
          onChange={(value) => {
            if (
              Array.isArray(value) &&
              value[0] instanceof Date &&
              value[1] instanceof Date
            )
              onDate([value[0], value[1]]);
          }}
        />
        {date && (
          <Typography>
            {date[0].toLocaleDateString("hu-HU")} –{" "}
            {date[1].toLocaleDateString("hu-HU")}
          </Typography>
        )}
        {date && (
          <Button onClick={() => onDate(null)}>Dátumszűrés törlése</Button>
        )}
        <Button
          onClick={() => {
            onFilter("all");
            onCategory("");
            onDate(null);
            onPatient("");
            onPractitioner("");
          }}
        >
          Összes szűrő törlése
        </Button>
      </Paper>
    </Box>
  );
}
