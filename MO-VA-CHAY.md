# Mở bản đã sửa

1. Đóng cửa sổ chạy ứng dụng cũ trước khi build lại.
2. Trong File Explorer, nhấp đúp **Chay-ung-dung.cmd** ở thư mục này.
3. Chờ bước khởi tạo DB thành công và dòng **Now listening on**.
4. Mở http://localhost:5073/Account/Setup để tạo quản trị viên lần đầu. Nếu đã có tài khoản, dùng /Account/Login.

Không nhấn Run trên file .cmd trong Visual Studio. Nếu dùng Visual Studio, mở PawnShopManagement.slnx, chọn PawnShopManagement làm Startup Project, Rebuild rồi Ctrl+F5.

## Cơ sở dữ liệu

Development tự tạo database nếu thiếu và bổ sung bảng/cột theo Database/schema.sql. Không xóa database cũ. Cơ chế này thay EnsureCreated vì EnsureCreated không bổ sung bảng vào DB đã có một phần schema. Nếu dữ liệu cũ bị trùng CCCD hoặc không phù hợp độ dài, khởi tạo sẽ dừng để người vận hành kiểm tra; không tự cắt/xóa hồ sơ.

Nếu lỗi quyền hoặc instance: kiểm tra ConnectionStrings:DefaultConnection trong appsettings.json, mở LocalDB dưới đúng tài khoản Windows đang chạy ứng dụng. Không thêm EnableRetryOnFailure để che lỗi thiếu DB/quyền. Bước khởi tạo sẽ báo lỗi và dừng ở cửa sổ lệnh.

## Xác minh SQL thực tế

Nhấp đúp **Kiem-tra-SQL.cmd**. Script dùng database PawnShopVerification có tên ngẫu nhiên, không sử dụng hoặc xóa PawnShopDB. DB kiểm thử được giữ lại sau khi chạy để kiểm tra. Nếu cấu hình SQL Server khác LocalDB, cập nhật PAWN_SQL_TEST_CONNECTION trong script. Kết quả nằm ở Ket-qua-SQL.

32 kiểm thử InMemory đã PASS. Chưa xác minh SQL trong phiên công cụ vì Windows từ chối kết nối named pipe. Không coi kết quả InMemory là kiểm chứng FK/UNIQUE/CHECK của SQL Server.

## Tài liệu

Thư mục Tai-lieu chứa báo cáo PDF, nội dung Markdown để chỉnh sửa, sơ đồ draw.io, test case CSV và PowerPoint. Ảnh giao diện thực tế và thông tin trường/nhóm cần bổ sung trước khi nộp. Báo cáo phân biệt rõ kết quả đã kiểm tra và SQL chưa xác minh.


## Thời hạn và cảnh báo hạn

Bản mới đã có TermDays, ngày đến hạn, cảnh báo và bộ lọc theo hạn. Xem Tai-lieu/Thoi-han-va-canh-bao.md. Kết quả mới nhất: 60 kiểm tra PASS; bộ SQL thật vẫn cần chạy riêng.
