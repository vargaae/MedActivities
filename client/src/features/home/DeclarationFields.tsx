import {
  Alert,
  Box,
  Checkbox,
  FormControlLabel,
  TextField,
  Typography,
} from "@mui/material";
import { Link } from "react-router";

export type DeclarationPolicy = {
  version: string;
  title: string;
  highlight: string;
  text: string;
  requiresSignature: boolean;
};
export type DeclarationAcceptance = {
  accepted: boolean;
  version: string;
  signedName: string;
};

export default function DeclarationFields({
  policy,
  value,
  onChange,
  disabled,
}: {
  policy: DeclarationPolicy;
  value: DeclarationAcceptance;
  onChange: (value: DeclarationAcceptance) => void;
  disabled: boolean;
}) {
  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        border: 1,
        borderColor: "divider",
        borderRadius: 2,
        p: 2,
      }}
    >
      <Typography variant="h6">{policy.title}</Typography>
      <Alert severity="info">
        <strong>{policy.highlight}</strong>
      </Alert>
      <Typography variant="body2" sx={{ whiteSpace: "pre-line" }}>
        {policy.text}
      </Typography>
      <Alert severity="warning">
        Ezzel elfogadja a GDPR szerinti adatkezelési feltételeket, és hozzájárul
        személyes és egészségügyi adatai EgészségÚt alkalmazásban történő
        korlátozott kezeléséhez.{" "}
        <Link
          to="https://szglegal.hu/az-egeszsegugyi-adatkezelesek-bananheja/"
          target="_blank"
          rel="noopener noreferrer"
        >
          Részletes adatkezelési tájékoztató
        </Link>
      </Alert>
      {policy.requiresSignature && (
        <TextField
          label="Nyilatkozattevő teljes neve"
          required
          autoComplete="name"
          disabled={disabled}
          value={value.signedName}
          slotProps={{ htmlInput: { maxLength: 100, minLength: 3 } }}
          helperText="Személyes, naplózott elfogadás; nem minősített elektronikus aláírás."
          onChange={(e) =>
            onChange({
              ...value,
              signedName: e.target.value,
              version: policy.version,
            })
          }
        />
      )}
      <FormControlLabel
        control={
          <Checkbox
            required
            disabled={disabled}
            checked={value.accepted && value.version === policy.version}
            onChange={(e) =>
              onChange({
                ...value,
                accepted: e.target.checked,
                version: policy.version,
              })
            }
          />
        }
        label={
          policy.requiresSignature
            ? "Elolvastam a titoktartási nyilatkozatot, és személyesen vállalom a benne foglalt kötelezettségeket."
            : "Elolvastam és megértettem az adatkezelési nyilatkozatot, és elfogadom a fent leírt, ellátási célú adatfelhasználást."
        }
      />
      <Typography variant="caption" color="text.secondary">
        Verzió: {policy.version}
      </Typography>
    </Box>
  );
}
