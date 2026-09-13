namespace Desktop_Pacienskezelo;
public record Session(string Id, string UserName, string[] Roles);
public record Person(string Id, string Name) { public override string ToString() => Name; }
public record Patient(string Id, string Name, string TajNumber, DateOnly BirthDate,
    string? Email, string? Phone, string? Address, string? Notes, string? UserId);
public record ActivityItem(string Id, string Title, DateTime Date, string Description, string Category,
    string City, string Venue, string Status, double Latitude, double Longitude,
    bool CanEditFields, bool CanEditAssignments, bool IsAppointment, Person[] Patients, Person[] Practitioners);
public record AssignmentOptions(bool CanCreate, bool CanAssign, bool IsPractitioner,
    Person? OwnPractitioner, Person[] Patients, Person[] Practitioners);
public record AppointmentItem(string Id, string PatientId, string PractitionerId, string ActivityId,
    DateTime StartTime, int Status, string? Note);
public record DocumentItem(string Id, string Title, string FileName, int Size, DateTime CreatedAt, bool CanEdit);
public record NoteItem(string Id, string Text, string Author, DateTime CreatedAt, bool CanEdit);
public record PractitionerItem(string Id, string Name, bool BookingEnabled) { public override string ToString() => Name; }
