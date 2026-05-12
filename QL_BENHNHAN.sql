USE master
DROP DATABASE QL_BENHVIEN
GO

CREATE DATABASE QL_BENHVIEN
GO



USE QL_BENHVIEN
GO

SET DATEFORMAT DMY


CREATE TABLE dbo.USERS
(
	userId INT IDENTITY,
	email VARCHAR(200),
	passWord CHAR(200) NOT NULL,
	phone CHAR(20) UNIQUE,
	role NVARCHAR(20) CHECK (role = N'P' OR role = N'D' OR role = N'M') DEFAULT N'P',
	createdAt DATETIME DEFAULT GETDATE(),
	updateAt DATETIME CHECK (updateAt <= GETDATE()),
	CONSTRAINT pk_us PRIMARY KEY (userId),
	CHECK (updateAt IS NULL OR updateAt > createdAt)
)
GO

CREATE TABLE dbo.PATIENTS
(
	patientId INT IDENTITY,
	fullName NVARCHAR(50) NOT NULL,
	gender NVARCHAR(10) CHECK (GENDER = N'Nam' OR GENDER = N'Nữ'),
	dateOfBirth DATE CHECK (DATEOFBIRTH < GETDATE()),
	address NVARCHAR(200),
	phone CHAR(20) UNIQUE,
	bloodType CHAR(5) CHECK (	bloodType = 'A+' OR 
								bloodType = 'A-' OR 
								bloodType = 'B+' OR
								bloodType = 'B-' OR
								bloodType = 'O+' OR
								bloodType = 'O-' OR
								bloodType = 'AB+' OR
								bloodType = 'AB-'),
	height DECIMAL(5, 2),
	weight DECIMAL(5, 2),
	allergies NVARCHAR(200), -- DỊ ỨNG
	chronicDiseases NVARCHAR(200), --BỆNH NỀN	
	createdAt DATETIME DEFAULT GETDATE(),
	updateAt DATETIME CHECK (updateAt <= GETDATE()),
	userId INT,
	CONSTRAINT pk_pt PRIMARY KEY(patientId),
	CONSTRAINT pk_pt_us FOREIGN KEY (userId) REFERENCES dbo.USERS(userId),
	CHECK (updateAt IS NULL OR updateAt > createdAt)
)
GO

CREATE TABLE dbo.SPECIALTIES --CHUYÊN KHOA
(
	specialtyId CHAR(50) NOT NULL,
	specialtyName NVARCHAR(50) NOT NULL UNIQUE,
	description NVARCHAR(200),
	CONSTRAINT pk_spc PRIMARY KEY (specialtyId)
)
GO

CREATE TABLE dbo.DOCTORS
(
	doctorId INT IDENTITY,
	fullName NVARCHAR(200) NOT NULL,
	specialtyId CHAR(50),	-- chuyên khoa
	experienceYears INT CHECK (experienceYears >= 0),
	bio NVARCHAR(500),
	avatarUrl CHAR(500) DEFAULT ('default_image.png'),
	workStartTime TIME CHECK (workStartTime > '06:00' AND workStartTime < '17:00'),
	workEndTime TIME CHECK (workEndTime > '11:00' AND workEndTime <= '20:00'),
	createdAt DATETIME DEFAULT GETDATE(),
	updateAt DATETIME CHECK (updateAt <= GETDATE()),
	userId INT,
	CONSTRAINT pk_dt PRIMARY KEY (doctorId),
	CONSTRAINT fk_dt_spc FOREIGN KEY (specialtyId) REFERENCES dbo.SPECIALTIES(specialtyId),
	CONSTRAINT pk_dt_us FOREIGN KEY (userId) REFERENCES dbo.USERS(userId),
	CHECK (updateAt IS NULL OR updateAt > createdAt),
	CHECK (workStartTime < workEndTime)
)
GO

