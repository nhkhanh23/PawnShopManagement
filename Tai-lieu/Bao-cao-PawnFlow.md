Hệ thống quản lý cầm đồ PawnFlow

Báo cáo kỹ thuật bài tập lớn Lập trình web

ASP.NET Core MVC trên .NET 10 · SQL Server · Entity Framework Core

Phiên bản hoàn thiện mã nguồn ngày 16 tháng 10 năm 2026

Phạm vi và kết quả

Ứng dụng hỗ trợ quản lý hồ sơ khách hàng và hợp đồng cầm cố, với hai vai trò quản trị viên và nhân viên. Giao diện có bộ lọc, phân trang, AJAX và hai ngôn ngữ Việt và Anh. REST API cung cấp thao tác đọc, tạo, cập nhật và xóa hợp đồng. Bản cập nhật bổ sung thời hạn cầm theo ngày, ngày đến hạn và cảnh báo sắp đến hạn hoặc quá hạn.

Đã có 60 kiểm tra tự động PASS: 57 kiểm tra hành vi và logic với test host InMemory, cùng 3 kiểm tra dịch truy vấn sang SQL Server. Truy cập SQL Server từ môi trường kiểm tra hiện bị Windows từ chối, vì vậy chưa xác nhận các kiểm thử SQL là thành công. Bộ kiểm thử SQL riêng được cung cấp để chạy dưới tài khoản Windows có quyền LocalDB.

Kho mã nguồn: <link href="https://github.com/nhkhanh23/PawnShopManagement" color="#16765f">github.com/nhkhanh23/PawnShopManagement</link>. Link này dùng để xem và tải mã nguồn; ứng dụng cần chạy theo hướng dẫn cài đặt.

Cấu trúc báo cáo

| Phần | Nội dung |
|---|---|
| 4.1 | Tổng quan, phạm vi nghiệp vụ và vai trò |
| 4.2 | Use case, ERD, sitemap và wireframe |
| 4.3 | Triển khai, API, bảo mật và quốc tế hóa |
| 4.4 | Kiểm thử, kết quả và cách sửa lỗi |
| 4.5 | Kết luận, giới hạn và hướng phát triển |
Tài liệu đi kèm: mã nguồn, script tạo bảng, hướng dẫn chạy, bộ kiểm thử, tệp CSV test case và sơ đồ draw.io có thể chỉnh sửa. Thông tin trường, giảng viên và thành viên nhóm do nhóm bổ sung trước khi nộp.


---

4.1 Tổng quan và phân tích yêu cầu


Đề tài giải quyết việc lưu trữ tập trung thông tin khách hàng, tài sản nhận cầm và khoản vay. Người dùng tra cứu nhanh hồ sơ, sửa dữ liệu và theo dõi trạng thái đang cầm, đã chuộc hoặc đã thanh lý.

Các lớp người dùng

| Chức năng | Staff | Admin |
|---|---|---|
| Xem, tìm kiếm, lọc và phân trang | Có | Có |
| Nhập thời hạn và theo dõi cảnh báo hạn | Có | Có |
| Thêm và sửa khách hàng, hợp đồng | Có | Có |
| Xóa hồ sơ theo ràng buộc | Không | Có |
| Tạo tài khoản nhân viên | Không | Có |
| Sử dụng REST API | Theo quyền | Theo quyền |
Quy trình nghiệp vụ

Nhân viên đăng nhập, tạo hoặc chọn khách hàng, khai báo tài sản, khoản vay và số ngày cầm rồi lưu hợp đồng. Ngày đến hạn được tính từ ngày cầm cộng với số ngày đã nhập. Nhân viên theo dõi danh sách cảnh báo để xử lý hợp đồng sắp đến hạn, đến hạn hôm nay hoặc quá hạn. Khi có thay đổi thực tế, nhân viên cập nhật trạng thái. Quản trị viên quản lý tài khoản nhân viên và thực hiện thao tác xóa có xác nhận.

