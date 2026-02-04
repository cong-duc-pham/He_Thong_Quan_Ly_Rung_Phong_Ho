-- Migration: Thêm trường GhiChuNoiBo vào bảng NhanSus
-- Ngày: 2026-02-05

-- Kiểm tra và thêm cột GhiChuNoiBo nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhanSus]') AND name = 'GhiChuNoiBo')
BEGIN
    ALTER TABLE [dbo].[NhanSus]
    ADD [GhiChuNoiBo] NVARCHAR(MAX) NULL;
    
    PRINT 'Đã thêm cột GhiChuNoiBo vào bảng NhanSus';
END
ELSE
BEGIN
    PRINT 'Cột GhiChuNoiBo đã tồn tại trong bảng NhanSus';
END
GO
