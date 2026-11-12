USE BookStoreDB;
GO

-- Q1: All books with their author and category (3-table JOIN)
SELECT b.BookID, b.Title, a.FirstName + ' ' + a.LastName AS Author,
       c.CategoryName, b.Price, b.Stock
FROM Books b
INNER JOIN Authors a    ON b.AuthorID = a.AuthorID
INNER JOIN Categories c ON b.CategoryID = c.CategoryID
ORDER BY b.BookID;

-- Q2: Books cheaper than 10, cheapest first (WHERE + ORDER BY)
SELECT Title, Price
FROM Books
WHERE Price < 10
ORDER BY Price;

-- Q3: Books that are out of stock or almost out of stock (WHERE)
SELECT Title, Stock
FROM Books
WHERE Stock <= 5
ORDER BY Stock;

-- Q4: Number of books and total stock per category (GROUP BY)
SELECT c.CategoryName, COUNT(b.BookID) AS NumberOfBooks, SUM(b.Stock) AS TotalStock
FROM Categories c
INNER JOIN Books b ON c.CategoryID = b.CategoryID
GROUP BY c.CategoryName
ORDER BY NumberOfBooks DESC, c.CategoryName;

-- Q5: All orders with customer name, book title and total price (4-table JOIN)
SELECT o.OrderID, cu.FirstName + ' ' + cu.LastName AS Customer,
       b.Title, o.Quantity, b.Price * o.Quantity AS TotalPrice,
       CONVERT(VARCHAR(10), o.OrderDate, 23) AS OrderDate
FROM Orders o
INNER JOIN Customers cu ON o.CustomerID = cu.CustomerID
INNER JOIN Books b      ON o.BookID = b.BookID
ORDER BY o.OrderDate, o.OrderID;

-- Q6: Top 3 best-selling books (SUM + GROUP BY + TOP)
SELECT TOP 3 b.Title, SUM(o.Quantity) AS TotalSold
FROM Orders o
INNER JOIN Books b ON o.BookID = b.BookID
GROUP BY b.Title
ORDER BY TotalSold DESC, b.Title;

-- Q7: Customers who have never placed an order (LEFT JOIN + IS NULL)
SELECT cu.CustomerID, cu.FirstName, cu.LastName, cu.City
FROM Customers cu
LEFT JOIN Orders o ON cu.CustomerID = o.CustomerID
WHERE o.OrderID IS NULL;

-- Q8: Total money spent by each customer, only if more than 20 (GROUP BY + HAVING)
SELECT cu.FirstName + ' ' + cu.LastName AS Customer,
       SUM(b.Price * o.Quantity) AS TotalSpent
FROM Customers cu
INNER JOIN Orders o ON cu.CustomerID = o.CustomerID
INNER JOIN Books b  ON o.BookID = b.BookID
GROUP BY cu.FirstName, cu.LastName
HAVING SUM(b.Price * o.Quantity) > 20
ORDER BY TotalSpent DESC;
