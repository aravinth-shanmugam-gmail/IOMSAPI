drop table Customer;
drop table ItemUnit;
drop table OrderStatus;
drop table CourierStatus;
drop table InventoryItem;
drop table SalesOrder;
drop table SalesOrderDetail;
drop table SearchColumns;
drop table SearchOperators;

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
    password_hash VARCHAR(255),
    oauth_provider VARCHAR(50),
    oauth_id VARCHAR(255),
    otp_code VARCHAR(10),
    otp_expiry DATETIME
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

create table SalesOrder
(id int identity(1,1) primary key, createdate datetime default SYSDATETIME(), statusdate datetime default SYSDATETIME(), orderstatus varchar(20) FOREIGN key references OrderStatus(status), orderamount decimal (10,2), Discount decimal(10,2), notes varchar(500));

create table SalesOrderDetail
(id int identity(1,1) primary key, orderid int foreign key references SalesOrder(id), inventoryid int FOREIGN key references InventoryItem(id), quantity decimal(10,2));

create table SearchColumns
(id int identity(1,1) primary key, columnName varchar(30));

insert into SearchColumns (columnName) values
('name'),('state'),('city'),('zipcode');

---------------------

select * from ItemUnit;

select * from customer;


select * from InventoryItem;
