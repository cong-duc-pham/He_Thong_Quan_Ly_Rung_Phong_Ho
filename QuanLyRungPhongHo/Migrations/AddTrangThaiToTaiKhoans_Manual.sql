
-- Kiểm tra xem cột TrangThai đã tồn tại chưa

-- Thêm cột mới với giá trị mặc định = 1 (Kích hoạt)
ALTER TABLE [dbo].[TaiKhoans]
ADD [TrangThai] BIT NOT NULL DEFAULT 1
    
PRINT 'Thêm cột TrangThai thành công!'
    
-- Cập nhật tất cả tài khoản hiện có = Kích hoạt
UPDATE [dbo].[TaiKhoans]
SET [TrangThai] = 1
WHERE [TrangThai] IS NULL
    
PRINT 'Cập nhật giá trị mặc định cho các bản ghi cũ thành công!'