CREATE TABLE dbo.APPOINTMENTS --đặt lịch khám
(
	appointmentId INT IDENTITY,
	patientId INT NOT NULL,
	doctorId INT NOT NULL,
	appointmentDate DATETIME CHECK (appointmentDate >= GETDATE()) NOT NULL,
	specialtyId CHAR(50) NOT NULL, --kHOA KHÁM BỆNH
	queueNumber INT, --set lại số thứ tự theo từng ngày
	status NVARCHAR(50) CHECK (	status = N'Chờ khám' OR
								status = N'Đã khám' OR
								status = N'Chờ xác nhận' OR
								status = N'Đã xác nhận' OR
								status = N'Đã hủy'),
	note NVARCHAR(200),
	isCover BIT DEFAULT 0,
	price DECIMAL(12,2) CHECK (price >= 0),
	createdAt DATETIME DEFAULT GETDATE(),
	updateAt DATETIME CHECK (updateAt <= GETDATE()),
	CONSTRAINT pk_app PRIMARY KEY (appointmentId),
	CONSTRAINT fk_app_pt FOREIGN KEY (patientId) REFERENCES dbo.PATIENTS(patientId),
	CONSTRAINT fk_app_dt FOREIGN KEY (doctorId) REFERENCES dbo.DOCTORS(doctorId),
	CONSTRAINT fk_app_sp FOREIGN KEY (specialtyId) REFERENCES dbo.SPECIALTIES(specialtyId),
	CHECK (updateAt IS NULL OR updateAt > createdAt)
)
GO

CREATE TABLE dbo.MEDICAL_RECORDS --hồ sơ bệnh án
(
	recordId INT IDENTITY,
	appointmentId INT NOT NULL,
	diagnosis NVARCHAR(200), --chuẩn đoán
	symptoms NVARCHAR(200), --chịu chứng
	treatment NVARCHAR(200), -- phương án điều trị
	isCover BIT, -- BHYT
	percentCover INT CHECK (percentCover >= 0 AND percentCover <= 100), --mức giảm theo BHYT
	createdAt DATETIME DEFAULT GETDATE(),
	CONSTRAINT pk_mr PRIMARY KEY (recordId),
	CONSTRAINT fk_mr_app FOREIGN KEY (appointmentId) REFERENCES dbo.APPOINTMENTS(appointmentId)
)
GO

CREATE TABLE dbo.CHATWITHDOCTOR
(
	chatId INT IDENTITY,
	patientId INT NOT NULL,
	doctorId INT NOT NULL,
	createdAt DATETIME DEFAULT GETDATE(),
	CONSTRAINT pk_c PRIMARY KEY (chatId),
	CONSTRAINT fk_c_pt FOREIGN KEY (patientId) REFERENCES dbo.PATIENTS(patientId),
	CONSTRAINT fk_c_dt FOREIGN KEY (doctorId) REFERENCES dbo.DOCTORS(doctorId),
	CONSTRAINT unq_c UNIQUE(patientId, doctorId)
)
GO

CREATE TABLE dbo.MESSAGEDETAIL
(
	chatId INT NOT NULL,
	messageId INT IDENTITY,
	imageUrl CHAR(500) DEFAULT ('default_image.png'),
	senderId CHAR(50),
	receiverId CHAR(50),
	messageText NVARCHAR(2000),
	createdAt DATETIME DEFAULT GETDATE(),
	CONSTRAINT pk_mess PRIMARY KEY (chatId, messageId),
	CONSTRAINT fk_mess_c FOREIGN KEY (chatId) REFERENCES dbo.CHATWITHDOCTOR(chatId)
)
GO

CREATE TABLE dbo.MEDICATIONS
(
	medicationId CHAR(50) NOT NULL,
	medicineName NVARCHAR(200) NOT NULL UNIQUE,
	dosage DECIMAL(2, 1) CHECK (dosage >= 0), -- liều dùng
	frequency INT CHECK (frequency >= 0), -- số lần /ngày
	startDate DATETIME CHECK (startDate <= GETDATE()), -- ngày sản xuất
	endDate DATETIME, -- hạn sử dụng
	quantity INT CHECK (quantity >= 0),
	unit NVARCHAR(20) CHECK (	unit = N'Viên' OR 
								unit = N'Gói' OR 
								unit = N'Ống' OR 
								unit = N'Túi' OR 
								unit = N'Lọ' OR 
								unit = N'Chai' OR 
								unit = N'Lon' OR 
								unit = N'Tuýp' OR 
								unit = N'Hộp'), -- đơn vị tính
	country NVARCHAR(50),
	manufacturer NVARCHAR(200), -- nhà sản xuất
	price DECIMAL(10,2) CHECK (price >= 0),
	isCover BIT,
	note NVARCHAR(500),
	CONSTRAINT pk_mdc PRIMARY KEY (medicationId),
	CHECK (startDate < endDate)
)
GO

