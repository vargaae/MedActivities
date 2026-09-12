type ActivityPerson = { id: string; name: string };

type Activity = {
    id: string
    title: string
    date: Date
    description: string
    category: string
    isCancelled: boolean
    city: string
    venue: string
    latitude: number
    longitude: number
    canEditFields: boolean
    canEditAssignments: boolean
    isAppointment: boolean
    status: string
    patients: ActivityPerson[]
    practitioners: ActivityPerson[]
}

type LocationIQSuggestion = {
    place_id: string
    osm_id: string
    osm_type: string
    licence: string
    lat: string
    lon: string
    boundingbox: string[]
    class: string
    type: string
    display_name: string
    display_place: string
    display_address: string
    address: LocationIQAddress
}

type LocationIQAddress = {
    name: string
    house_number: string
    road: string
    suburb?: string
    town?: string
    village?: string
    city?: string
    county: string
    state: string
    postcode: string
    country: string
    country_code: string
    neighbourhood?: string
}
type ActivityWrite = {
    id?: string;
    title: string;
    date: Date | string;
    description: string;
    category: string;
    city: string;
    venue: string;
    latitude: number;
    longitude: number;
    patientId?: string;
    practitionerIds?: string[];
};