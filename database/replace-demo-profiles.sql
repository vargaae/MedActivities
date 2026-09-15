-- Fiktív magyar mintaprofilok. Futtatás előtt teljes BACKUP szükséges.
-- Csak egeszsegut-demo-v1- prefixű profilokat töröl. Más rekordokat megtart.
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF EXISTS(SELECT 1 FROM Patients WHERE Id LIKE 'hu-sample-v2-%')
    THROW 51000, 'A magyar mintakészlet már létezik. A csere nem futtatható újra.', 1;
CREATE TABLE #P (N int PRIMARY KEY, Id nvarchar(128), Name nvarchar(100), Taj nvarchar(9), Birth date, Email nvarchar(256), Address nvarchar(300));
INSERT #P VALUES
(1,N'hu-sample-v2-patient-1',N'Nagy Anna',N'810010004','1960-01-01',N'paciens1@example.invalid',N'Budapest, Petőfi Sándor utca 3.'),
(2,N'hu-sample-v2-patient-2',N'Kovács Péter',N'810010011','1962-02-02',N'paciens2@example.invalid',N'Debrecen, Kossuth Lajos utca 4.'),
(3,N'hu-sample-v2-patient-3',N'Tóth Eszter',N'810010028','1964-03-03',N'paciens3@example.invalid',N'Szeged, Arany János utca 5.'),
(4,N'hu-sample-v2-patient-4',N'Szabó Gábor',N'810010035','1966-04-04',N'paciens4@example.invalid',N'Pécs, Béke utca 6.'),
(5,N'hu-sample-v2-patient-5',N'Horváth Júlia',N'810010042','1968-05-05',N'paciens5@example.invalid',N'Győr, Rákóczi Ferenc utca 7.'),
(6,N'hu-sample-v2-patient-6',N'Varga Tamás',N'810010059','1970-06-06',N'paciens6@example.invalid',N'Budapest, Petőfi Sándor utca 8.'),
(7,N'hu-sample-v2-patient-7',N'Kiss Dóra',N'810010066','1972-07-07',N'paciens7@example.invalid',N'Debrecen, Kossuth Lajos utca 9.'),
(8,N'hu-sample-v2-patient-8',N'Molnár Ádám',N'810010073','1974-08-08',N'paciens8@example.invalid',N'Szeged, Arany János utca 10.'),
(9,N'hu-sample-v2-patient-9',N'Németh Katalin',N'810010080','1976-09-09',N'paciens9@example.invalid',N'Pécs, Béke utca 11.'),
(10,N'hu-sample-v2-patient-10',N'Farkas Márton',N'810010097','1978-10-10',N'paciens10@example.invalid',N'Győr, Rákóczi Ferenc utca 12.'),
(11,N'hu-sample-v2-patient-11',N'Balogh Zsófia',N'810010107','1980-11-11',N'paciens11@example.invalid',N'Budapest, Petőfi Sándor utca 13.'),
(12,N'hu-sample-v2-patient-12',N'Papp László',N'810010114','1982-12-12',N'paciens12@example.invalid',N'Debrecen, Kossuth Lajos utca 14.'),
(13,N'hu-sample-v2-patient-13',N'Takács Réka',N'810010121','1984-01-13',N'paciens13@example.invalid',N'Szeged, Arany János utca 15.'),
(14,N'hu-sample-v2-patient-14',N'Juhász Bence',N'810010138','1986-02-14',N'paciens14@example.invalid',N'Pécs, Béke utca 16.'),
(15,N'hu-sample-v2-patient-15',N'Lakatos Emese',N'810010145','1988-03-15',N'paciens15@example.invalid',N'Győr, Rákóczi Ferenc utca 17.'),
(16,N'hu-sample-v2-patient-16',N'Mészáros András',N'810010152','1990-04-16',N'paciens16@example.invalid',N'Budapest, Petőfi Sándor utca 18.'),
(17,N'hu-sample-v2-patient-17',N'Oláh Noémi',N'810010169','1992-05-17',N'paciens17@example.invalid',N'Debrecen, Kossuth Lajos utca 19.'),
(18,N'hu-sample-v2-patient-18',N'Simon Dániel',N'810010176','1994-06-18',N'paciens18@example.invalid',N'Szeged, Arany János utca 20.'),
(19,N'hu-sample-v2-patient-19',N'Rácz Viktória',N'810010183','1996-07-19',N'paciens19@example.invalid',N'Pécs, Béke utca 21.'),
(20,N'hu-sample-v2-patient-20',N'Fekete István',N'810010190','1998-08-20',N'paciens20@example.invalid',N'Győr, Rákóczi Ferenc utca 22.');
CREATE TABLE #D (N int PRIMARY KEY, Id nvarchar(128), Name nvarchar(100), Taj nvarchar(9), Specialty nvarchar(100), City nvarchar(100));
INSERT #D VALUES
(1,N'hu-sample-v2-doctor-1',N'Dr. Kovács Ágnes',N'820020004',N'Belgyógyászat',N'Budapest'),
(2,N'hu-sample-v2-doctor-2',N'Dr. Nagy Balázs',N'820020011',N'Kardiológia',N'Debrecen'),
(3,N'hu-sample-v2-doctor-3',N'Dr. Szabó Judit',N'820020028',N'Ortopédia',N'Szeged'),
(4,N'hu-sample-v2-doctor-4',N'Dr. Tóth András',N'820020035',N'Reumatológia',N'Pécs'),
(5,N'hu-sample-v2-doctor-5',N'Dr. Varga Éva',N'820020042',N'Neurológia',N'Győr'),
(6,N'hu-sample-v2-doctor-6',N'Dr. Kiss Miklós',N'820020059',N'Gyógytorna',N'Budapest'),
(7,N'hu-sample-v2-doctor-7',N'Dr. Horváth Orsolya',N'820020066',N'Rehabilitáció',N'Debrecen'),
(8,N'hu-sample-v2-doctor-8',N'Dr. Molnár Zoltán',N'820020073',N'Diagnosztika',N'Szeged'),
(9,N'hu-sample-v2-doctor-9',N'Dr. Németh Gabriella',N'820020080',N'Dietetika',N'Pécs'),
(10,N'hu-sample-v2-doctor-10',N'Dr. Farkas Levente',N'820020097',N'Sportorvoslás',N'Győr'),
(11,N'hu-sample-v2-doctor-11',N'Dr. Balogh Ildikó',N'820020107',N'Bőrgyógyászat',N'Budapest'),
(12,N'hu-sample-v2-doctor-12',N'Dr. Papp Gergely',N'820020114',N'Szemészet',N'Debrecen'),
(13,N'hu-sample-v2-doctor-13',N'Dr. Takács Krisztina',N'820020121',N'Fül-orr-gégészet',N'Szeged'),
(14,N'hu-sample-v2-doctor-14',N'Dr. Juhász Sándor',N'820020138',N'Sebészet',N'Pécs'),
(15,N'hu-sample-v2-doctor-15',N'Dr. Rácz Andrea',N'820020145',N'Pulmonológia',N'Győr');
SELECT Id OldId, UserId, CONVERT(int, (ROW_NUMBER() OVER(ORDER BY CASE WHEN Id LIKE '%patient-profile' THEN 0 ELSE 1 END,Id)-1)%20+1) N INTO #PM FROM Patients WHERE Id LIKE 'egeszsegut-demo-v1-%';
SELECT Id OldId, UserId, CONVERT(int, (ROW_NUMBER() OVER(ORDER BY CASE WHEN Id LIKE '%doctor-profile' THEN 0 ELSE 1 END,Id)-1)%15+1) N INTO #DM FROM Practitioners WHERE Id LIKE 'egeszsegut-demo-v1-%';
-- Új kezelői technikai fiókok, beégetett jelszó nélkül.
INSERT AspNetUsers(Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount)
SELECT Id+'-user',CONCAT('kezelo',N,'@example.invalid'),UPPER(CONCAT('kezelo',N,'@example.invalid')),
CONCAT('kezelo',N,'@example.invalid'),UPPER(CONCAT('kezelo',N,'@example.invalid')),1,NULL,NEWID(),NEWID(),0,0,1,0 FROM #D;
INSERT AspNetUserRoles(UserId,RoleId) SELECT d.Id+'-user',r.Id FROM #D d CROSS JOIN AspNetRoles r WHERE r.Name='Practitioner';
INSERT Patients(Id,UserId,Name,TajNumber,BirthDate,Email,Phone,Address,Notes,CreatedAt)
SELECT Id,NULL,Name,Taj,Birth,Email,NULL,Address,N'Fiktív bemutatóadat; nem valódi személy. A TAJ kizárólag tesztelésre szolgál.',SYSUTCDATETIME() FROM #P;
INSERT Practitioners(Id,UserId,Name,TajNumber,Specialty,City,Venue)
SELECT Id,Id+'-user',Name,Taj,Specialty,City,N'EgészségÚt Szakrendelő – '+City FROM #D;
INSERT PatientActivities(PatientId,ActivityId)
SELECT DISTINCT p.Id,a.ActivityId FROM PatientActivities a JOIN #PM m ON m.OldId=a.PatientId JOIN #P p ON p.N=m.N;
DELETE a FROM PatientActivities a JOIN #PM m ON m.OldId=a.PatientId;
INSERT ActivityPractitioners(PractitionerId,ActivityId)
SELECT DISTINCT d.Id,a.ActivityId FROM ActivityPractitioners a JOIN #DM m ON m.OldId=a.PractitionerId JOIN #D d ON d.N=m.N;
DELETE a FROM ActivityPractitioners a JOIN #DM m ON m.OldId=a.PractitionerId;
SELECT DISTINCT COALESCE(p.Id,a.PatientId) PatientId,COALESCE(d.Id,a.PractitionerId) PractitionerId INTO #Grants
FROM PatientPractitionerAccesses a LEFT JOIN #PM pm ON pm.OldId=a.PatientId LEFT JOIN #P p ON p.N=pm.N
LEFT JOIN #DM dm ON dm.OldId=a.PractitionerId LEFT JOIN #D d ON d.N=dm.N
WHERE pm.OldId IS NOT NULL OR dm.OldId IS NOT NULL;
DELETE a FROM PatientPractitionerAccesses a WHERE EXISTS(SELECT 1 FROM #PM WHERE OldId=a.PatientId) OR EXISTS(SELECT 1 FROM #DM WHERE OldId=a.PractitionerId);
INSERT PatientPractitionerAccesses(PatientId,PractitionerId,GrantedAt) SELECT g.PatientId,g.PractitionerId,SYSUTCDATETIME() FROM #Grants g WHERE NOT EXISTS(SELECT 1 FROM PatientPractitionerAccesses a WHERE a.PatientId=g.PatientId AND a.PractitionerId=g.PractitionerId);
UPDATE a SET PatientId=p.Id FROM Appointments a JOIN #PM m ON m.OldId=a.PatientId JOIN #P p ON p.N=m.N;
UPDATE a SET PractitionerId=d.Id FROM Appointments a JOIN #DM m ON m.OldId=a.PractitionerId JOIN #D d ON d.N=m.N;
UPDATE a SET PatientId=p.Id FROM PatientNotes a JOIN #PM m ON m.OldId=a.PatientId JOIN #P p ON p.N=m.N;
UPDATE a SET PatientId=p.Id FROM PatientDocuments a JOIN #PM m ON m.OldId=a.PatientId JOIN #P p ON p.N=m.N;
DELETE a FROM PractitionerWorkingHours a JOIN #DM m ON m.OldId=a.PractitionerId;
DELETE a FROM PractitionerBookingSettings a JOIN #DM m ON m.OldId=a.PractitionerId;
DELETE a FROM Patients a JOIN #PM m ON m.OldId=a.Id;
DELETE a FROM Practitioners a JOIN #DM m ON m.OldId=a.Id;
-- A korábbi demóbelépések egy kiválasztott új profilra mutatnak.
UPDATE p SET UserId=x.UserId FROM Patients p JOIN #P n ON n.Id=p.Id
CROSS APPLY(SELECT TOP 1 m.UserId FROM #PM m WHERE m.N=n.N ORDER BY CASE WHEN m.OldId LIKE '%patient-profile' THEN 0 ELSE 1 END,m.OldId) x;
UPDATE d SET UserId='egeszsegut-demo-v1-Practitioner' FROM Practitioners d
WHERE d.Id='hu-sample-v2-doctor-1' AND EXISTS(SELECT 1 FROM #DM WHERE UserId='egeszsegut-demo-v1-Practitioner');
INSERT PractitionerBookingSettings(Id,PractitionerId,BookingEnabled) SELECT Id+'-booking',Id,1 FROM #D;
INSERT PractitionerWorkingHours(Id,PractitionerId,DayOfWeek,IsWorkingDay,StartTime,EndTime)
SELECT d.Id+'-hours-'+CONVERT(varchar,day.N),d.Id,day.N,CASE WHEN day.N BETWEEN 1 AND 5 THEN 1 ELSE 0 END,'08:00','16:00'
FROM #D d CROSS JOIN (VALUES(0),(1),(2),(3),(4),(5),(6))day(N);
-- Egységes kategóriakódok és bemutatófelirat nélküli eseménycímek.
UPDATE Activities SET Title=LEFT(Title,CHARINDEX(N' – demó',Title)-1) WHERE Id LIKE 'egeszsegut-demo-v1-%' AND CHARINDEX(N' – demó',Title)>0;
UPDATE Activities SET Category=CASE Category WHEN N'Vizsgálat' THEN 'examination' WHEN N'Konzultáció' THEN 'consultation' WHEN N'Kezelés' THEN 'treatment' WHEN N'Gyógytorna' THEN 'physiotherapy' WHEN N'Labor' THEN 'laboratory' WHEN N'Képalkotó vizsgálat' THEN 'imaging' WHEN N'Kontroll' THEN 'control' ELSE Category END WHERE Id LIKE 'egeszsegut-demo-v1-%';
UPDATE Activities SET Venue=N'EgészségÚt Szakrendelő – '+City WHERE Id LIKE 'egeszsegut-demo-v1-%';
IF (SELECT COUNT(*) FROM Patients WHERE Id LIKE 'hu-sample-v2-%')<>20 OR (SELECT COUNT(*) FROM Practitioners WHERE Id LIKE 'hu-sample-v2-%')<>15 THROW 51001,'Hibás céldarabszám.',1;
COMMIT;
SELECT 'Patients' Kind,COUNT(*) Total FROM Patients UNION ALL SELECT 'Practitioners',COUNT(*) FROM Practitioners UNION ALL SELECT 'Activities',COUNT(*) FROM Activities;