CREATE TABLE dbo.PRESCRIPTIONS
(
	prescriptionId INT IDENTITY,
	recordId INT NOT NULL,
	note NVARCHAR(200),
	createdAt DATETIME DEFAULT GETDATE(),
	CONSTRAINT pk_ps PRIMARY KEY (prescriptionId),
	CONSTRAINT fk_ps_mr FOREIGN KEY (recordId) REFERENCES dbo.MEDICAL_RECORDS(recordId)
)
GO

CREATE TABLE dbo.PRESCRIPTIONDETAIL
(
	prescriptionId INT,
	medicationId CHAR(50) NOT NULL,
	duration INT CHECK (duration >= 0), -- số ngày
	quantity INT CHECK (quantity >= 0),
	note NVARCHAR(100),
	CONSTRAINT pk_psd PRIMARY KEY (prescriptionId, medicationId ),
	CONSTRAINT fk_psd_mdc FOREIGN KEY (medicationId) REFERENCES dbo.MEDICATIONS(medicationId),
	CONSTRAINT fk_psd_ps FOREIGN KEY (prescriptionId) REFERENCES dbo.PRESCRIPTIONS(prescriptionId)
)
GO

CREATE TABLE dbo.SERVICES
(
	serviceId CHAR(50) NOT NULL,
	serviceName NVARCHAR(200) NOT NULL,
	price DECIMAL(12,2) CHECK (price >= 0),
	department NVARCHAR(200),
	description NVARCHAR(200),
	isCover BIT,
	specialtyId CHAR(50) NOT NULL,	-- chuyên khoa
	CONSTRAINT fk_sv_spc FOREIGN KEY (specialtyId) REFERENCES dbo.SPECIALTIES(specialtyId),
	CONSTRAINT pk_sv PRIMARY KEY (serviceId)	
)
GO

CREATE TABLE dbo.PAYMENT
(
	paymentId INT IDENTITY,
	patientId INT,
	paymentType NVARCHAR(100) CHECK (	paymentType = N'Đơn thuốc' OR 
										paymentType = N'Dịch vụ' OR 
										paymentType = N'Đặt lịch'),
	totalAmount DECIMAL(12,2) CHECK (totalAmount >= 0),
	createdAt DATETIME DEFAULT GETDATE(),
	updateAt DATETIME CHECK (updateAt <= GETDATE()),
	CONSTRAINT pk_pm PRIMARY KEY (paymentId),
	CONSTRAINT fk_pm_pt FOREIGN KEY (patientId) REFERENCES dbo.PATIENTS(patientId),
	CHECK (updateAt IS NULL OR updateAt > createdAt)
)
GO

CREATE TABLE dbo.PAYMENTPRESCRIPTION
(
	paymentId INT NOT NULL,
	prescriptionId INT NOT NULL,
	CONSTRAINT pk_pap PRIMARY KEY (paymentId),
	CONSTRAINT fk_pap_pm FOREIGN KEY (paymentId) REFERENCES dbo.PAYMENT(paymentId),
	CONSTRAINT fk_pap_ps FOREIGN KEY (prescriptionId) REFERENCES dbo.PRESCRIPTIONS(prescriptionId)
)
GO

CREATE TABLE dbo.PAYMENTAPPOINTMENT
(
	paymentId INT NOT NULL,
	appointmentId INT NOT NULL,
	CONSTRAINT pk_paa PRIMARY KEY (paymentId),
	CONSTRAINT fk_paa_pm FOREIGN KEY (paymentId) REFERENCES dbo.PAYMENT(paymentId),
	CONSTRAINT fk_paa_ps FOREIGN KEY (appointmentId) REFERENCES dbo.APPOINTMENTS(appointmentId)
)
GO

CREATE TABLE dbo.PAYMENTSERVICE
(
	paymentId INT NOT NULL,
	serviceId CHAR(50) NOT NULL,
	quantity INT CHECK (quantity >= 0) DEFAULT 1,
	CONSTRAINT pk_pas PRIMARY KEY (paymentId, serviceId),
	CONSTRAINT fk_pas_pm FOREIGN KEY (paymentId) REFERENCES dbo.PAYMENT(paymentId),
	CONSTRAINT fk_pas_sv FOREIGN KEY (serviceId) REFERENCES dbo.SERVICES(serviceId)
)
GO





--------------------------
----TRIGGER-----
	--1.CẬP NHẬT TỰ ĐỘNG THỜI GIAN CẬP NHẬT CỦA USERS
