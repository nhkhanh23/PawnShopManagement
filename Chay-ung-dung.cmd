@echo off
setlocal
set "PAWN_PROJECT="
set "PAWN_MATCHES=0"
if exist "%~dp0PawnShopManagement\PawnShopManagement.csproj" (
    set "PAWN_PROJECT=%~dp0PawnShopManagement\PawnShopManagement.csproj"
    goto projectfound
)
if exist "%~dp0PawnShopManagement.csproj" (
    set "PAWN_PROJECT=%~dp0PawnShopManagement.csproj"
    goto projectfound
)
rem Also support a source folder nested an extra level after extraction.
for /r "%~dp0" %%F in (PawnShopManagement.csproj) do if exist "%%~fF" (
    set "PAWN_PROJECT=%%~fF"
    set /a PAWN_MATCHES+=1 >nul
)
if "%PAWN_MATCHES%"=="1" goto projectfound
if "%PAWN_MATCHES%"=="0" (
    echo KHONG TIM THAY PawnShopManagement.csproj trong thu muc nay.
    echo Hay giai nen toan bo ma nguon, khong chay file CMD ben trong ZIP.
    echo Tren GitHub phai co file PawnShopManagement/PawnShopManagement.csproj.
) else (
    echo TIM THAY NHIEU BAN DU AN. Khong tu dong chon de tranh chay nham.
    echo Dat file CMD cung cap voi thu muc PawnShopManagement can chay.
)
if /i "%~1"=="--check-project" exit /b 2
pause
exit /b 2
:projectfound
echo Du an: "%PAWN_PROJECT%"
if /i "%~1"=="--check-project" exit /b 0
for %%F in ("%PAWN_PROJECT%") do set "PAWN_PROJECT_DIR=%%~dpF"
pushd "%PAWN_PROJECT_DIR%"
if errorlevel 1 goto pathfailed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo Khong tim thay .NET SDK. Can cai .NET SDK 10 truoc khi chay.
    goto failed
)
set "ASPNETCORE_ENVIRONMENT=Development"

echo [1/4] Khoi phuc thu vien NuGet...
dotnet restore "%PAWN_PROJECT%"
if errorlevel 1 goto failed
echo [2/4] Bien dich ung dung...
dotnet build "%PAWN_PROJECT%" --no-restore
if errorlevel 1 goto failed
echo [3/4] Kiem tra va khoi tao co so du lieu...
dotnet run --project "%PAWN_PROJECT%" --no-build --no-launch-profile -- --init-db
if errorlevel 1 goto databasefailed
echo [4/4] Mo http://localhost:5073/Account/Login trong trinh duyet.
echo Lan dau chua co tai khoan: http://localhost:5073/Account/Setup
echo Giu cua so nay mo. Nhan Ctrl+C de dung.
dotnet run --project "%PAWN_PROJECT%" --no-build --no-launch-profile -- --urls http://localhost:5073
if errorlevel 1 goto failed
popd
exit /b 0
:databasefailed
echo Khong khoi tao duoc DB. Kiem tra SQL Server LocalDB va chuoi ket noi.
goto failed

:failed
echo.
echo Khong khoi dong duoc. Xem loi phia tren va chup anh de duoc ho tro.
popd
pause
exit /b 1
:pathfailed
echo Khong mo duoc thu muc du an. Hay giai nen vao thu muc co quyen truy cap.
pause
exit /b 1
