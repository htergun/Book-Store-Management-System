-- =====================================================
-- BookStoreDB - database creation script (SQL Server)
-- Run this script once in SSMS before starting the app.
-- =====================================================

IF DB_ID('BookStoreDB') IS NULL
    CREATE DATABASE BookStoreDB;
GO

USE BookStoreDB;
GO

-- ---------- TABLE DEFINITIONS ----------

CREATE TABLE Authors (
    AuthorID    INT IDENTITY(1,1) PRIMARY KEY,
    FirstName   NVARCHAR(50) NOT NULL,
    LastName    NVARCHAR(50) NOT NULL,
    Country     NVARCHAR(50)
);

CREATE TABLE Categories (
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Books (
    BookID       INT IDENTITY(1,1) PRIMARY KEY,
    Title        NVARCHAR(150) NOT NULL,
    AuthorID     INT NOT NULL,
    CategoryID   INT NOT NULL,
    Price        DECIMAL(8,2) NOT NULL CHECK (Price > 0),
    Stock        INT NOT NULL DEFAULT 0 CHECK (Stock >= 0),
    PublishYear  INT,
    CONSTRAINT FK_Books_Authors    FOREIGN KEY (AuthorID)   REFERENCES Authors(AuthorID),
    CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

CREATE TABLE Customers (
    CustomerID  INT IDENTITY(1,1) PRIMARY KEY,
    FirstName   NVARCHAR(50) NOT NULL,
    LastName    NVARCHAR(50) NOT NULL,
    Email       NVARCHAR(100) NOT NULL UNIQUE,
    Phone       NVARCHAR(20),
    City        NVARCHAR(50)
);

CREATE TABLE Orders (
    OrderID     INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID  INT NOT NULL,
    BookID      INT NOT NULL,
    Quantity    INT NOT NULL CHECK (Quantity > 0),
    OrderDate   DATE NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Orders_Books     FOREIGN KEY (BookID)     REFERENCES Books(BookID)
);
GO

-- ---------- SAMPLE DATA ----------

INSERT INTO Authors (FirstName, LastName, Country) VALUES
(N'George',  N'Orwell',   N'United Kingdom'),
(N'Agatha',  N'Christie', N'United Kingdom'),
(N'Paulo',   N'Coelho',   N'Brazil'),
(N'Orhan',   N'Pamuk',    N'Turkey'),
(N'Dan',     N'Brown',    N'United States');

INSERT INTO Categories (CategoryName) VALUES
(N'Fiction'), (N'Mystery'), (N'Dystopian'), (N'Thriller');

INSERT INTO Books (Title, AuthorID, CategoryID, Price, Stock, PublishYear) VALUES
(N'1984',                          1, 3,  9.99, 25, 1949),
(N'Animal Farm',                   1, 3,  7.50, 30, 1945),
(N'Murder on the Orient Express',  2, 2, 11.25, 15, 1934),
(N'And Then There Were None',      2, 2, 10.80,  0, 1939),
(N'The Alchemist',                 3, 1,  8.90, 40, 1988),
(N'Eleven Minutes',                3, 1,  9.40, 12, 2003),
(N'My Name Is Red',                4, 1, 12.50,  8, 1998),
(N'Snow',                          4, 1, 11.90, 10, 2002),
(N'The Da Vinci Code',             5, 4, 13.20, 20, 2003),
(N'Angels & Demons',               5, 4, 12.70,  5, 2000);

INSERT INTO Customers (FirstName, LastName, Email, Phone, City) VALUES
(N'Ali',    N'Yilmaz', N'ali.yilmaz@example.com',    N'555-0101', N'Istanbul'),
(N'Ayse',   N'Demir',  N'ayse.demir@example.com',    N'555-0102', N'Ankara'),
(N'Mehmet', N'Kaya',   N'mehmet.kaya@example.com',   N'555-0103', N'Izmir'),
(N'Zeynep', N'Celik',  N'zeynep.celik@example.com',  N'555-0104', N'Istanbul'),
(N'Can',    N'Ozturk', N'can.ozturk@example.com',    N'555-0105', N'Bursa'),
(N'Elif',   N'Sahin',  N'elif.sahin@example.com',    N'555-0106', N'Antalya');

INSERT INTO Orders (CustomerID, BookID, Quantity, OrderDate) VALUES
(1, 1,  2, '2026-10-01'),
(1, 5,  1, '2026-10-01'),
(2, 3,  1, '2026-10-05'),
(3, 1,  1, '2026-10-10'),
(4, 9,  2, '2026-10-12'),
(2, 5,  3, '2026-10-20'),
(3, 7,  1, '2026-11-02'),
(5, 10, 1, '2026-11-05');
GO