Không cho xóa khách hàng đang có hợp đồng. Kiểm tra đầu vào tại server bao gồm CCCD không trùng, khách hàng tồn tại, tiền vay trong khoảng quy định và ngày cầm không thuộc tương lai. Việc chuyển trạng thái đã chuộc không thay thế chứng từ thu tiền.


---

4.2 Sơ đồ use case


Hình 1 Các nhóm chức năng và vai trò

Điều kiện và kết quả

Tất cả use case quản lý yêu cầu người dùng đã đăng nhập. Tạo hoặc sửa dữ liệu yêu cầu biểu mẫu hợp lệ và token chống CSRF. Xóa hồ sơ và tạo nhân viên yêu cầu vai trò Admin; controller và API cùng áp dụng ràng buộc này.

Khi lưu thành công, giao diện chuyển về danh sách và hiện thông báo. Khi lỗi validation, dữ liệu nhập và thông báo lỗi được giữ trên form. Truy cập id không tồn tại trả 404; API chưa đăng nhập trả 401 và không đủ quyền trả 403.


---

4.2 Thiết kế cơ sở dữ liệu


Hình 2 ERD và quan hệ một nhiều

| Bảng | Dữ liệu và ràng buộc chính |
|---|---|
| Customers | Tên bắt buộc; CCCD 9 hoặc 12 số và duy nhất; điện thoại 10 số bắt đầu 0; địa chỉ tối đa 250 ký tự. |
| PawnContracts | Khóa ngoại CustomerID; tiền vay decimal(18,2), từ 500.000 đến 1 tỷ; lãi suất 0–100; trạng thái chỉ có ba giá trị. TermDays lưu số ngày cầm; DueDate được tính khi đọc, không lưu cột riêng. |
| AppUsers | Tên đăng nhập duy nhất; mật khẩu chỉ lưu hash; vai trò Admin hoặc Staff; lưu số lần đăng nhập sai và thời điểm mở khóa. |
Database/schema.sql tạo bảng và bổ sung cột còn thiếu bằng các điều kiện kiểm tra tồn tại. Khởi tạo dùng transaction và khóa ứng dụng SQL để tránh hai tiến trình cùng sửa schema. Bản cập nhật bổ sung TermDays kiểu int nullable và CHECK cho giá trị 1–3650; NULL chỉ giữ cho hồ sơ cũ chưa có thời hạn. Script dừng nếu dữ liệu cũ cản trở tạo chỉ mục duy nhất, không tự xóa hoặc cắt dữ liệu.

Các CHECK tiền vay, lãi suất, trạng thái và vai trò có trên bảng được tạo mới bằng schema.sql. Với cơ sở dữ liệu cũ, cần kiểm tra riêng các constraint đang có; không coi việc khởi tạo thành công là bằng chứng tất cả constraint cũ đã được nâng cấp.


---

4.2 Thiết kế điều hướng và giao diện


Hình 3 Sitemap ứng dụng

Hình 4 Wireframe danh sách hợp đồng trên desktop

Trên màn hình nhỏ, thanh điều hướng chuyển thành hàng ngang; thẻ thống kê xếp dọc và bảng cuộn ngang trong vùng riêng. Form giữ nhãn rõ ràng, hiển thị lỗi bên dưới trường và tổng hợp lỗi ở đầu form. Tệp So-do-thiet-ke.drawio chứa các hình để chỉnh sửa.


---

4.3 Triển khai và giao tiếp API


Controller nhận yêu cầu, kiểm tra quyền và validation, rồi dùng ApplicationDbContext truy cập dữ liệu. Razor hiển thị giao diện; Bootstrap hỗ trợ bố cục và jQuery phục vụ validation. Fetch tải PartialView cho bộ lọc và phân trang hợp đồng, có thông báo khi tải thất bại và hỗ trợ lịch sử trình duyệt.

