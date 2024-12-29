CREATE DATABASE RequestTT;
GO
USE RequestTT;
GO

-- Bảng Departments 
CREATE TABLE TblDepartments (
    department_id INT IDENTITY(1,1) PRIMARY KEY,
    department_name NVARCHAR(100) NOT NULL,
    status SMALLINT NOT NULL -- trạng thái (đang hoạt động, tạm dừng)
);

-- Bảng Roles 
CREATE TABLE TblRoles (
    role_id INT IDENTITY(1,1) PRIMARY KEY,
    role_name NVARCHAR(100) NOT NULL -- tên nhóm quyền
);

-- Bảng Permissions 
CREATE TABLE TblPermissions (
    permission_id INT IDENTITY(1,1) PRIMARY KEY,
    permission_name NVARCHAR(100) NOT NULL, -- tên quyền hạn
    description NVARCHAR(MAX) -- mô tả
);

-- Bảng Users 
CREATE TABLE TblUsers (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL, -- tên người dùng
    email VARCHAR(100) UNIQUE NOT NULL, -- email
    phone VARCHAR(15) NOT NULL, -- số điện thoại
    username VARCHAR(50) UNIQUE NOT NULL, -- tài khoản
    password VARCHAR(255) NOT NULL, -- mật khẩu
    role_id INT, -- nhóm quyền
    department_id INT, -- phòng ban
    status SMALLINT NOT NULL, -- trạng thái (kích hoạt, chưa kích hoạt)
    is_admin BIT DEFAULT 0, -- có phải admin không (0: không, 1: có)
    reset_token VARCHAR(255), -- mã reset mật khẩu
    reset_token_expiry DATETIME, -- hạn của mã reset mật khẩu
    created_user INT, -- người tạo
    created_at DATETIME, -- ngày tạo
    updated_user INT, -- người cập nhật
    updated_at DATETIME, -- ngày cập nhật
    FOREIGN KEY (role_id) REFERENCES TblRoles(role_id),
    FOREIGN KEY (department_id) REFERENCES TblDepartments(department_id),
    FOREIGN KEY (created_user) REFERENCES TblUsers(user_id),
    FOREIGN KEY (updated_user) REFERENCES TblUsers(user_id)
);

-- Bảng User_Permissions 
CREATE TABLE TblUser_Permissions (
    user_permission_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT, -- id người dùng
    permission_id INT, -- id quyền hạn
    FOREIGN KEY (user_id) REFERENCES TblUsers(user_id),
    FOREIGN KEY (permission_id) REFERENCES TblPermissions(permission_id)
);

-- Bảng Customers 
CREATE TABLE TblCustomers (
    customer_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL, -- tên khách
    email VARCHAR(100) UNIQUE NOT NULL, -- email
    phone VARCHAR(15) NOT NULL, -- số điện thoại
    tax_code VARCHAR(50), -- mã số thuế
    company NVARCHAR(100), -- tên công ty
    product NVARCHAR(100) NOT NULL, -- sản phẩm
    username VARCHAR(50) UNIQUE NOT NULL, -- tài khoản
    password VARCHAR(255) NOT NULL, -- mật khẩu
    status SMALLINT NOT NULL, -- trạng thái (kích hoạt, chưa kích hoạt)
    reset_token VARCHAR(255), -- mã reset mật khẩu
    reset_token_expiry DATETIME, -- hạn của mã reset mật khẩu
    token VARCHAR(255), -- mã tạo tài khoản
    token_expiry DATETIME, -- hạn của mã tạo tài khoản
    created_user INT, -- người tạo
    created_at DATETIME, -- ngày tạo
    updated_user INT, -- người cập nhật
    updated_at DATETIME, -- ngày cập nhật
    FOREIGN KEY (created_user) REFERENCES TblUsers(user_id),
    FOREIGN KEY (updated_user) REFERENCES TblUsers(user_id)
);

-- Bảng Support Requests 
CREATE TABLE TblSupport_Requests (
    request_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT, -- id khách
    department_id INT, -- id phòng ban
    request_details NVARCHAR(MAX) NOT NULL, -- chi tiết yêu cầu
    status SMALLINT NOT NULL, -- trạng thái
    created_at DATETIME NOT NULL, -- ngày tạo
    resolved_at DATETIME, -- ngày hoàn thành
    FOREIGN KEY (customer_id) REFERENCES TblCustomers(customer_id),
    FOREIGN KEY (department_id) REFERENCES TblDepartments(department_id)
);

