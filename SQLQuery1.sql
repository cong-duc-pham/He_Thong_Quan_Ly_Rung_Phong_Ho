-------------------------------------------------------
-- BƯỚC 1: DỮ LIỆU CHO BẢNG DANH MỤC XÃ (DanhMucXas)
-------------------------------------------------------
INSERT INTO DanhMucXas (MaXa, TenXa) VALUES 
('XB01', N'Xã Thanh Hương'),
('XB02', N'Xã Xuân Lộc'),
('XB03', N'Xã Hòa Bình');
GO

-------------------------------------------------------
-- BƯỚC 2: DỮ LIỆU CHO BẢNG DANH MỤC THÔN (DanhMucThons)
-- Lưu ý: Cột 'DanhMucXaMaXa' là KHÓA NGOẠI thực tế theo EF Core
-------------------------------------------------------
INSERT INTO DanhMucThons (MaThon, TenThon, DanhMucXaMaXa, MaXa) VALUES 
('T01', N'Thôn Hòa Phú', 'XB01', 'XB01'),
('T02', N'Thôn Tân Tiến', 'XB01', 'XB01'),
('T03', N'Thôn Lâm Đồng', 'XB02', 'XB02'),
('T04', N'Thôn An Bình', 'XB03', 'XB03');
GO

-------------------------------------------------------
-- BƯỚC 3: DỮ LIỆU CHO BẢNG LÔ RỪNG (LoRungs)
-- Lưu ý: Sử dụng cột 'DanhMucThonMaThon' để liên kết
-- MaLo sẽ tự động sinh: 1, 2, 3, 4, 5
-------------------------------------------------------
INSERT INTO LoRungs (SoTieuKhu, SoKhoanh, SoLo, DanhMucThonMaThon, DienTich, LoaiRung, TrangThai) VALUES 
(101, 1, 1, 'T01', 45.5, N'Rừng phòng hộ đầu nguồn', N'Rừng giàu'),
(101, 2, 1, 'T01', 30.0, N'Rừng sản xuất', N'Rừng trung bình'),
(102, 3, 2, 'T02', 15.2, N'Rừng đặc dụng', N'Rừng phục hồi'),
(201, 5, 1, 'T03', 60.0, N'Rừng phòng hộ đầu nguồn', N'Rừng nghèo'),
(301, 8, 3, 'T04', 50.0, N'Rừng ven biển', N'Rừng giàu');
GO

-------------------------------------------------------
-- BƯỚC 4: DỮ LIỆU CHO BẢNG NHÂN SỰ (NhanSus)
-- Lưu ý: Sử dụng cột 'DanhMucXaMaXa'
-------------------------------------------------------
INSERT INTO NhanSus (HoTen, ChucVu, SDT, DanhMucXaMaXa, MaXa) VALUES 
(N'Nguyễn Văn An', N'Cán bộ lâm nghiệp xã', '0912345678', 'XB01', 'XB01'),
(N'Trần Thị Bình', N'Trưởng thôn', '0987654321', 'XB01', 'XB01'),
(N'Lê Văn Cường', N'Bảo vệ rừng', '0933556788', 'XB02', 'XB02'),
(N'Phạm Minh Duy', N'Quản lý hành chính', '0998877665', 'XB03', 'XB03');
GO

-------------------------------------------------------
-- BƯỚC 5: DỮ LIỆU CHO BẢNG TÀI KHOẢN (TaiKhoans)
-- MaNV tham chiếu đến ID tự tăng ở bước 4 (1, 2, 3, 4)
-------------------------------------------------------
INSERT INTO TaiKhoans (TenDangNhap, MatKhau, Quyen, MaNV) VALUES 
('admin_tinh', '123456', N'Admin_Tinh', 1), 
('user_xb01', '123456', N'QuanLy_Xa', 2),    
('user_xb02', '123456', N'NhanVien_Thon', 3), 
('duy_pm', '123456', N'QuanLy_Xa', 4);
GO

-------------------------------------------------------
-- BƯỚC 6: DỮ LIỆU CHO BẢNG SINH VẬT (SinhVats)
-- Sử dụng cột 'LoRungMaLo' tham chiếu đến ID ở bước 3
-------------------------------------------------------
INSERT INTO SinhVats (TenLoai, LoaiSV, MucDoQuyHiem, LoRungMaLo, MaLo) VALUES 
(N'Lim xẹt', N'Thực vật', N'Sắp nguy cấp', 1, 1),
(N'Gỗ Huỳnh', N'Thực vật', N'Bình thường', 1, 1),
(N'Hươu Sao', N'Động vật', N'Nguy cấp', 2, 2),
(N'Cọp', N'Động vật', N'Cực kỳ nguy cấp', 4, 4),
(N'Thông nước', N'Thực vật', N'Bình thường', 5, 5);
GO

-------------------------------------------------------
-- BƯỚC 7: DỮ LIỆU CHO BẢNG NHẬT KÝ BẢO VỆ (NhatKyBaoVes)
-- Sử dụng 'LoRungMaLo' và 'NhanSuMaNV'
-------------------------------------------------------
INSERT INTO NhatKyBaoVes (NgayGhi, LoaiSuViec, NoiDung, LoRungMaLo, NhanSuMaNV, MaLo, MaNV_GhiNhan, ToaDoGPS) VALUES 
('2023-10-20 08:30:00', N'Tuần tra', N'Kiểm tra tình hình rừng, an toàn.', 1, 1, 1, 1, '16.0544, 108.2020'),
('2023-10-21 14:00:00', N'Phát hiện cây đổ', N'Có 1 cây thông lớn bị gió quật đổ chắn đường', 2, 2, 2, 2, '16.0550, 108.2030'),
('2023-10-22 09:15:00', N'Săn bắt trái phép', N'Phát hiện bẫy thú thô sơ, đã phá bỏ.', 4, 3, 4, 3, '16.0600, 108.2100'),
('2023-10-25 10:00:00', N'Cháy rừng (Cảnh báo)', N'Dộ ẩm thấp, cần tăng cường tuần tra.', 1, 1, 1, 1, '16.0545, 108.2022'),
('2023-10-26 16:30:00', N'Tuần tra', N'Không có sự cố bất thường.', 5, 4, 5, 4, '16.0700, 108.2200');
GO