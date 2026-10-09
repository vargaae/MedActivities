@echo off
setlocal

set "ROOT=%~dp0"
set "API_DIR=%ROOT%API"
set "CLIENT_DIR=%ROOT%client"
set "FRONTEND_URL=https://localhost:3000"
set "PRESENTATION=%ROOT%EgeszsegUt_vizsgaremek_bemutato.pptx"

if not exist "%API_DIR%\API.csproj" (
    echo HIBA: Az API projekt nem talalhato: "%API_DIR%"
    pause
    exit /b 1
)

if not exist "%CLIENT_DIR%\package.json" (
    echo HIBA: A frontend projekt nem talalhato: "%CLIENT_DIR%"
    pause
    exit /b 1
)

echo MedActivities API inditasa...
start "MedActivities API" cmd /k "cd /d "%API_DIR%" && dotnet run --project API.csproj"

echo Varakozas az API elindulasara...
timeout /t 5 /nobreak >nul

echo MedActivities frontend inditasa...
start "MedActivities Frontend" cmd /k "cd /d "%CLIENT_DIR%" && npm run dev"

echo Varakozas a frontend elindulasara...
timeout /t 5 /nobreak >nul

echo Frontend megnyitasa a bongeszoben...
start "" "%FRONTEND_URL%"

echo Vizsgaremek bemutato megnyitasa...
if exist "%PRESENTATION%" (
    start "" "%PRESENTATION%"
) else (
    echo FIGYELEM: A bemutato nem talalhato: "%PRESENTATION%"
)

echo.
echo Az API, a frontend es a bongeszo elindult.
echo API:      https://localhost:5001
echo Frontend: %FRONTEND_URL%
echo.
pause