CREATE TRIGGER TG_UPDATE_USER ON  dbo.USERS
FOR UPDATE, INSERT
AS
BEGIN
	UPDATE dbo.USERS
	SET updateAt = GETDATE()
	FROM dbo.USERS U
	JOIN inserted I ON I.userId = U.userId
END
GO
-----2.CẬP NHẬT TỰ ĐỘNG THỜI GIAN CẬP NHẬT CỦA BỆNH NHÂN
CREATE TRIGGER TG_UPDATE_PATIENT ON  dbo.PATIENTS
FOR UPDATE, INSERT
AS
BEGIN
	UPDATE dbo.PATIENTS
	SET updateAt = GETDATE()
	FROM dbo.PATIENTS P
	JOIN inserted I ON I.patientId = P.patientId
END
GO
-----3.CẬP NHẬT TỰ ĐỘNG THỜI GIAN CẬP NHẬT CỦA BÁC SĨ
CREATE TRIGGER TG_UPDATE_DOCTOR ON dbo.DOCTORS
FOR UPDATE, INSERT
AS
BEGIN
	UPDATE dbo.DOCTORS
	SET updateAt = GETDATE()
	FROM dbo.DOCTORS D
	JOIN inserted I ON I.doctorId = D.doctorId
END
GO
-----4.CẬP NHẬT TỰ ĐỘNG THỜI GIAN CẬP NHẬT CỦA ĐẶT LỊCH
CREATE TRIGGER TG_UPDATE_APPOINTMENT ON dbo.APPOINTMENTS
FOR UPDATE, INSERT
AS
BEGIN
	UPDATE dbo.APPOINTMENTS
	SET updateAt = GETDATE()
	FROM dbo.APPOINTMENTS A
	JOIN inserted I ON I.appointmentDate = A.appointmentId
END
GO
-----5.CẬP NHẬT TỰ ĐỘNG THỜI GIAN CẬP NHẬT CỦA HÓA ĐƠN
CREATE TRIGGER TG_UPDATE_PAYMENT ON dbo.PAYMENT
FOR UPDATE, INSERT
AS
BEGIN
	UPDATE dbo.PAYMENT
	SET updateAt = GETDATE()
	FROM dbo.PAYMENT P
	JOIN inserted I ON I.paymentId = P.paymentId
END
GO
------6. CẬP NHẬT TỔNG TIỀN HÓA ĐƠN CHO HÓA ĐƠN DỊCH VỤ
CREATE TRIGGER TG_TOTALAMOUNT_SERVICE ON dbo.PAYMENTSERVICE
FOR INSERT, UPDATE, DELETE
AS
BEGIN
	UPDATE dbo.PAYMENT
	SET totalAmount = (	SELECT ISNULL(SUM(price * quantity), 0) 
						FROM dbo.PAYMENTSERVICE P 
						JOIN dbo.SERVICES S ON S.serviceId = P.serviceId 
						WHERE P.paymentId = dbo.PAYMENT.paymentId)
	FROM dbo.PAYMENT 
	WHERE paymentId IN ((	SELECT paymentId FROM inserted) 
							UNION 
						(	SELECT paymentId FROM deleted))
END
GO
------7. CẬP NHẬT TỔNG TIỀN HÓA ĐƠN CHO HÓA ĐƠN ĐẶT LỊCH
CREATE TRIGGER TG_TOTALAMOUNT_APPOINTMENT ON dbo.PAYMENTAPPOINTMENT
FOR INSERT, UPDATE, DELETE
AS
BEGIN
	UPDATE dbo.PAYMENT
	SET totalAmount = (	SELECT ISNULL(SUM(price), 0) 
						FROM dbo.PAYMENTAPPOINTMENT P 
						JOIN dbo.APPOINTMENTS S ON S.appointmentId = P.appointmentId
						WHERE P.paymentId = dbo.PAYMENT.paymentId)
	FROM dbo.PAYMENT 
	WHERE paymentId IN ((	SELECT paymentId FROM inserted) 
							UNION 
						(	SELECT paymentId FROM deleted))
