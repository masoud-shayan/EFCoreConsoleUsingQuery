using System;
using System.Collections.Generic;
using static System.Console;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using LinqWithEFCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace WorkingWithEFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // QueryingCategories(); 
            // QueryingProducts();
            // QueryingWithLike();
            // AddProduct(6, "masoud shayan", 500M);
            // IncreaseProductPrice("maso" , 10);
            // DeleteProducts("maso");
            // SelectedFilterAndSort();
            // JoinCategoriesAndProducts();
            GroupJoinCategoriesAndProducts();
        }


        //select - where
        static void QueryingCategories()
        {
            using (var db = new Northwind())
            {

                WriteLine("Categories and how many products they have:"); // there is no related Product object to use include method and we use joins later instead of it
                IQueryable<Category> cats = 
                    from c in db.Categories
                    select c;
                
                foreach (Category c in cats)
                {
                    // WriteLine($"{c.CategoryName} has {c.Products.Count}products.");
                    WriteLine(c.CategoryName);
                }
            }
        }


        //select - where
        static void QueryingProducts()
        {
            using (var db = new Northwind())
            {
                WriteLine("Products that cost more than a price, highest at top.");

                string input;
                decimal price;

                do
                {
                    WriteLine("Enter a product price :");
                    input = ReadLine();
                } while (!decimal.TryParse(input, out price));

                Console.WriteLine(price);
                
                // server-side evaluating in EF core 3.1 and works on SQlite because we convert decimal to double type

                IQueryable<Product> prods =
                    from p in db.Products
                    where (double)p.UnitPrice > (double)price
                    orderby (double)p.UnitPrice descending
                    select  p;
                


                foreach (var product in prods)
                {
                    WriteLine(
                        $"pId : {product.ProductID} - pName : {product.ProductName} - pCost : {product.UnitPrice:$#} - PUnit : {product.UnitsInStock} in Stock");
                }
            }
        }


        //select - where with like
        static void QueryingWithLike()
        {
            using (var db = new Northwind())
            {
                WriteLine("Enter part of a product name: ");
                string input = ReadLine();
                


                IQueryable<Product> prods =
                    from p in db.Products
                    where  EF.Functions.Like(p.ProductName, $"%{input}%")
                    select p;

                foreach (var product in prods)
                {
                    WriteLine($"name :{product.ProductID} {product.ProductName} - stock : {product.UnitsInStock} - discounted : {product.Discontinued}");
                }
            }
        }


        // select - orderByDescending 
        static void ListProducts()
        {
            using (var db = new Northwind())
            {
                WriteLine("{0,-3} {1,-35} {2,8} {3,5} {4}",
                    "ID", "Product Name", "Cost", "Stock", "Disc.");

                IQueryable <Product> prods =
                    from p in db.Products
                    orderby (double)p.UnitPrice descending 
                    select p;

                foreach (var item in prods)
                {
                    WriteLine("{0:000} {1,-35} {2,8:$#,##0.00} {3,5} {4}",
                        item.ProductID, item.ProductName, item.UnitPrice,
                        item.UnitsInStock, item.Discontinued);
                }
            }
        }


        // insert
        static void AddProduct(int catergoryID, string productName, decimal? price)
        {
            using (var db = new Northwind())
            {
                var newProduct = new Product
                {
                    CategoryID = catergoryID,
                    ProductName = productName,
                    UnitPrice = price
                };

                db.Products.Add(newProduct);

                int affected = db.SaveChanges();


                if (affected == 1)
                {
                    ListProducts();
                }
                else
                {
                    WriteLine("the last transaction did not execute");
                }
            }
        }


        // update
        static void IncreaseProductPrice(string name, decimal amount)
        {
            using (var db = new Northwind())
            {
                // get first product whose name starts with name


                Product updateProduct =
                    (from p in db.Products
                    where p.ProductName.StartsWith(name)
                    select p).First();
                
                

                updateProduct.UnitPrice += amount;

                int affected = db.SaveChanges();

                if (affected == 1)
                {
                    ListProducts();
                }
                else
                {
                    WriteLine("the last transaction did not execute");
                }
            }
        }


        //delete
        static void DeleteProducts(string name)
        {
            using (var db = new Northwind())
            {
                // IQueryable<Product> deleteProds = db.Products
                //     .Where(product => product.ProductName.StartsWith(name));

                IQueryable<Product> deleteProds =
                    from p in db.Products
                    where p.ProductName.StartsWith(name)
                    select p;

                db.Products.RemoveRange(deleteProds);

                int deleted = db.SaveChanges();
                if (deleted > 0)
                {
                    WriteLine($"{deleted} product(s) were deleted.");
                    ListProducts();
                }
                else
                {
                    WriteLine("the last transaction did not execute");
                }
            }
        }

        
        
        
        
        // select specific columns using select keyword
        static void SelectedFilterAndSort()
        {
            using (var db = new Northwind())
            {
                // Select all column from Database
                
                // IQueryable<Product> query =
                //     from p in db.Products
                //     where (double) p.UnitPrice < (double) 10M
                //     orderby (double)p.UnitPrice descending 
                //     select p;


                // Select the the only columns that we want from Database
                
                var query =
                    from p in db.Products
                    where (double) p.UnitPrice < (double) 10M
                    orderby (double)p.UnitPrice descending 
                    select new
                    {
                        p.ProductID ,
                        p.ProductName,
                        p.UnitPrice
                    };

                

                Console.WriteLine("Products that cost less than $10:");

                foreach (var product in query)
                {
                    Console.WriteLine("{0}: {1} costs {2:$#,##0.00}", product.ProductID, product.ProductName,
                        product.UnitPrice);
                }

                Console.WriteLine();
            }
        }
        
        
        
        
        
        
        // Inner Join 
        static void JoinCategoriesAndProducts()
        {
            using (var db = new Northwind())
            {

                
                var query =
                    from c in db.Categories
                    join p in db.Products on c.CategoryID equals p.CategoryID
                    select new
                    {
                        c.CategoryName,
                        p.ProductName,
                        p.ProductID
                    };
                
                


                foreach (var item in query)
                {
                    Console.WriteLine($"{item.ProductID}: {item.ProductName} is in {item.CategoryName}.");
                }
            }
        }

        
        
        // Left Outer Join or Group Join
        static void GroupJoinCategoriesAndProducts()
        {
            using (var db = new Northwind())
            {
                
                // group all products by their category to return 8 matche

                var queryGroup =
                    from c in db.Categories
                    join p in db.Products 
                        on c.CategoryID equals p.CategoryID into  matchingProducts
                    select new
                    {
                        c.CategoryName ,
                        Products = (from p in matchingProducts orderby p.ProductName select p)
                    };
                


                foreach (var item in queryGroup)
                {
                    Console.WriteLine($"{item.CategoryName} has {item.Products.Count()} products.");
         
                    foreach (var product in item.Products)
                    {
                        Console.WriteLine($"{product.ProductName}");
                    }
                }
            }
        }
        
    }
}