-- Bảng Request_Transfers 
CREATE TABLE TblRequest_Transfers (
    transfer_id INT IDENTITY(1,1) PRIMARY KEY,
    request_id INT, -- id yêu cầu chuyển giao
    from_department_id INT, -- phòng ban chuyển đi
    to_department_id INT, -- phòng ban tiếp nhận
    priority SMALLINT DEFAULT 1, -- mức độ ưu tiên (1: thấp, 2: TB, 3: cao)
    transferred_by INT, -- id người thực hiện chuyển giao
    transferred_at DATETIME NOT NULL DEFAULT GETDATE(), -- ngày chuyển giao
    note NVARCHAR(MAX), -- ghi chú khi chuyển giao
    FOREIGN KEY (request_id) REFERENCES TblSupport_Requests(request_id),
    FOREIGN KEY (from_department_id) REFERENCES TblDepartments(department_id),
    FOREIGN KEY (to_department_id) REFERENCES TblDepartments(department_id),
    FOREIGN KEY (transferred_by) REFERENCES TblUsers(user_id)
);

-- Bảng Request_Processing 
CREATE TABLE TblRequests_Processing (
    process_id INT IDENTITY(1,1) PRIMARY KEY,
    request_id INT, -- id yêu cầu
    department_id INT, -- id phòng ban xử lý
    is_completed SMALLINT DEFAULT 0, -- trạng thái xử lý (0: chưa hoàn thành, 1: hoàn thành, 2: không xử lý được)
    processed_at DATETIME, -- ngày hoàn thành xử lý
    note NVARCHAR(MAX), -- ghi chú xử lý
    FOREIGN KEY (request_id) REFERENCES TblSupport_Requests(request_id),
    FOREIGN KEY (department_id) REFERENCES TblDepartments(department_id)
);

-- Bảng FAQs 
CREATE TABLE TblFaqs (
    faq_id INT IDENTITY(1,1) PRIMARY KEY,
    question NVARCHAR(255) NOT NULL, -- câu hỏi
    answer NVARCHAR(MAX) NOT NULL, -- trả lời
    created_at DATETIME NOT NULL, -- ngày tạo
    updated_at DATETIME -- ngày cập nhật
);

INSERT INTO TblRoles (role_name)
VALUES 
(N'Admin'),
(N'Nhân viên'),
(N'Khách hàng'),
(N'Quản lý'),
(N'Kỹ thuật viên');

INSERT INTO TblDepartments (department_name, status)
VALUES 
(N'Hỗ trợ kỹ thuật', 1),
(N'Chăm sóc khách hàng', 1),
(N'Bảo trì hệ thống', 1),
(N'Kinh doanh', 1),
(N'Marketing', 1);

INSERT INTO TblPermissions (permission_name, description)
VALUES 
(N'Xem yêu cầu hỗ trợ', N'Cho phép xem các yêu cầu hỗ trợ của khách hàng'),
(N'Giải quyết yêu cầu', N'Cho phép giải quyết các yêu cầu hỗ trợ của khách hàng'),
(N'Thêm người dùng', N'Cho phép thêm mới người dùng hệ thống'),
(N'Quản lý quyền truy cập', N'Quản lý quyền truy cập của người dùng'),
(N'Quản lý khách hàng', N'Quản lý thông tin khách hàng và các yêu cầu hỗ trợ');

-- Insert một vài người dùng với role và department đã có sẵn
INSERT INTO TblUsers (full_name, email, phone, username, password, role_id, department_id, status, is_admin, created_user, created_at, updated_user, updated_at)
VALUES
(N'Nguyễn Văn An', 'nguyenvanan@example.com', '0123456789', 'nguyenvanan', 'password123', 1, 1, 1, 1, 1, GETDATE(), 1, GETDATE()), -- Admin
(N'Trần Thị Bình', 'tranthibinh@example.com', '0123456790', 'tranthibinh', 'password123', 2, 2, 1, 0, 1, GETDATE(), 1, GETDATE()), -- Nhân viên
(N'Lê Minh Công', 'leminhcong@example.com', '0123456791', 'leminhcong', 'password123', 3, 3, 1, 0, 1, GETDATE(), 1, GETDATE()), -- Khách hàng
(N'Phan Thi Dâng', 'phanthidang@example.com', '0123456792', 'phanthidang', 'password123', 4, 4, 1, 0, 1, GETDATE(), 1, GETDATE()), -- Quản lý
(N'Bùi Thị Hòa', 'buithihoa@example.com', '0123456793', 'buithihoa', 'password123', 5, 5, 1, 0, 1, GETDATE(), 1, GETDATE()); -- Kỹ thuật viên

