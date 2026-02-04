-- Insert default shifts if not exist
IF NOT EXISTS (SELECT 1 FROM CaLamViecs WHERE MaCa = 1)
BEGIN
    SET IDENTITY_INSERT CaLamViecs ON;
    
    INSERT INTO CaLamViecs (MaCa, TenCa, GioBatDau, GioKetThuc, MoTa, TrangThai)
    VALUES 
    (1, N'Ca Sáng', '07:00:00', '11:00:00', N'Ca làm việc buổi sáng', 1),
    (2, N'Ca Chiều', '13:00:00', '17:00:00', N'Ca làm việc buổi chiều', 1),
    (3, N'Ca Tối', '18:00:00', '22:00:00', N'Ca làm việc buổi tối', 1);
    
    SET IDENTITY_INSERT CaLamViecs OFF;
END
ELSE
BEGIN
    PRINT 'Shifts already exist';
END

-- Verify
SELECT * FROM CaLamViecs;