| HTTP | Endpoint | Kết quả |
|---|---|---|
| GET | /api/contracts?deadline=DueSoon&page=1 | 200, danh sách tối đa 10 dòng |
| GET | /api/contracts/{id} | 200 hoặc 404 |
| POST | /api/contracts | 201 và Location |
| PUT | /api/contracts/{id} | 204 hoặc lỗi validation |
| DELETE | /api/contracts/{id} | 204, chỉ Admin |
Security

Cookie xác thực lưu danh tính và vai trò. Tất cả yêu cầu ghi dữ liệu đều kiểm tra anti-forgery token; client API lấy token qua /Account/Token và gửi header RequestVerificationToken. PasswordHasher lưu mật khẩu dạng hash. Tài khoản khóa 5 phút sau 5 lần nhập sai; khóa được lưu trong DB để không mất khi thay cookie phiên.

Razor mã hóa nội dung người dùng; model binding giới hạn thuộc tính ở MVC và bỏ navigation Customer khi nhận JSON. Redirect chỉ chấp nhận URL nội bộ. Cookie có HttpOnly và SameSite. Khóa bảo vệ cookie lưu ngoài wwwroot, dùng DPAPI trên Windows. HTTPS cần cấu hình chứng chỉ hợp lệ khi triển khai.

Quốc tế hóa và định dạng số

Resources/UiResources.resx và UiResources.en-US.resx quản lý nhãn và thông báo. RequestLocalization chọn vi-VN hoặc en-US, cookie lưu lựa chọn. Model binder đọc số HTML theo dấu chấm invariant để lãi 1.5 không bị hiểu thành 15; định dạng hiển thị vẫn theo ngôn ngữ người dùng.

API nhận termDays khi tạo hoặc cập nhật. GET trả termDays và dueDate; lọc hạn dùng deadline=Overdue, DueToday hoặc DueSoon và kết hợp được với search, status, page. DueDate là giá trị suy ra, client không được ghi đè trực tiếp.


---

4.3 Minh họa chức năng


| Màn hình | Cách thao tác và kết quả quan sát |
|---|---|
| Danh sách hợp đồng | Hiển thị dư nợ, số hợp đồng đang cầm và đã chuộc. Có bộ lọc theo trạng thái và hạn hợp đồng, số lượng cảnh báo và cột thời hạn/ngày đến hạn; mỗi dòng có thao tác theo vai trò. |
| Tạo và sửa hợp đồng | Chọn khách hàng; nhập tài sản, tiền vay, lãi suất, ngày cầm và thời hạn 1–3650 ngày. Ngày đến hạn cập nhật ngay trên form. Khung giải thích hiển thị tiền lãi ước tính, chưa bao gồm phí. |
| Khách hàng | Tìm theo tên, điện thoại hoặc CCCD. CCCD trùng bị chặn. Khách còn hợp đồng không thể bị xóa. |
| Đăng nhập và quyền | Nhân viên không thấy thao tác quản trị. Server vẫn chặn yêu cầu trực tiếp không đủ quyền, kể cả khi client tự tạo nút. |
| Ngôn ngữ | Chọn Việt hoặc Anh trên topbar rồi xác nhận. Nhãn, thông báo và số liệu chuyển theo culture; lựa chọn được lưu vào cookie. |
Danh sách hợp đồng với dữ liệu kiểm thử giả. Bộ lọc, thống kê và thao tác sửa xuất hiện trong cùng màn hình. Ảnh màn hình cần chụp bổ sung sau khi khởi động trên máy có quyền truy cập SQL Server.

Biểu mẫu sửa giữ nguyên khách hàng, lãi suất và trạng thái. Tiền lãi ước tính thay đổi theo giá trị nhập. Ảnh màn hình cần chụp bổ sung sau khi khởi động trên máy có quyền truy cập SQL Server.

