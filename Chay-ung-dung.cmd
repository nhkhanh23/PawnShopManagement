@echo off
setlocal
cd /d "%~dp0PawnShopManagement"
echo [1/4] Khoi phuc thu vien NuGet...
dotnet restore
if errorlevel 1 goto failed
echo [2/4] Bien dich ung dung...
dotnet build --no-restore
if errorlevel 1 goto failed
echo [3/4] Kiem tra va tao co so du lieu neu chua co...
dotnet run --no-build --launch-profile http -- --init-db
if errorlevel 1 goto databasefailed
echo [4/4] Mo http://localhost:5073/Account/Setup de tao tai khoan lan dau.
echo Giu cua so nay mo khi su dung. Nhan Ctrl+C de dung.
dotnet run --no-build --launch-profile http
if errorlevel 1 goto failed
exit /b 0
:databasefailed
echo.
echo Khong truy cap duoc SQL Server hoac khong co quyen tao PawnShopDB.
echo Kiem tra ConnectionStrings trong PawnShopManagement\appsettings.json.
echo Giu cua so nay va chup anh thong bao loi phia tren de duoc ho tro.
pause
exit /b 1
:failed
echo.
echo Khong khoi dong duoc. Xem thong bao loi phia tren.
pause
exit /b 1
