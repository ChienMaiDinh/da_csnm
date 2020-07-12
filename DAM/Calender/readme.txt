1. Tạo db, tạo 2 table user vs planitem giống trong thư mục Database
2. Dùng Visual Studio dể mở Solution Calender.sln
3. Cấu hình lại file AppConfig trong Project Calender
  <connectionStrings>
    <add name="DefaultConnection" connectionString="Server=DESKTOP-NP05509;Database=QuanLyLichCongTy1;User Id=sa;Password=123456;" providerName="System.Data.SqlClient" />
  </connectionStrings>
-Chỉnh sửa thông tin server name, database name, username, password theo người sử dụng.
4.Chỉnh sửa lại địa chit IPv4 theo server trong 2 file Form1.cs và QuanLyLichNhanVien.cs
    IP = new IPEndPoint(IPAddress.Parse("192.168.1.111"), 6740);
chạy ipconfig coi IP của mình rồi sửa lại
5.Build Project
6.Truy cập đường dẫn \Calender\Server\bin\Debug, sau dó chạy chuong trình Server.exe
7.Truy cập đường dẫn \Calender\Calender\big\Debug, sau dó chạy chuong trình Calender.exe(có thể mở nhiều lần)
    -Cửa sổ đăng nhập hiện lên
    -Ðang nhập với tài khoản admin , password admin để đăng nhập với phân quyền quản lý.
    -Đăng nhập với tài khoản hieutran, password 1 để đăng nhập với phân quyền người dùng.
8.kích vào ngày trong lịch để xem lịch ngày, thêm công việc(đối với quản lý), chỉnh sửa trạng thái công việc(đối với người dùng).

bật server lên 
rồi đăng nhập 2 nick 1 cái admin, 1 cái user
