-- Migration để cập nhật độ dài cột MaThon từ 10 lên 20 ký tự
-- Chạy thủ công trong SQL Server Management Studio hoặc qua dotnet

-- Kiểm tra nếu cột có ràng buộc
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LoRungs_DanhMucThons_MaThon')
BEGIN
    -- Tạm thời drop foreign key
    ALTER TABLE [LoRungs] DROP CONSTRAINT [FK_LoRungs_DanhMucThons_MaThon];
END

-- Thay đổi độ dài cột MaThon
ALTER TABLE [DanhMucThons] ALTER COLUMN [MaThon] NVARCHAR(20) NOT NULL;

-- Thay đổi độ dài cột MaThon trong bảng LoRungs nếu có
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LoRungs' AND COLUMN_NAME = 'MaThon')
BEGIN
    ALTER TABLE [LoRungs] ALTER COLUMN [MaThon] NVARCHAR(20) NULL;
END

-- Tái tạo foreign key
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LoRungs_DanhMucThons_MaThon')
BEGIN
    ALTER TABLE [LoRungs]
    ADD CONSTRAINT [FK_LoRungs_DanhMucThons_MaThon]
    FOREIGN KEY ([MaThon]) REFERENCES [DanhMucThons]([MaThon])
    ON DELETE SET NULL;
END

PRINT 'Đã cập nhật độ dài cột MaThon thành công!';