Dữ liệu minh họa dùng tên và số định danh giả trong test host, không phản ánh giao dịch thực tế. Đối chiếu kết quả SQL trước khi dùng các hình này làm bằng chứng nghiệm thu trên SQL Server.


---

4.3 Thời hạn cầm và cảnh báo


Thời hạn được nhập bằng số ngày nguyên từ 1 đến 3650. Hợp đồng mới mặc định 30 ngày và cho phép thay đổi trước khi lưu. Ngày đến hạn bằng ngày cầm cộng thời hạn theo ngày lịch; ví dụ 16/10/2026 cộng 30 ngày là 15/11/2026.

| Đại lượng | Cách tính hoặc lưu trữ |
|---|---|
| TermDays | Cột int nullable trong SQL Server; form tạo và sửa yêu cầu có giá trị trong khoảng 1–3650. |
| DueDate | Thuộc tính suy ra, có NotMapped; bằng StartDate.Date.AddDays(TermDays). Không tạo cột ngày đến hạn riêng. |
| Ngày hiện tại | Theo múi giờ Việt Nam UTC+7, không theo culture hoặc múi giờ trình duyệt. |
| Số ngày còn lại | Ngày đến hạn trừ ngày hiện tại; không tính theo giờ hoặc phút. |
Các mốc cảnh báo

| Nhóm | Điều kiện khi đang cầm | Ví dụ với hạn 15/11/2026 |
|---|---|---|
| Quá hạn | Ngày đến hạn trước hôm nay | Ngày 16/11: quá hạn 1 ngày |
| Đến hạn hôm nay | Ngày đến hạn bằng hôm nay | Ngày 15/11: đến hạn hôm nay |
| Sắp đến hạn | Còn 1–3 ngày, gồm đúng 3 ngày | Ngày 12–14/11: cảnh báo sớm |
| Còn hạn | Còn hơn 3 ngày | Ngày 11/11: còn 4 ngày |
| Không cảnh báo | Đã chuộc, đã thanh lý hoặc chưa biết thời hạn | Hồ sơ cũ cần bổ sung thời hạn |
Cảnh báo hiển thị trên trang khi người dùng mở, tải lại hoặc lọc dữ liệu. Hệ thống không gửi email, SMS hoặc thông báo nền. Cảnh báo hạn không tự thay đổi trạng thái hợp đồng và không tự thanh lý tài sản.

Bảo toàn dữ liệu cũ

Không gán thời hạn 30 ngày cho hồ sơ đã tồn tại vì thời hạn thực tế chưa được ghi nhận. Hồ sơ này giữ TermDays = NULL, không tham gia cảnh báo và hiện “Chưa nhập thời hạn”. Khi sửa, nhân viên nhập thời hạn theo thỏa thuận thực tế. Giá trị mặc định 30 ngày chỉ áp dụng khi tạo đối tượng mới.


---

4.4 Kiểm thử và kết quả


Bộ kiểm thử dùng WebApplicationFactory với cookie xác thực, token CSRF và các controller/view thật. Dữ liệu ban đầu có 12 hợp đồng, gồm 10 đang cầm và 2 đã chuộc. Có hai tài khoản kiểm thử Admin và Staff. Kết quả hiện tại: 60 kiểm tra PASS, gồm 57 kiểm tra hành vi/logic với InMemory và 3 kiểm tra dịch truy vấn SQL. Kiểm tra dịch SQL không kết nối hoặc thực thi trên SQL Server; chưa có kết quả PASS của bộ kiểm thử SQL thực tế.

