# Thời hạn cầm và cảnh báo hạn

## Sử dụng

1. Đóng ứng dụng đang chạy, mở lại Chay-ung-dung.cmd. Bước khởi tạo tự bổ sung cột TermDays và ràng buộc dữ liệu, không xóa hợp đồng cũ.
2. Vào Tạo hợp đồng hoặc Sửa. Nhập **Thời hạn cầm (ngày)** từ 1 đến 3650 ngày. Hợp đồng mới mặc định 30 ngày; đây là giá trị gợi ý, người dùng có thể đổi.
3. **Ngày đến hạn = ngày cầm + số ngày cầm**. Ngày đến hạn được xem trước khi lưu, tính lại tại server khi đọc dữ liệu, không lấy từ giá trị tự gửi của trình duyệt.
4. Xem cột **Thời hạn / đến hạn** và các ô cảnh báo phía trên bảng. Dùng bộ lọc Hạn hợp đồng để xem riêng các nhóm.

Ví dụ: cầm ngày 16/10/2026 trong 30 ngày thì đến hạn ngày 15/11/2026.

## Quy tắc cảnh báo

| Nhóm | Điều kiện với hợp đồng đang cầm |
|---|---|
| Quá hạn | Ngày đến hạn trước hôm nay; hiển thị số ngày quá hạn |
| Đến hạn hôm nay | Ngày đến hạn bằng hôm nay; chưa tính là quá hạn |
| Sắp đến hạn | Còn 1 đến 3 ngày, bao gồm đúng 3 ngày |
| Còn hạn | Còn hơn 3 ngày; hiển thị số ngày còn lại |

Tính theo ngày tại Việt Nam (UTC+7), không theo giờ/phút. Cảnh báo xuất hiện khi mở, tải lại hoặc lọc trang; không gửi email, SMS hay thông báo nền. Hợp đồng đã chuộc hoặc đã thanh lý không cảnh báo, nhưng vẫn giữ ngày đến hạn để xem lại.

## Dữ liệu cũ

Hợp đồng trước bản cập nhật chưa có thời hạn được giữ TermDays = NULL và hiện **Chưa nhập thời hạn**. Hệ thống không tự đoán thời hạn cũ là 30 ngày. Mở Sửa để nhập thời hạn theo thỏa thuận thực tế trước khi lưu; sau đó hợp đồng mới tham gia cảnh báo.

## API và cơ sở dữ liệu

- POST/PUT `/api/contracts`: thêm thuộc tính `termDays` (số nguyên, 1–3650). Với PUT nên gửi đầy đủ dữ liệu, bao gồm thời hạn thực tế.
- GET API trả `termDays` và `dueDate`. Hợp đồng cũ chưa nhập thời hạn trả null cho ngày đến hạn.
- Lọc API: `/api/contracts?deadline=Overdue`, `deadline=DueToday` hoặc `deadline=DueSoon`; kết hợp được với search, status và page.
- MVC dùng tham số deadlineFilter; bộ lọc được giữ khi phân trang AJAX và dùng Back/Forward.
- Schema mới thêm cột nullable và CHECK cho khoảng 1–3650. Nullable chỉ phục vụ bảo toàn dữ liệu cũ; biểu mẫu tạo/sửa yêu cầu nhập thời hạn.

## Kiểm chứng

Kiểm thử bổ sung bao gồm tính ngày qua tháng nhuận/năm mới, mốc 0/1/3/4 ngày, loại trừ hợp đồng kết thúc, dữ liệu cũ, lọc API/AJAX/phân trang, lưu/sửa thời hạn, lỗi nhập và khả năng dịch truy vấn sang SQL DATEADD. Kết quả chi tiết trong integration-results.txt. Các kiểm thử chạy bằng InMemory; kiểm tra dịch SQL không đồng nghĩa đã kết nối hoặc chạy truy vấn trên SQL Server thật.

Báo cáo PDF đã cập nhật thời hạn, ngày đến hạn, quy tắc cảnh báo và 60 kiểm tra PASS. Slide hiện có vẫn cần đồng bộ với phần mới. Bổ sung ảnh giao diện thực tế từ bản mới trước khi nộp.