END
GO
------8. CẬP NHẬT TỔNG TIỀN HÓA ĐƠN CHO HÓA ĐƠN THUỐC
CREATE TRIGGER TG_TOTALAMOUNT_PRESCRIPTION ON dbo.PAYMENTPRESCRIPTION
FOR INSERT, UPDATE, DELETE
AS
BEGIN
	UPDATE dbo.PAYMENT
	SET totalAmount = (	SELECT ISNULL(SUM(price * PS.quantity), 0) 
						FROM dbo.PAYMENTPRESCRIPTION P 
						JOIN dbo.PRESCRIPTIONS S ON S.prescriptionId = P.prescriptionId
						JOIN dbo.PRESCRIPTIONDETAIL PS ON PS.prescriptionId = S.prescriptionId
						JOIN dbo.MEDICATIONS M ON M.medicationId = PS.medicationId
						WHERE P.paymentId = dbo.PAYMENT.paymentId)
	FROM dbo.PAYMENT 
	WHERE paymentId IN ((	SELECT paymentId FROM inserted) 
							UNION 
						(	SELECT paymentId FROM deleted))
END
GO
-----9. CẬP NHẬT SỐ LƯỢNG SỬ DỊCH VỤ
CREATE TRIGGER TG_QUANTITY_SERVICE ON dbo.PAYMENTSERVICE
INSTEAD OF INSERT 
AS
BEGIN
	IF EXISTS (	SELECT 1 
				FROM dbo.PAYMENTSERVICE P 
				JOIN inserted I ON I.serviceId = P.serviceId AND I.paymentId = P.paymentId)
		BEGIN
			UPDATE dbo.PAYMENTSERVICE
			SET quantity = P.quantity + ISNULL(I.quantity, 1)
			FROM dbo.PAYMENTSERVICE P 
			JOIN inserted I ON I.paymentId = P.paymentId AND I.serviceId = P.serviceId
		END
	INSERT INTO dbo.PAYMENTSERVICE (paymentId, serviceId, quantity)
	SELECT paymentId, serviceId, ISNULL(quantity, 1)
	FROM inserted I
	WHERE NOT EXISTS (	SELECT 1 
						FROM dbo.PAYMENTSERVICE P 
						WHERE I.serviceId = p.serviceId AND I.paymentId = P.paymentId)
END
GO

	----------------------
	--1. PROCEDURE TÍNH TOÁN SỐ THỨ TỰ KHÁM + XỬ LÝ ĐỒNG THỜI CHỈ NHẬN 1 LÚC 1 LỊCH ĐẶT

CREATE PROCEDURE sp_CreateAppointment
    @patientId INT,
    @doctorId INT,
	@specialty CHAR(50),
    @appointmentDate DATETIME,
    @note NVARCHAR(200),
    @price DECIMAL(12,2)
AS
BEGIN
    DECLARE @max INT;
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRAN;

    SELECT @max = ISNULL(MAX(queueNumber), 0)
    FROM APPOINTMENTS WITH (UPDLOCK, HOLDLOCK)
    WHERE CAST(appointmentDate AS DATE) = CAST(@appointmentDate AS DATE) AND (specialtyId = @specialty) AND (doctorId = @doctorId);

    INSERT INTO APPOINTMENTS(
        patientId,
        doctorId,
		specialtyId,
        appointmentDate,
        queueNumber,
        status,
        note,
        price
    )
    VALUES (
        @patientId,
        @doctorId,
		@specialty,
        @appointmentDate,
        @max + 1,
        N'Chờ xác nhận',
        @note,
        @price
    );

    COMMIT;
END




-- =========================
-- SPECIALTIES
-- =========================
INSERT INTO dbo.SPECIALTIES (specialtyId, specialtyName, description)
VALUES
('SP001', N'Da liễu tổng quát', N'Khám và điều trị các bệnh da cơ bản'),
('SP002', N'Da liễu thẩm mỹ', N'Chăm sóc và điều trị thẩm mỹ da'),
('SP003', N'Da liễu laser', N'Ứng dụng laser trong điều trị da'),
('SP004', N'Da liễu dị ứng miễn dịch', N'Điều trị dị ứng da và miễn dịch'),
('SP005', N'Da liễu nhi', N'Điều trị bệnh da cho trẻ em'),
('SP006', N'Da liễu nhiễm trùng', N'Điều trị nấm, vi khuẩn, virus da'),
('SP007', N'Da liễu điều trị mụn', N'Chuyên sâu về mụn và sẹo mụn'),
('SP008', N'Da liễu tóc và móng', N'Điều trị rụng tóc, nấm móng'),
('SP009', N'Da liễu lão hóa', N'Chống lão hóa và tái tạo da');

