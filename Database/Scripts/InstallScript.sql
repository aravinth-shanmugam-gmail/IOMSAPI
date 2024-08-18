drop table Customer;
drop table ItemUnit;
drop table OrderStatus;
drop table CourierStatus;
drop table InventoryItem;
drop table SalesOrder;
drop table SalesOrderDetail;
drop table SearchColumns;
drop table SearchOperators;

create table Customer
(id int identity(1,1) primary key, name varchar(50), addressline varchar(500), state varchar(50), city varchar(60), country varchar(50), zipcode varchar(10), phone1 varchar(12), phone2 varchar(12), email varchar(100));

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
(id int identity(1,1) primary key, name varchar(50), description varchar(200), unit varchar(5) FOREIGN key references ItemUnit(unit), minUnit int, pricePerUnit decimal(10,2));

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