INSERT INTO TblUser_Permissions (user_id, permission_id)
VALUES 
-- Admin được cấp tất cả quyền
(1, 1),
(1, 2),
(1, 3),
(1, 4),
(1, 5),
-- Nhân viên có quyền giải quyết yêu cầu và xem yêu cầu
(2, 1),
(2, 2),
(2, 5),
-- Khách hàng chỉ có quyền xem yêu cầu hỗ trợ
(3, 1),
(3, 5),
-- Quản lý có quyền quản lý khách hàng và giải quyết yêu cầu
(4, 1),
(4, 2),
(4, 5),
-- Kỹ thuật viên có quyền giải quyết yêu cầu kỹ thuật
(5, 1),
(5, 2);

INSERT INTO TblCustomers (full_name, email, phone, tax_code, product, username, password, status, created_user, created_at, updated_user, updated_at)
VALUES 
(N'Nguyễn Hồng Tâm', 'nguyenhongtam@example.com', '0123456794', '3001539671', N'Website Dạy học online', 'nguyenhongtam', 'password123', 1, 1, GETDATE(), 1, GETDATE()),
(N'Trần Minh Thư', 'tranminhthu@example.com', '0123456795', '0302096741', N'Phần mềm lọc giống lúa', 'tranminhthu', 'password123', 1, 1, GETDATE(), 1, GETDATE()),
(N'Lê Minh Châu', 'leminhchau@example.com', '0123456796', '1034095867', N'Phần mềm quản lý figure', 'leminhchau', 'password123', 1, 1, GETDATE(), 1, GETDATE()),
(N'Phan Thi In', 'phanthiin@example.com', '0123456797', '4102938065', N'Shop thời trang', N'phanthiin', 'password123', 1, 1, GETDATE(), 1, GETDATE()),
(N'Bùi Thị Vân Hà', 'buithivanha@example.com', '0123456798', '03350981586', N'Website khu nghỉ dưỡng Healing', 'buithivanha', 'password123', 1, 1, GETDATE(), 1, GETDATE());

INSERT INTO TblSupport_Requests (customer_id, department_id, request_details, status, created_at, resolved_at)
VALUES
(1, 1, N'Vấn đề kỹ thuật', 1, GETDATE(), NULL),
(2, 2, N'Yêu cầu hỗ trợ khách hàng', 1, GETDATE(), NULL),
(3, 3, N'Yêu cầu bảo trì hệ thống', 1, GETDATE(), NULL),
(4, 4, N'Yêu cầu tư vấn dịch vụ', 1, GETDATE(), NULL),
(5, 5, N'Yêu cầu giải quyết sự cố', 1, GETDATE(), NULL);

INSERT INTO TblRequest_Transfers (request_id, from_department_id, to_department_id, priority, transferred_by, transferred_at, note)
VALUES
(1, 1, 2, 1, 1, GETDATE(), N'Chuyển yêu cầu từ bộ phận kỹ thuật sang chăm sóc khách hàng'),
(2, 2, 3, 1, 2, GETDATE(), N'Chuyển yêu cầu từ bộ phận chăm sóc khách hàng sang bảo trì hệ thống'),
(3, 3, 4, 1, 3, GETDATE(), N'Chuyển yêu cầu từ bộ phận bảo trì hệ thống sang kinh doanh'),
(4, 4, 5, 1, 4, GETDATE(), N'Chuyển yêu cầu từ bộ phận kinh doanh sang marketing'),
(5, 5, 1, 1, 5, GETDATE(), N'Chuyển yêu cầu từ bộ phận marketing sang hỗ trợ kỹ thuật');

INSERT INTO TblRequests_Processing (request_id, department_id, is_completed, processed_at, note)
VALUES
(1, 1, 0, GETDATE(), N'Đang xử lý'),
(2, 2, 0, GETDATE(), N'Chưa xử lý'),
(3, 3, 0, GETDATE(), N'Đã xử lý'),
(4, 4, 0, GETDATE(), N'Không xử lý được'),
(5, 5, 0, GETDATE(), N'Đã xử lý');

INSERT INTO TblFaqs (question, answer, created_at, updated_at)
VALUES
(N'Cách đăng ký tài khoản?', N'Để đăng ký tài khoản, vui lòng điền thông tin vào form đăng ký trên website.', GETDATE(), GETDATE()),
(N'Cách khôi phục mật khẩu?', N'Bạn có thể khôi phục mật khẩu qua email đã đăng ký của bạn.', GETDATE(), GETDATE()),
(N'Chính sách bảo mật là gì?', N'Chúng tôi cam kết bảo mật thông tin cá nhân của bạn.', GETDATE(), GETDATE()),
(N'Cách liên hệ hỗ trợ?', N'Bạn có thể liên hệ hỗ trợ qua số điện thoại hoặc email của chúng tôi.', GETDATE(), GETDATE()),
(N'Thời gian hỗ trợ khách hàng?', N'Chúng tôi cung cấp dịch vụ hỗ trợ từ 8h sáng đến 6h chiều mỗi ngày.', GETDATE(), GETDATE());
