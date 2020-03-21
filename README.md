# Umbrella

![Umbrella](https://github.com/alfonsohdez08/umbrella/workflows/Umbrella/badge.svg?branch=master)
[![Nuget](https://img.shields.io/nuget/v/Umbrella.DataTable)](https://www.nuget.org/packages/Umbrella.DataTable)

Umbrella is a simple library that add the capability of convert an `IEnumerable` instance to a `DataTable` by using [C# projections](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/projection-operations). This saves the time of creating boilerplate code for define the `DataTable` columns and map your data into the `DataTable`.

## Table of Content

- [Installation](#Installation)
- [Usage](#Usage)
    * [Column Settings](#Column-Settings)
        * [Methods](#Methods)
    * [Projections](#Projections)
        * [Projections Cheatsheet](#Varieties-of-projections-Cheatsheet)
- [How it works?](#How-it-works)
- [Why you would use this library?](#Why-you-would-use-this-library)

## Installation

>This library is distributed through NuGet; you can find it as [`Umbrella.DataTable`](https://www.nuget.org/packages/Umbrella.DataTable).

Using Package Manager Console in Visual Studio:

````shell
PM> Install-Package Umbrella.DataTable
````

Using .NET Core CLI:

````shell
> dotnet add package Umbrella.DataTable
````

## Usage

Just import the `Umbrella` namespace in your class, and wherever you have an instance of an `IEnumerable`, you would be able to use the extension method `ToDataTable`; and provide a projection that would correspond to your `DataTable`. See the code snippet below for see a sample of the `ToDataTable` method usage:

````csharp
using Umbrella;

...

public class Order
{
    public long Id {get; set;}
    public string Description {get; set;}
    public decimal Amount {get; set;}
    public decimal Freight {get; set;}
    public bool? IsShipped {get; set;}
}

...

List<Order> orders = GetOrders();

// Here's where Umbrella kicks in and creates your DataTable based on a projection
DataTable ordersTable = orders.ToDataTable(o => new {ID = o.Id, NetAmount = o.Amount + o.Freight, p.IsShipped});
````

The code snippet above would create a `DataTable` where its columns are `ID`, `NetAmount` and `IsShipped`. The data type of a column is inferred by the expression assigned to each property within the projection:

- `ID` is assigned to the expression `o.Id`, and its type is the member accessed type: `long`.
- `NetAmount` is assigned to a sum, and its type is `decimal` because both operands are `decimal`.
- `IsShipped`, as `ID`, is assigned to a member accessing expression, so its type is `bool`.

Notice the projection passed as argument to the `ToDataTable` method: it dictates what are going to be the columns of the `DataTable` and how the data should be mapped to each column within a row:

| Column | Mapping Expression | Description |
| - | - | - |
| `ID` | `o.Id` | The property `Id` value from an `Order` instance would be allocated here |
| `NetAmount` |  `o.Amount + o.Freight` | The sum between the two properties (`Amount` & `Freight`) of an `Order` would be allocated here |
| `IsShipped` | `o.IsShipped` | The property `IsShipped` value from an instance of an `Order` instance would be allocated here |

### Column Settings

In case one of your `DataTable` columns must have spaces embedded, then use the methods provided by `ColumnSettings` class to do so. `ColumnSettings`, as its name implies, it's a class that states the settings for a column: its mapping expression and its name. You can't instantiate it by your own, instead you have to use a method that does so: `ColumnSetings Build<T>(Expression<Func<T>> columnMappingExpression)`. `Build` is a static method that creates a `ColumnSettings` instance with the mapping expression provided as argument through the `columnMappingExpression` parameter. For example:

````csharp
(Order o) => new {ShippedDt = ColumnSettings.Build(() => o.ShippedDate).Name("Shipped Date")}
````

The method `Build` creates a `ColumnSettings` instance that has as mapping expression `o.ShippedDate`, and the `Name` method demands a different name for the column instead of using the property projected name: `Shipped Date` instead of `ShippedDt`. The data type of the column is inferred by the mapping expression provided to the `Build` method.

#### Methods

##### `ColumnSettings.Build<T>(Expression<Func<T>> columnMappingExpression)`

It's a static method that creates a `ColumnSettings` instance with the column mapping expression provided.

````csharp
(Person p) => new {FullName = ColumnSettings.Build(() => p.FirstName + " " + p.LastName)}
````

##### `ColumnSettings.Name(string columnName)`

Sets the name for a column.

````csharp
(Person p) => new {NewId = ColumnSettings.Build(() => p.PersonId).Name("ID")}
````

### Projections

See below multiple projections and their intepretation from a `DataTable` standpoint:

#### A `DataTable` that has one column

````csharp
(Order o) => new {o.Id}
````

or 

````csharp
(Order o) => o.Id
````
>Note: your projection might have or not a `new` operator in it. In case it does not have it, you can project  **only one** member; you can't have multiple members projected tied with an operator different than `new`, like: `o.FirstName + " " + o.LastName` - this projection is invalid because it can't infer the column's name.

#### A `DataTable` that has three columns and two of them are nullable

````csharp
(Order o) => new {ID = o.Id, o.IsShipped, Desc = ColumnSettings.Build(() => o.Description).Name("Order Description")}
````

`IsShipped` is declared as `bool?` so the column generated for it would be nullable; `Description` is declared as a `string` and by default it's considered as nullable - check the mapping expression provided to the `ColumnSettings.Build` method.

##### Varieties of projections (Cheatsheet)

| Projector | Columns | Notes |
| - | - | - |
| `(Order o) => new {o.Id, Desc = o.Description}` | `Id`, `Desc`| - |
| `(Order o) => o` | `Id`, `Description`, `Amount`, `Freight`, `IsShipped` | This is an **implicit projection**. A projection is implicit when you return the projector parameter instead of manipulate the members within the projection - the properties projected are the writable one. An implicit projection is *valid* when your parameter type is a composite type: `class` or `struct` |
| `(Order o) => new Order(){Id = o.Id}` | `Id`| The members initialized are the taken as columns |
| `(int i) => new {Identifier = i}` | `Identifier`| - |
| `(Order o) =>` | `o.Id` | This is called a `single member projection` because it only projects a member. |

>Ensure your projection is a flat -no nested projection, meaning no nested `new` operators- and the data types of each property is a [built-in type](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types).

### How it works?

**Umbrella** takes the projection's **Expression Tree** generated by the .NET Compiler in order to generate the `DataTable` metadata: column names, column data types and column constraints - at the moment it supports if the column allows `null` or not -. It also helps in dumping the data from the `IEnumerable` instance into the `DataTable` by creating a *mapping function* that takes as input an `IEnumerable` item and return a new piece of data. 

#### How Columns are inferred?

When projecting objects, you have as input an object of `type A` and produce an object of `type B` by executing a function that shapes/transforms the input object into another one. This function is known as **projector**. For example, check the projector below:

````csharp
(Order o) => new {ID = o.Id, o.Description, TotalAmount = o.Amount}
````

From a C# standpoint, we're projecting an object where its properties are `ID`, `Description` and `TotalAmount`. From the `DataTable` standpoint, we're demanding a `DataTable` where its columns are `ID`, `Description` and `TotalAmount` too. So basically **a property within the projection equals to a column**.

You have to ensure that your projector outputs an object no matter what, thus you would have properties in hand. There's an exception for this rule, and it's when your input type is already an composite type (`class`/`struct`) and your projector basically projects a member of the object without explicitily creating a new object - without using the `new` operator - . For instance:

````csharp
(Order o) => o.Id
````

The projector above is valid: it demands the creation of a `DataTable` that has a single column: `Id`. However, the projector below is invalid:

````csharp
(int i) => i
````

**Umbrella** can't translate this as a column because it's not even a composite type. It doesn't have state, properties, and so on. In case you want to produce a `DataTable` based on the given collection, then we have to wrap our field into an object by using the `new` operator:

````csharp
(int i) => new {ID = i}
````

Now **Umbrella** sees that it should create a `DataTable` of a single column called `ID`.

>If your input type isn't a complex one, then ensure you use the `new` operator within the projection.

#### How the data is mapped to the `DataTable`?

Notice that the properties encountered in the projection are mapped to an expression: either an expression that access the member of the input type, or an arithmethic expression, or a string operation and so on. Each one of these expressions are used to produce the value of a column within a row when iterating over the `IEnumerable` instance. Each iteration represents a row, and when all the delegates are executed for a specific item, it's appended to the `DataTable` as a row.

Let's assume we have the following a set of Orders and we project them as:

````csharp
List<Order> orders = new List<Order>()
{
    new Order(){Id = 1, Description = "BMX", Amount = 24, Freight = 1, IsShipped = false},
    new Order(){Id = 2, Description = "Toyota Corolla", Amount = 2211, Freight = 12, IsShipped = true},
    new Order(){Id = 3, Description = "Yamaha Bike", Amount = 224, Freight = 4, IsShipped = false}
};

DataTable orderTable = orders.ToDataTable(o => new {o.Id, TotalAmount = o.Amount + o.Freight, Processed = true});
````

We would end with a table like this:

| Id | TotalAmount | Processed |
| - | - | - |
| 1 | 25 | true |
| 2 | 2223 | true |
| 3 | 228 | true |

How we ended with the `DataTable` already filled like the one above? We already know how the column discovery works: *each property within the projection represents a column*, but what about the values listed below each columns... where they come from? They come from the source collection - the `IEnumerable` instance -, but each value is the output of a function that is already mapped to each column. See the data structure below used by **Umbrella** in order to produce the value for each column within a row:

| Column | Mapping function *(seems as a lambda expression)* | Note |
| - | - | - |
| `Id` | `(Order o) => o.Id` | Returns the Order's `Id` value |
| `TotalAmount` | `(Order o) => o.Amount + o.Freight` | Returns the sum of Order's `Amount` and `Freight` |
| `Processed` | `() => true` | Always return `true` (notice it's a parameterless function due to it did not reference the projector's input object) |

When dumping the data into the `DataTable`, the mapping function is looked up by the underlying column, and it's executed by passing as argument - or not - the underlying collection's item.

### Why you would use this library?

The process for create a `DataTable` can be tedious: initialize the table, create the `DataColumn`s by specifying its name and data type, create a `DataRow` per each entry and map the entry's values to the row, and finally append the `DataRow` to the table. This process can be really cumbersome when dealing with a dataset that has numerous columns. That's one of the situations where `Umbrella` takes action: creates `DataTable` with data in from an `IEnumerable` instance by using projections.

I have seen solutions about how to convert a `List` to a `DataTable` where the generic type of the `List` is reflected in order to fetch its properties and use them as columns for the table. Although this solution might satisfy a bunch of scenarios, you're coupling your underlying type to the table generation. The way `Umbrella` generates the table it's different even though it uses reflection behind of scenes: you project an object that would represent your `DataTable` schema and the values that should be allocated in each column. This approach provides you the control of **what are going to be your columns** and **how your data should be mapped to each column when creating a row**. 

The main purpose of writting this library is boost your productivity as Developer and reduce the lines of codes required for manipulate `DataTable`s - which would increase the readibility of your code.
