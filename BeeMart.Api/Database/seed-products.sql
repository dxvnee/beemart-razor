IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (Name, Price, Stock, ImageFileName) VALUES
    (N'Indomie Goreng', 3500, 200, NULL),
    (N'Indomie Soto', 3200, 200, NULL),
    (N'Aqua 600ml', 4000, 300, NULL),
    (N'Teh Botol Sosro 450ml', 5500, 150, NULL),
    (N'Coca-Cola 390ml', 6000, 150, NULL),
    (N'Susu Ultra Milk Coklat 250ml', 6500, 120, NULL),
    (N'Kopi Kapal Api Sachet', 1500, 400, NULL),
    (N'Chitato Sapi Panggang', 11000, 100, NULL),
    (N'Taro Net Seaweed', 2500, 150, NULL),
    (N'Biskuit Roma Kelapa', 9000, 100, NULL),
    (N'Roti Tawar Sari Roti', 16000, 60, NULL),
    (N'Gula Pasir 1kg', 17000, 80, NULL),
    (N'Minyak Goreng Bimoli 1L', 20000, 80, NULL),
    (N'Beras Premium 5kg', 75000, 50, NULL),
    (N'Telur Ayam 1kg', 30000, 60, NULL),
    (N'Sabun Mandi Lifebuoy', 4500, 120, NULL),
    (N'Pasta Gigi Pepsodent 75g', 12000, 100, NULL),
    (N'Tissue Paseo 250 Lembar', 14000, 90, NULL),
    (N'Buku Tulis Sidu 38 Lembar', 5000, 150, NULL),
    (N'Pulpen Standard AE7', 3000, 200, NULL);
END
GO