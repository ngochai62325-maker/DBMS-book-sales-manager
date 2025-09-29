USE QLBanSach;
GO


-- TRIGGER: Tự động trừ số lượng sách khi thêm/sửa/xóa chi tiết hóa đơn
-- Xóa trigger cũ nếu tồn tại
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_CapNhatSoLuongSach_ChiTietHoaDon')
BEGIN
    DROP TRIGGER tr_CapNhatSoLuongSach_ChiTietHoaDon;
END
GO

-- Tạo trigger mới
CREATE TRIGGER tr_CapNhatSoLuongSach_ChiTietHoaDon
ON ChiTietHoaDon
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000);
    
    BEGIN TRY
        -- XỬ LÝ INSERT (Thêm chi tiết hóa đơn)
        IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
        BEGIN
            -- Cập nhật số lượng sách (trừ đi số lượng bán)
            UPDATE Sach 
            SET SoLuong = Sach.SoLuong - i.SoLuong
            FROM Sach 
            INNER JOIN inserted i ON Sach.MaSach = i.MaSach;
            
            -- Kiểm tra số lượng sau khi trừ có âm không
            IF EXISTS (
                SELECT 1 
                FROM Sach s
                INNER JOIN inserted i ON s.MaSach = i.MaSach 
                WHERE s.SoLuong < 0
            )
            BEGIN
                -- Lấy thông tin sách có số lượng âm để thông báo
                DECLARE @TenSach NVARCHAR(200), @SoLuongConLai INT, @SoLuongBan INT;
                
                SELECT TOP 1 
                    @TenSach = s.TenSach, 
                    @SoLuongConLai = s.SoLuong + i.SoLuong, -- Số lượng trước khi trừ
                    @SoLuongBan = i.SoLuong
                FROM Sach s
                INNER JOIN inserted i ON s.MaSach = i.MaSach 
                WHERE s.SoLuong < 0;
                
                -- Rollback và thông báo lỗi
                SET @ErrorMessage = N'Không đủ số lượng sách "' + @TenSach + N'". ' +
                                   N'Số lượng còn lại: ' + CAST(@SoLuongConLai AS NVARCHAR(10)) + N', ' +
                                   N'số lượng muốn bán: ' + CAST(@SoLuongBan AS NVARCHAR(10)) + N'.';
                RAISERROR(@ErrorMessage, 16, 1);
                RETURN;
            END
        END
        
        -- XỬ LÝ UPDATE (Sửa chi tiết hóa đơn)
        ELSE IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        BEGIN
            -- Hoàn lại số lượng cũ (cộng lại số lượng đã trừ trước đó)
            UPDATE Sach 
            SET SoLuong = Sach.SoLuong + d.SoLuong
            FROM Sach 
            INNER JOIN deleted d ON Sach.MaSach = d.MaSach;
            
            -- Trừ đi số lượng mới
            UPDATE Sach 
            SET SoLuong = Sach.SoLuong - i.SoLuong
            FROM Sach 
            INNER JOIN inserted i ON Sach.MaSach = i.MaSach;
            
            -- Kiểm tra số lượng sau khi cập nhật có âm không
            IF EXISTS (
                SELECT 1 
                FROM Sach s
                INNER JOIN inserted i ON s.MaSach = i.MaSach 
                WHERE s.SoLuong < 0
            )
            BEGIN
                -- Lấy thông tin sách có số lượng âm để thông báo
                DECLARE @TenSachUpdate NVARCHAR(200), @SoLuongConLaiUpdate INT, @SoLuongBanUpdate INT;
                
                SELECT TOP 1 
                    @TenSachUpdate = s.TenSach, 
                    @SoLuongConLaiUpdate = s.SoLuong + i.SoLuong, -- Số lượng sau khi hoàn lại cũ
                    @SoLuongBanUpdate = i.SoLuong
                FROM Sach s
                INNER JOIN inserted i ON s.MaSach = i.MaSach 
                INNER JOIN deleted d ON s.MaSach = d.MaSach
                WHERE s.SoLuong < 0;
                
                -- Rollback và thông báo lỗi
                SET @ErrorMessage = N'Không đủ số lượng sách "' + @TenSachUpdate + N'". ' +
                                   N'Số lượng có thể bán: ' + CAST(@SoLuongConLaiUpdate AS NVARCHAR(10)) + N', ' +
                                   N'số lượng muốn cập nhật: ' + CAST(@SoLuongBanUpdate AS NVARCHAR(10)) + N'.';
                RAISERROR(@ErrorMessage, 16, 1);
                RETURN;
            END
        END
        
        -- XỬ LÝ DELETE (Xóa chi tiết hóa đơn)
        ELSE IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
        BEGIN
            -- Hoàn lại số lượng đã bán (cộng lại vào kho)
            UPDATE Sach 
            SET SoLuong = Sach.SoLuong + d.SoLuong
            FROM Sach 
            INNER JOIN deleted d ON Sach.MaSach = d.MaSach;
        END
        
    END TRY
    BEGIN CATCH
        -- Xử lý lỗi
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        DECLARE @ErrorProcedure NVARCHAR(128) = ERROR_PROCEDURE();
        DECLARE @ErrorLine INT = ERROR_LINE();
        DECLARE @ErrorMessageCatch NVARCHAR(4000) = ERROR_MESSAGE();
        
        -- Re-raise lỗi với thông tin chi tiết
        SET @ErrorMessage = N'Lỗi trong trigger cập nhật số lượng sách: ' + @ErrorMessageCatch;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO





-- Test để đảm bảo trigger hoạt động
PRINT N'';
PRINT N'=== Kiểm tra trigger hoạt động ===';

-- Kiểm tra danh sách trigger
SELECT 
    name AS TenTrigger,
    object_name(parent_id) AS TenBang,
    is_disabled AS BiVoHieuHoa,
    create_date AS NgayTao
FROM sys.triggers 
WHERE name LIKE '%ChiTietHoaDon%';

PRINT N'Trigger đã sẵn sàng sử dụng!';