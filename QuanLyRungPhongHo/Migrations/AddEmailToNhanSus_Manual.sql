-- Add Email column to NhanSus table
-- Run this script in SQL Server Management Studio or use Entity Framework migration

-- Check if the Email column already exists
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[NhanSus]') 
    AND name = 'Email'
)
BEGIN
    -- Add Email column
    ALTER TABLE [dbo].[NhanSus]
    ADD [Email] NVARCHAR(256) NULL;
    
    PRINT 'Email column added successfully to NhanSus table';
END
ELSE
BEGIN
    PRINT 'Email column already exists in NhanSus table';
END
GO
