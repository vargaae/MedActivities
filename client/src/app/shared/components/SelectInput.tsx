import { FormControl, FormHelperText, InputLabel, MenuItem, Select, type SelectProps } from "@mui/material";
import { type FieldValues, useController, type UseControllerProps } from "react-hook-form"

type Props<T extends FieldValues> = {
    label: string
    name: keyof T
    items: {text: string, value: string}[];
} & UseControllerProps<T> & Partial<SelectProps>

export default function AppSelectInput<T extends FieldValues>(props: Props<T>) {
    const {fieldState, field} = useController({...props});

    return (
        <FormControl fullWidth error={!!fieldState.error}>
            <InputLabel id={`${String(props.name)}-label`}>{props.label}</InputLabel>
            <Select
                id={String(props.name)}
                name={String(props.name)}
                labelId={`${String(props.name)}-label`}
                value={field.value || ''}
                label={props.label}
                onChange={field.onChange}
            >
                {props.items.map((item, index) => (
                    <MenuItem value={item.value} key={index}>{item.text}</MenuItem>
                ))}
            </Select>
            <FormHelperText>{fieldState.error?.message}</FormHelperText>
        </FormControl>
    )
}
