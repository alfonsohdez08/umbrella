use Northwind;
go

drop procedure if exists dbo.GetOrdersWithSpecificProducts;
go

drop procedure if exists dbo.InsertProducts;
go

drop type if exists dbo.Ids;
go

drop type if exists dbo.ProductType;
go

create type dbo.Ids as table
(
	[Id] int not null
);
go

create procedure dbo.GetOrdersWithSpecificProducts @Ids dbo.Ids readonly
as
begin
	select
		o.OrderID, o.OrderDate
	from dbo.Orders as o
		inner join dbo.[Order Details] as od
			on o.OrderID = od.OrderID
	where od.ProductID in (select [Id] from @Ids);
end;
go

create type dbo.ProductType as table
(
	[Name] nvarchar(40) not null
	, SupplierId int null
	, CategoryId int null
	, [Quantity Per Unit] nvarchar(20) null
	, UnitPrice money not null
	, UnitsInStock smallint null
	, UnitsOnOrder smallint null
	, ReorderLevel smallint null
	, Discontinued bit not null
);
go

create procedure dbo.InsertProducts @Products dbo.ProductType readonly
as
begin
	begin try;
		
		begin transaction;
			insert into dbo.Products
			(ProductName
			, SupplierID
			, CategoryID
			, QuantityPerUnit
			, UnitPrice
			, UnitsInStock
			, UnitsOnOrder
			, ReorderLevel
			, Discontinued
			)
			select
			[Name]
			, SupplierId
			, CategoryId
			, [Quantity Per Unit]
			, UnitPrice
			, UnitsInStock
			, UnitsOnOrder
			, ReorderLevel
			, Discontinued
			from @Products

		commit transaction;
	end try
	begin catch;
		rollback transaction;
	
		throw;
	end catch;
end;
go
