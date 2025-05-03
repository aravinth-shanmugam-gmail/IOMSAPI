drop table Customer;
drop table ItemUnit;
drop table OrderStatus;
drop table CourierStatus;
drop table InventoryItem;
drop table SalesOrder;
drop table SalesOrderDetail;
drop table SearchColumns;
drop table SearchOperators;
drop table InventoryImages;
drop table Cart;

CREATE TABLE customer (
    id INT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    addressline VARCHAR(255),
    state VARCHAR(255),
    city VARCHAR(255),
    country VARCHAR(255),
    zipcode VARCHAR(20),
    phone1 VARCHAR(20) NOT NULL UNIQUE,
    phone2 VARCHAR(20),
    email VARCHAR(255) NOT NULL UNIQUE,
    passwordhash VARCHAR(255),
    otpcode VARCHAR(10),
    otpexpiry DATETIME,
    status varchar(30) default('registered') not null
);

create table ItemUnit
(unit varchar(5) primary key, description varchar(20));

insert into ItemUnit values ('g', 'gram');
Insert into ItemUnit values ('ml', 'millilitre');
Insert into ItemUnit values ('l', 'litre');
Insert into ItemUnit values ('kg', 'kilogram');
Insert into ItemUnit values ('#', 'nos');

create table OrderStatus
(status varchar(20) primary key, description varchar(50));

create table CourierStatus
(status varchar(20) primary key, description varchar(50));

create table InventoryItem
(id int identity(1,1) primary key, name varchar(50), description varchar(200), unit varchar(5) FOREIGN key references ItemUnit(unit), minUnit int, pricePerUnit decimal(10,2), imageFilePath varchar(200) null);

CREATE TABLE AdditionalInvImage (
    imageId INT PRIMARY KEY IDENTITY,
    itemId INT NOT NULL FOREIGN key references InventoryItem(id),
    imageData VARCHAR(MAX) NOT NULL,
    imageSortOrder INT null,
    imageDescription VARCHAR(1000) NULL
);

create table SalesOrder
(id int identity(1,1) primary key, createdate datetime default SYSDATETIME(), customerId int foreign key references customer(id), statusdate datetime default SYSDATETIME(), orderstatus varchar(20) FOREIGN key references OrderStatus(status), orderamount decimal (10,2), Discount decimal(10,2), notes varchar(500));

create table SalesOrderDetail
(id int identity(1,1) primary key, orderid int foreign key references SalesOrder(id), inventoryid int FOREIGN key references InventoryItem(id), itemPrice decimal(10,2), quantity decimal(10,2));

create table SearchColumns
(id int identity(1,1) primary key, columnName varchar(30));

insert into SearchColumns (columnName) values
('name'),('state'),('city'),('zipcode');

create table Cart(
    id INT primary key identity,
    customerId int not null FOREIGN key REFERENCES Customer(Id),
    itemId int not null FOREIGN key REFERENCES InventoryItem (id),
    quantity decimal(10,2)
);

CREATE TABLE Payment (
    Id INT PRIMARY KEY IDENTITY,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customer(Id),
    RazorpayOrderId VARCHAR(50) NOT NULL,
    RazorpayPaymentId VARCHAR(50) NULL,
    RazorpaySignature VARCHAR(100) NULL,
    Amount DECIMAL(10, 2) NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'INR',
    Status VARCHAR(20) NOT NULL DEFAULT 'CREATED', -- CREATED, CONFIRMED, FAILED
    CreatedAt DATETIME NOT NULL DEFAULT SYSDATETIME(),
    ConfirmedAt DATETIME NULL
);

---------------------

select * from ItemUnit;

select * from customer;

select * from InventoryItem;