| Mục tiêu và đầu vào | Kỳ vọng | Thực tế |
|---|---|---|
| Không cookie, GET MVC / API | Redirect / 401 | PASS |
| POST không token CSRF | 400 | PASS |
| Staff xóa API / tạo nhân viên | 403 / từ chối | PASS |
| 12 bản ghi, trang 2 | 2 phần tử | PASS |
| Lọc DangCam, trang 999 | 10 dòng, về trang 1 | PASS |
| Header AJAX khi lọc | HTML fragment, không layout | PASS |
| CCCD đã tồn tại | Trả form, không lưu trùng | PASS |
| Tiền 2500000, lãi 1.5 | Giữ đúng cả hai giá trị | PASS |
| Khách 999, tiền 1, ngày tương lai | Không thêm hợp đồng | PASS |
| REST tạo, sửa, xóa rồi đọc | 201, 204, 204, 404 | PASS |
| Chọn en-US | Dịch nhãn và tên trường | PASS |
| Sai mật khẩu ở 5 session mới | Khóa DB, chặn đăng nhập đúng | PASS |
| SQL FK, UNIQUE và CHECK | SQL từ chối dữ liệu sai | Chưa chạy |
Tệp Test-cases.csv có mục tiêu, đầu vào/ràng buộc, đầu ra kỳ vọng, thực tế, kết quả và môi trường cho từng kiểm tra. integration-results.txt ghi từng assertion PASS. Bộ SQL tạo database PawnShopVerification riêng để không thay đổi PawnShopDB.


---

4.4 Kiểm thử thời hạn hợp đồng


28 kiểm tra mới bổ sung cho 32 kiểm tra nền tảng. Các ca kiểm tra bao phủ cách tính ngày lịch, ranh giới cảnh báo, lưu/sửa dữ liệu, validation, REST API và bộ lọc khi phân trang. Bảng dưới đây tổng hợp các đầu vào đại diện; tệp CSV đi kèm ghi từng assertion.

| Mục tiêu và đầu vào | Kỳ vọng | Thực tế |
|---|---|---|
| 31/01/2024, thời hạn 30 ngày | Đến hạn 01/03/2024 | PASS |
| 31/12/2026, thời hạn 1 ngày | Đến hạn 01/01/2027 | PASS |
| Đang cầm, hạn hôm qua | Overdue | PASS |
| Đang cầm, hạn hôm nay | DueToday, chưa quá hạn | PASS |
| Còn 1 hoặc 3 ngày | DueSoon | PASS |
| Còn 4 ngày | OnTime, chưa cảnh báo sớm | PASS |
| Đã chuộc hoặc thanh lý, hạn cũ | Closed, không cảnh báo | PASS |
| Hồ sơ cũ có TermDays = NULL | Unknown, không đoán hạn | PASS |
| Tạo 45 ngày, sửa thành 60 ngày | Lưu đúng thời hạn và ngày suy ra | PASS |
| Thời hạn 0, 3651 hoặc để trống | Trả form lỗi, không lưu | PASS |
| API nhận thời hạn 0 | HTTP 400 | PASS |
| 11 hợp đồng sắp đến hạn, trang 2 | 1 dòng, giữ bộ lọc | PASS |
| Dịch biểu thức lọc bằng SQL provider | Có DATEADD trong SQL sinh ra | PASS dịch SQL |
Các kiểm tra SQL UNIQUE, FOREIGN KEY và CHECK đang được đánh dấu chưa xác minh. Không dùng kết quả sinh câu SQL hoặc kết quả InMemory để kết luận schema và dữ liệu thật đã chạy thành công.


---

4.4 Lỗi phát hiện và cách xử lý


| Lỗi | Nguyên nhân | Cách sửa và xác minh |
|---|---|---|
| Không tìm thấy exe | Chưa restore/build | File cmd restore rồi build; dừng khi lỗi. Build mới thành công. |
| Không mở được PawnShopDB | DB chưa có hoặc không đủ quyền | Tạo DB nếu thiếu, chạy schema theo transaction. Truy cập SQL thực tế còn bị Windows chặn trong phiên công cụ. |
| Nút Sửa không chạy | Thiếu action/view | Bổ sung GET/POST Edit; kiểm thử lưu cập nhật PASS. |
| Form hợp đồng luôn lỗi | Validation navigation Customer | ValidateNever, kiểm tra CustomerID thực tế; tạo hợp đồng PASS. |
| Số thập phân sai culture | HTML number khác định dạng vi-VN | Invariant number binder; xác minh lãi 1.5 lưu đúng. |
| Thư viện không tương thích | PagedList.Mvc dành .NET Framework | Bỏ thư viện cũ, phân trang EF; build 0 cảnh báo. |
Chạy kiểm thử SQL Server

