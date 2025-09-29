USE QLBanSach;
GO

-- 1) Tạo ROLE nếu chưa có 
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE type = 'R' AND name = 'app_admin')
    CREATE ROLE app_admin;
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE type = 'R' AND name = 'app_employee')
    CREATE ROLE app_employee;
GO

-- 2) Quyền cho ADMIN: toàn quyền trong schema dbo --
GRANT CONTROL ON SCHEMA::dbo TO app_admin;    -- bao gồm toàn bộ quyền trên mọi object dbo
GRANT EXECUTE TO app_admin;                   -- chạy mọi proc/UDF trong DB
GO

-- 3) Quyền cho NHÂN VIÊN 
-- Quyền đọc bảng chính 
GRANT SELECT ON OBJECT::dbo.Sach            TO app_employee;
GRANT SELECT ON OBJECT::dbo.LoaiSach        TO app_employee;
GRANT SELECT ON OBJECT::dbo.HoaDon          TO app_employee;
GRANT SELECT ON OBJECT::dbo.ChiTietHoaDon   TO app_employee;

-- Quyền thao tác nghiệp vụ bán hàng
GRANT INSERT, UPDATE, DELETE ON OBJECT::dbo.HoaDon        TO app_employee;
GRANT INSERT, UPDATE, DELETE ON OBJECT::dbo.ChiTietHoaDon TO app_employee;



-- Cho phép dùng các TVF tìm kiếm
IF OBJECT_ID('dbo.fn_TimKiemSachTheoTen',       'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemSachTheoTen       TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemSachTheoTacGia',    'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemSachTheoTacGia    TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemSachTheoLoai',      'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemSachTheoLoai      TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemLoaiSachTheoTen',   'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemLoaiSachTheoTen   TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemHoaDonTheoTenKH',   'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemHoaDonTheoTenKH   TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemHoaDonTheoSDT',     'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemHoaDonTheoSDT     TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemHoaDonTheoNgay',    'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemHoaDonTheoNgay    TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemSachTongHop',       'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemSachTongHop       TO app_employee;
IF OBJECT_ID('dbo.fn_TimKiemHoaDonTongHop',     'IF') IS NOT NULL GRANT SELECT  ON OBJECT::dbo.fn_TimKiemHoaDonTongHop     TO app_employee;

-- Cho phép chạy scalar function tính toán
IF OBJECT_ID('dbo.fn_TinhThanhTien',        'FN') IS NOT NULL GRANT EXECUTE ON OBJECT::dbo.fn_TinhThanhTien        TO app_employee;
IF OBJECT_ID('dbo.fn_TinhTongTienHoaDon',   'FN') IS NOT NULL GRANT EXECUTE ON OBJECT::dbo.fn_TinhTongTienHoaDon   TO app_employee;

-- (Tùy chọn) Cấp quyền đọc các VIEW nếu tồn tại
IF OBJECT_ID('dbo.vw_DanhSachSach',   'V') IS NOT NULL GRANT SELECT ON OBJECT::dbo.vw_DanhSachSach   TO app_employee;
IF OBJECT_ID('dbo.vw_DanhSachHoaDon', 'V') IS NOT NULL GRANT SELECT ON OBJECT::dbo.vw_DanhSachHoaDon TO app_employee;
IF OBJECT_ID('dbo.vw_ChiTietHoaDon',  'V') IS NOT NULL GRANT SELECT ON OBJECT::dbo.vw_ChiTietHoaDon  TO app_employee;
IF OBJECT_ID('dbo.vw_DanhSachLoaiSach','V') IS NOT NULL GRANT SELECT ON OBJECT::dbo.vw_DanhSachLoaiSach TO app_employee;
IF OBJECT_ID('dbo.vw_LoaiSach',       'V') IS NOT NULL GRANT SELECT ON OBJECT::dbo.vw_LoaiSach       TO app_employee;
GO

-- Tạo login
CREATE LOGIN admin01 WITH PASSWORD = '1';
CREATE USER  admin01 FOR LOGIN admin01;
ALTER ROLE app_admin ADD MEMBER admin01;

CREATE LOGIN nv01 WITH PASSWORD = '2';
CREATE USER  nv01 FOR LOGIN nv01;
ALTER ROLE app_employee ADD MEMBER nv01;
