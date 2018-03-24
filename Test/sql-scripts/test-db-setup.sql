DROP DATABASE IF EXISTS ProductCatalogDemo;
GO

CREATE DATABASE ProductCatalogDemo;
GO

USE ProductCatalogDemo;
GO

DROP TABLE IF EXISTS Product
DROP SEQUENCE IF EXISTS ProductId
GO

CREATE SEQUENCE ProductId AS int START WITH 29
GO
CREATE TABLE Product (
	ProductID int DEFAULT (NEXT VALUE FOR ProductId) PRIMARY KEY,
	Name nvarchar(50) NOT NULL,
	Color nvarchar(15) NULL,
	Size nvarchar(5) NULL,
	Price money NOT NULL,
	Quantity int NULL,
	CompanyID int,
	Data nvarchar(4000),
	Tags nvarchar(4000),
	DateModified datetime2(0) NOT NULL DEFAULT (GETUTCDATE())
)
GO

DECLARE @products NVARCHAR(MAX) = 
N'[{"ProductID":15,"Name":"Adjustable Race","Color":"Magenta","Size":"62","Price":100.0000,"Quantity":75,"CompanyID":1,"Data":{"Type":"Part","MadeIn":"China"},"DateModified":"2016-02-11T21:27:32"},{"ProductID":16,"Name":"Bearing Ball","Color":"Magenta","Size":"62","Price":15.9900,"Quantity":90,"CompanyID":2,"Data":{"ManufacturingCost":11.672700,"Type":"Part","MadeIn":"China"},"Tags":["promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":17,"Name":"BB Ball Bearing","Color":"Magenta","Size":"62","Price":28.9900,"Quantity":80,"CompanyID":3,"Data":{"ManufacturingCost":21.162700,"Type":"Part","MadeIn":"China"},"DateModified":"2016-02-11T21:27:32"},{"ProductID":18,"Name":"Blade","Color":"Magenta","Size":"62","Price":18.0000,"Quantity":45,"CompanyID":4,"Data":{},"Tags":["new"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":19,"Name":"Sport-100 Helmet, Red","Color":"Red","Size":"72","Price":41.9900,"Quantity":38,"CompanyID":3,"Data":{"ManufacturingCost":30.652700,"Type":"Еquipment","MadeIn":"China"},"Tags":["promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":20,"Name":"Sport-100 Helmet, Black","Color":"Black","Size":"72","Price":31.4900,"Quantity":60,"CompanyID":1,"Data":{"ManufacturingCost":22.987700,"Type":"Еquipment","MadeIn":"China"},"Tags":["new","promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":21,"Name":"Mountain Bike Socks, M","Color":"White","Size":"M","Price":560.9900,"Quantity":30,"CompanyID":2,"Data":{"Type":"Clothes"},"Tags":["sales","promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":22,"Name":"Mountain Bike Socks, L","Color":"White","Size":"L","Price":120.9900,"Quantity":20,"CompanyID":3,"Data":{"ManufacturingCost":88.322700,"Type":"Clothes"},"Tags":["sales","promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":23,"Name":"Long-Sleeve Logo Jersey, XL","Color":"Multi","Size":"XL","Price":44.9900,"Quantity":60,"CompanyID":4,"Data":{"ManufacturingCost":32.842700,"Type":"Clothes"},"Tags":["sales","promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":24,"Name":"Road-650 Black, 52","Color":"Black","Size":"52","Price":704.6900,"Quantity":70,"CompanyID":5,"Data":{"Type":"Bike","MadeIn":"UK"},"DateModified":"2016-02-11T21:27:32"},{"ProductID":25,"Name":"Mountain-100 Silver, 38","Color":"Silver","Size":"38","Price":359.9900,"Quantity":45,"CompanyID":1,"Data":{"ManufacturingCost":262.792700,"Type":"Bike","MadeIn":"UK"},"Tags":["promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":26,"Name":"Road-250 Black, 48","Color":"Black","Size":"48","Price":299.0200,"Quantity":25,"CompanyID":2,"Data":{"ManufacturingCost":218.284600,"Type":"Bike","MadeIn":"UK"},"Tags":["new","promo"],"DateModified":"2016-02-11T21:27:32"},{"ProductID":27,"Name":"ML Bottom Bracket","Price":101.2400,"Quantity":50,"CompanyID":3,"Data":{"Type":"Part","MadeIn":"China"},"DateModified":"2016-02-11T21:27:32"},{"ProductID":28,"Name":"HL Bottom Bracket","Price":121.4900,"Quantity":65,"CompanyID":4,"Data":{"ManufacturingCost":88.687700,"Type":"Part","MadeIn":"China"},"DateModified":"2016-02-11T21:27:32"}]'
INSERT INTO Product (ProductID, Name, Color, Size, Price, Quantity, CompanyID, Data, Tags, DateModified)
SELECT ProductID, Name, Color, Size, Price, Quantity, CompanyID, Data, Tags, DateModified
FROM OPENJSON (@products) WITH(
	ProductID int,Name nvarchar(50),Color nvarchar(15),Size nvarchar(5),Price money,Quantity int,CompanyID int,
	Data nvarchar(MAX) AS JSON,Tags nvarchar(MAX) AS JSON,
	DateModified datetime2(0)
)
GO

DROP TABLE IF EXISTS Company
DROP SEQUENCE IF EXISTS CompanyId
GO
CREATE SEQUENCE CompanyId AS int START WITH 4
CREATE TABLE Company (
	CompanyID int PRIMARY KEY DEFAULT (NEXT VALUE FOR CompanyId),
	Name nvarchar(50) NOT NULL,
	Address nvarchar(100) NULL,
	Email nvarchar(50) NULL,
	Phone nvarchar(50) NULL,
	Postcode nvarchar(20) NULL,
)
GO

declare @companies nvarchar(max) = 
N'[{"CompanyID":1,"Name":"A Datum Corporation","Email":"msavic@datum.com","Address":"Suite 10, 183838 Southwest Boulevard, Surrey","Phone":"(381) 555-7639","Postcode":"46077"},{"CompanyID":2,"Name":"Contoso, Ltd.","Email":"zmisic@contoso.com","Address":"Unit 2, 2934 Night Road, Jolimont","Phone":"(360) 555-4901","Postcode":"98253"},{"CompanyID":3,"Name":"Consolidated Messenger","Email":"rputnik@consolidated-messanger.com","Address":"894 Market Day Street, West Mont","Phone":"(415) 555-1105","Postcode":"94101"}]'
INSERT INTO Company (CompanyID, Name, Address, Email, Phone, Postcode)
SELECT CompanyID, Name, Address, Email, Phone, Postcode
FROM OPENJSON (@companies)
	WITH(CompanyID int, Name nvarchar(50), Address nvarchar(100), Email nvarchar(50), Phone nvarchar(50), Postcode nvarchar(20))
GO

CREATE PROCEDURE dbo.ProcWithOutput @in int, @out int OUTPUT
AS BEGIN
	SET @out = @in;
END