Đóng ứng dụng đang chạy. Nhấp đúp Kiem-tra-SQL.cmd dưới tài khoản Windows có LocalDB. Bộ kiểm thử tạo database riêng, chạy cùng luồng HTTP và thêm kiểm tra FK, UNIQUE, CHECK. Chỉ cập nhật cột Thực tế và kết luận SQL khi script báo thành công. Nếu lỗi kết nối, lưu thông báo nguyên văn để xác định instance hoặc quyền đăng nhập.

Tiêu chí nghiệm thu còn cần kiểm tra trên máy người dùng

Khởi động từ thư mục giải nén mới; tạo quản trị viên đầu tiên trên SQL Server; đăng nhập bằng tài khoản vừa tạo; CRUD dữ liệu thật; đóng/mở ứng dụng và xác nhận dữ liệu được giữ; hoàn tất bộ SQL. Không thay kết quả chưa chạy bằng PASS trong báo cáo.


---

4.5 Kết luận và hướng phát triển


Ứng dụng đã có các thành phần chức năng theo tiêu chí bài tập: MVC, vai trò người dùng, giao diện responsive, CRUD, paging/filtering/AJAX, session/cookie, annotations và REST API. Bảo mật và quốc tế hóa đã có triển khai cụ thể trong mã nguồn. Bản cập nhật hỗ trợ nhập thời hạn cầm, tính ngày đến hạn và cảnh báo chỉ cho hợp đồng đang cầm. Bộ kiểm thử tự động bổ sung bằng chứng cho hành vi ở cấp ứng dụng.

Bài học kỹ thuật là cần tách kiểm chứng build, hành vi HTTP và ràng buộc cơ sở dữ liệu. Build thành công không chứng minh SQL đã cấu hình đúng; kiểm thử InMemory cũng không thay thế transaction và constraint trên SQL Server. Hướng dẫn cài đặt phải đi cùng quy trình tạo schema cho cả DB mới lẫn DB đã có một phần bảng.

Giới hạn hiện tại

Chưa có sổ quỹ, lịch sử thu lãi, nhật ký kiểm toán, khôi phục bản ghi đã xóa, MFA hoặc cơ chế xử lý xung đột khi hai người cùng sửa. Cảnh báo hạn hiện hiển thị trong giao diện, chưa gửi tự động qua email hoặc SMS.

Kịch bản thuyết trình và demo

| Thời lượng | Nội dung |
|---|---|
| 1 phút | Giới thiệu vấn đề và hai vai trò |
| 2 phút | Trình bày use case, ERD và MVC |
| 3 phút | Demo tạo khách hàng, nhập thời hạn hợp đồng, xem ngày đến hạn, lọc cảnh báo và phân trang |
| 1 phút | Demo Staff bị từ chối thao tác xóa và giới thiệu API |
| 1 phút | Kết quả kiểm thử, giới hạn SQL và hướng phát triển |
Cài đặt nhanh

Mở PawnShopManagement.slnx nếu dùng Visual Studio. Cách đơn giản hơn là nhấp đúp Chay-ung-dung.cmd trong File Explorer, chờ Now listening on rồi vào http://localhost:5073/Account/Setup khi chưa có tài khoản. Nếu đã tạo Admin, vào /Account/Login; trang Setup được đóng sau lần thiết lập đầu tiên. Đóng cửa sổ chạy cũ trước khi build lại để tránh khóa file exe.