-- =========================
-- USERS
-- =========================
INSERT INTO dbo.USERS (email, passWord, phone, role)
VALUES
('dalieu.thuclm@gmail.com', '123456', '0901000001', N'D'),
('dalieu.namnb@gmail.com', '123456', '0901000002', N'D'),
('dalieu.chungpc@gmail.com', '123456', '0901000003', N'D'),
('patient1@gmail.com', '123456', '0909000001', N'P'),
('p2@gmail.com', '123456', '0909000002', N'P'),
('p3@gmail.com', '123456', '0909000003', N'P');

-- =========================
-- DOCTORS
-- =========================
INSERT INTO dbo.DOCTORS
(fullName, specialtyId, experienceYears, bio, workStartTime, workEndTime, userId)
VALUES
(N'Lê Minh Thức', 'SP001', 10, N'Khám da tổng quát', '07:00', '16:00', 1),
(N'Nguyễn Bá Nam', 'SP002', 8, N'Chuyên trị nám, tàn nhang', '08:00', '17:00', 2),
(N'Phùng Chí Chung', 'SP003', 6, N'Chuyên laser da', '08:00', '16:30', 3);

-- =========================
-- SERVICES
-- =========================
INSERT INTO dbo.SERVICES
(serviceId, serviceName, price, department, description, isCover, specialtyId)
VALUES
('SV201', N'Khám da tổng quát', 150000, N'Da liễu tổng quát', N'Khám bệnh da cơ bản', 1, 'SP001'),
('SV202', N'Chăm sóc da mặt', 300000, N'Da liễu thẩm mỹ', N'Chăm sóc da chuyên sâu', 0, 'SP002'),
('SV203', N'Laser trị nám', 600000, N'Da liễu laser', N'Điều trị nám bằng laser', 0, 'SP003'),
('SV204', N'Test dị ứng', 250000, N'Da liễu dị ứng', N'Xét nghiệm dị ứng da', 1, 'SP004'),
('SV205', N'Điều trị mụn', 350000, N'Da liễu mụn', N'Điều trị mụn chuyên sâu', 0, 'SP007'),
('SV206', N'Điều trị rụng tóc', 400000, N'Da liễu tóc', N'Điều trị rụng tóc', 0, 'SP008');

-- =========================
-- PATIENTS
-- =========================
INSERT INTO dbo.PATIENTS
(fullName, gender, dateOfBirth, phone, userId)
VALUES
(N'Nguyễn Văn A', N'Nam', '2000-01-01', '0909000001', 4),
(N'Trần Thị B', N'Nữ', '2001-02-02', '0909000002', 5),
(N'Lê Văn C', N'Nam', '2002-03-03', '0909000003', 6);

-- =========================
-- APPOINTMENTS
-- =========================
DECLARE @day DATETIME = DATEADD(DAY, 1, GETDATE());

EXEC sp_CreateAppointment 1, 1, 'SP001', @day, N'Khám da tổng quát', 150000;
EXEC sp_CreateAppointment 2, 1, 'SP001', @day, N'Tái khám da', 150000;
EXEC sp_CreateAppointment 3, 2, 'SP002', @day, N'Khám nám da', 300000;

-- =========================
-- MEDICAL_RECORDS
-- =========================
INSERT INTO dbo.MEDICAL_RECORDS
(appointmentId, diagnosis, symptoms, treatment, isCover, percentCover)
VALUES
(1, N'Viêm da', N'Ngứa, nổi đỏ', N'Uống thuốc và bôi kem', 1, 80),
(2, N'Mụn trứng cá', N'Nổi mụn viêm', N'Trị mụn chuyên sâu', 0, 0);

-- =========================
-- PRESCRIPTIONS
-- =========================
INSERT INTO dbo.PRESCRIPTIONS (recordId, note)
VALUES
(1, N'Đơn thuốc viêm da'),
(2, N'Đơn thuốc trị mụn');

-- =========================
-- MEDICATIONS
-- =========================
INSERT INTO dbo.MEDICATIONS
(medicationId, medicineName, dosage, frequency, startDate, endDate,
 quantity, unit, country, manufacturer, price, isCover, note)
VALUES
('M001', N'Paracetamol 500mg', 1, 3,
 GETDATE()-100, GETDATE()+365,
 500, N'Viên', N'Việt Nam',
 N'Dược Hậu Giang', 5000, 1,
 N'Giảm đau, hạ sốt'),

