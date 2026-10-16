@echo off
setlocal
cd /d "%~dp0PawnShopManagement"
echo Dang khoi tao PawnShopDB. Khong xoa co so du lieu dang co.
dotnet run --launch-profile http -- --init-db
if errorlevel 1 goto failed
echo.
echo Khoi tao xong. Chay Chay-ung-dung.cmd roi mo http://localhost:5073/Account/Setup
pause
exit /b 0
:failed
echo.
echo Khong khoi tao duoc. Vui long chup anh loi phia tren.
pause
exit /b 1
