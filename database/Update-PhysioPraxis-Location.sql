-- Run against MedActivities; does not modify existing backup files.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
UPDATE dbo.Activities
SET City = N'Budapest', Venue = N'PhysioPraxis – Szent István körút 1.',
    Latitude = 47.51299, Longitude = 19.04809
WHERE REPLACE(Venue, N' ', N'') LIKE N'PhysioPraxis%'
  AND (City <> N'Budapest' OR Venue <> N'PhysioPraxis – Szent István körút 1.'
       OR Latitude <> 47.51299 OR Longitude <> 19.04809);
SELECT @@ROWCOUNT AS UpdatedActivities;
UPDATE dbo.Practitioners
SET City = N'Budapest', Venue = N'PhysioPraxis – Szent István körút 1.'
WHERE REPLACE(Venue, N' ', N'') LIKE N'PhysioPraxis%'
  AND (City <> N'Budapest' OR Venue <> N'PhysioPraxis – Szent István körút 1.');
SELECT @@ROWCOUNT AS UpdatedPractitioners;
COMMIT TRANSACTION;