('M002', N'Amoxicillin 500mg', 1, 2,
 GETDATE()-200, GETDATE()+300,
 300, N'Viên', N'Việt Nam',
 N'Imexpharm', 8000, 1,
 N'Kháng sinh'),

('M003', N'Vitamin C 500mg', 1, 1,
 GETDATE()-50, GETDATE()+500,
 400, N'Viên', N'Việt Nam',
 N'Trường Thọ', 3000, 0,
 N'Tăng đề kháng'),

('M004', N'Loratadine 10mg', 1, 1,
 GETDATE()-30, GETDATE()+400,
 200, N'Viên', N'Việt Nam',
 N'Sanofi', 7000, 1,
 N'Chống dị ứng'),

('M005', N'Clindamycin Gel', 1, 2,
 GETDATE()-20, GETDATE()+200,
 100, N'Tuýp', N'Pháp',
 N'Galderma', 45000, 0,
 N'Trị mụn'),

('M006', N'Doxycycline 100mg', 1, 2,
 GETDATE()-60, GETDATE()+300,
 150, N'Viên', N'Việt Nam',
 N'Vidipha', 9000, 1,
 N'Kháng sinh trị mụn'),

('M007', N'Ketoconazole Cream', 1, 2,
 GETDATE()-40, GETDATE()+250,
 120, N'Tuýp', N'Ấn Độ',
 N'Cipla', 30000, 1,
 N'Trị nấm da'),

('M008', N'Betamethasone Cream', 1, 2,
 GETDATE()-25, GETDATE()+200,
 100, N'Tuýp', N'Việt Nam',
 N'Boston Pharma', 25000, 0,
 N'Chống viêm da'),

('M009', N'Isotretinoin 10mg', 1, 1,
 GETDATE()-10, GETDATE()+180,
 80, N'Viên', N'Mỹ',
 N'Roche', 15000, 0,
 N'Trị mụn nặng'),

('M010', N'Erythromycin 500mg', 1, 2,
 GETDATE()-90, GETDATE()+300,
 200, N'Viên', N'Việt Nam',
 N'Domesco', 7000, 1,
 N'Kháng sinh');

-- =========================
-- PRESCRIPTIONDETAIL
-- =========================
INSERT INTO dbo.PRESCRIPTIONDETAIL
VALUES
(1, 'M001', 5, 10, N'Uống sau ăn'),
(1, 'M002', 3, 5, N'Uống sáng'),
(2, 'M001', 7, 14, N'Uống tối');

-- =========================
-- PAYMENT
-- =========================
INSERT INTO dbo.PAYMENT
(patientId, paymentType, totalAmount)
VALUES
(1, N'Dịch vụ', 0),
(1, N'Đặt lịch', 0),
(1, N'Đơn thuốc', 0),
(1, N'Đơn thuốc', 0);

-- =========================
-- PAYMENTSERVICE
-- =========================
INSERT INTO dbo.PAYMENTSERVICE
(paymentId, serviceId, quantity)
VALUES
(1, 'SV201', 1),
(1, 'SV203', 2),
(1, 'SV202', 1);

INSERT INTO dbo.PAYMENTSERVICE VALUES (1,'SV201',1);
INSERT INTO dbo.PAYMENTSERVICE VALUES (1,'SV201',2); -- test cộng dồn
INSERT INTO dbo.PAYMENTSERVICE VALUES (1,'SV202',1);

INSERT INTO dbo.PAYMENTAPPOINTMENT VALUES (2,1);
INSERT INTO dbo.PAYMENTAPPOINTMENT VALUES (4,2);

SELECT * FROM dbo.USERS;
SELECT * FROM dbo.PATIENTS;
SELECT * FROM dbo.SPECIALTIES;
SELECT * FROM dbo.DOCTORS;
SELECT * FROM dbo.APPOINTMENTS;
SELECT * FROM dbo.MEDICAL_RECORDS;
SELECT * FROM dbo.CHATWITHDOCTOR;
SELECT * FROM dbo.MESSAGEDETAIL;
SELECT * FROM dbo.MEDICATIONS;
SELECT * FROM dbo.PRESCRIPTIONS;
SELECT * FROM dbo.PRESCRIPTIONDETAIL;
SELECT * FROM dbo.SERVICES;
SELECT * FROM dbo.PAYMENT;
SELECT * FROM dbo.PAYMENTPRESCRIPTION;
SELECT * FROM dbo.PAYMENTAPPOINTMENT;
SELECT * FROM dbo.PAYMENTSERVICE;

