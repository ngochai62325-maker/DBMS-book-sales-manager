use QLBanSach;
go

-- Hàm tính thành tiền cho một chi tiết hóa đơn
create function fn_TinhThanhTien(@soLuong int, @giaBan decimal(10,2))
returns decimal(12,2)
as
begin
    declare @thanhTien decimal(12,2);
    set @thanhTien = @soLuong * @giaBan;
    return @thanhTien;
end;
go

-- Hàm tính tổng tiền cho một hóa đơn
create function fn_TinhTongTienHoaDon(@maHoaDon int)
returns decimal(15,2)
as
begin
    declare @tongTien decimal(15,2);
    
    select @tongTien = sum(ct.SoLuong * s.GiaBan)
    from ChiTietHoaDon ct
    inner join Sach s on ct.MaSach = s.MaSach
    where ct.MaHoaDon = @maHoaDon;
    
    -- Nếu không có chi tiết thì trả về 0
    if @tongTien is null
        set @tongTien = 0;
    
    return @tongTien;
end;
go
