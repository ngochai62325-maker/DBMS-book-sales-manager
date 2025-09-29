create database QLBanSach
go

use QLBanSach;
go

-- Bảng LoaiSach
CREATE TABLE LoaiSach (
    MaLoaiSach INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiSach NVARCHAR(100) NOT NULL
);

-- Bảng Sach
CREATE TABLE Sach (
    MaSach INT PRIMARY KEY IDENTITY(1,1),
    TenSach NVARCHAR(200) NOT NULL,
    MaLoaiSach INT NOT NULL,
    TacGia NVARCHAR(100) NOT NULL,
    SoLuong INT CHECK (SoLuong >= 0),
    GiaBan DECIMAL(10,2) CHECK (GiaBan >= 0),
    FOREIGN KEY (MaLoaiSach) REFERENCES LoaiSach(MaLoaiSach)
);

-- Bảng HoaDon
CREATE TABLE HoaDon (
    MaHoaDon INT PRIMARY KEY IDENTITY(1,1),
    NgayLapHoaDon DATE NOT NULL,
    TenKhachHang NVARCHAR(100) NOT NULL,
    SDT NVARCHAR(20)
);

-- Bảng ChiTietHoaDon
CREATE TABLE ChiTietHoaDon (
    MaHoaDon INT,
    MaSach INT,
    SoLuong INT CHECK (SoLuong > 0),
    PRIMARY KEY (MaHoaDon, MaSach),
    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);



-- Thêm dữ liệu vào LoaiSach
INSERT INTO LoaiSach (TenLoaiSach)
VALUES 
(N'Tiểu thuyết'),
(N'Khoa học'),
(N'Thiếu nhi');


-- Thêm dữ liệu vào Sach
INSERT INTO Sach (TenSach, MaLoaiSach, TacGia, SoLuong, GiaBan)
VALUES 
(N'Đắc Nhân Tâm', 1, N'Dale Carnegie', 50, 75000),
(N'Lược sử thời gian', 2, N'Stephen Hawking', 30, 120000),
(N'Truyện cổ tích Việt Nam', 3, N'Nhiều tác giả', 100, 45000);


-- Thêm dữ liệu vào HoaDon
INSERT INTO HoaDon (NgayLapHoaDon, TenKhachHang, SDT)
VALUES 
('2025-09-24', N'Nguyễn Văn A', '0909123456'),
('2025-09-25', N'Trần Thị B', '0912345678');


-- Thêm dữ liệu vào ChiTietHoaDon
INSERT INTO ChiTietHoaDon (MaHoaDon, MaSach, SoLuong)
VALUES 
(1, 1, 2),   -- Hóa đơn 1 mua 2 cuốn sách 1
(1, 2, 1),   -- Hóa đơn 1 mua 1 cuốn sách 2
(2, 3, 5);   -- Hóa đơn 2 mua 5 cuốn sách 3

