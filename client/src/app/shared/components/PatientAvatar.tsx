import { Avatar } from '@mui/material';
export default function PatientAvatar({ name }: { name: string }) {
    return <Avatar src="/images/patient-placeholder.svg" alt={name + ' – helyettesítő profilkép'} sx={{ width: 56, height: 56, bgcolor: '#e3f1ed' }} />;
}
