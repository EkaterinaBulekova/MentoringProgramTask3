// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 001")]
	    [Description("This sample return all customers whose sum of all orders more then X.")]

	    public void Linq001()
	    {
            decimal X = 104000;

	        var customers = dataSource.Customers
	            .Where(cust => cust.Orders.Select(ord => ord.Total).Sum() > X);

	        foreach (var cust in customers)
	        {
	            ObjectDumper.Write(cust);
	        }
	    }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 002")]
	    [Description("For each customer, make a list of suppliers located in the same country and the same city.")]

	    public void Linq002()
	    {
            // without group
	        var customers =
	            dataSource.Customers
	                .Select(cust =>
	                new
	                {
	                    Cust = cust,
	                    Supl = dataSource.Suppliers.Where(supl =>
	                        supl.Country == cust.Country && supl.City == cust.City)
	                });

            // with group
	        var grcustomer = dataSource.Customers
	            .GroupJoin(
	                dataSource.Suppliers,
	                cust => new { cust.City, cust.Country },
	                supl => new { supl.City, supl.Country },
	                (x, y) => new { cust = x, supls = y });

            foreach (var cust in customers)
            {
                ObjectDumper.Write(cust.Cust);
                foreach (var supl in cust.Supl.ToList())
                {
                    ObjectDumper.Write(supl);
                }
            }

            foreach (var gcust in grcustomer)
            {

                ObjectDumper.Write(gcust.cust);
                foreach (var supl in gcust.supls)
                {
                    ObjectDumper.Write(supl);
                }
            }
        }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 003")]
	    [Description("This sample return all customers whose orders have any more then X.")]

	    public void Linq003()
	    {
	        decimal X = 15000;

	        var customers = dataSource.Customers
	            .Where(cust => cust.Orders.Any(ord => ord.Total > X));

	        foreach (var cust in customers)
	        {
	            ObjectDumper.Write(cust);
	            foreach (var ord in cust.Orders)
	            {
	                ObjectDumper.Write(ord);
	            }
            }
        }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 004")]
	    [Description("This sample return all customers with first order's month and year.")]

	    public void Linq004()
	    {
	        var customers = dataSource.Customers
	            .Where(cust => cust.Orders.Any(o => o.OrderDate > DateTime.MinValue))
	            .Select(cust => new
	            {
	                cust.Orders.Select(o => o.OrderDate).Min().Month,
	                cust.Orders.Select(o => o.OrderDate).Min().Year,
	                cust.CompanyName
	            });
	        foreach (var cust in customers)
	        {
	            ObjectDumper.Write(cust);
	        }
	    }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 005")]
	    [Description("This sample return all customers with first order's month and year sorted by Year, Month, descending by TotalSum and CompanyName.")]

	    public void Linq005()
	    {
	        var customers = dataSource.Customers
	            .Where(cust => cust.Orders.Any(o => o.OrderDate > DateTime.MinValue))
	            .Select(cust => new
	            {
	                cust.Orders.Select(o => o.OrderDate).Min().Month,
	                cust.Orders.Select(o => o.OrderDate).Min().Year,
	                cust.CompanyName,
                    TotalSum = cust.Orders.Select(o => o.Total).Sum()
	            })
	            .OrderBy(c =>  c.Year)
	            .ThenBy( c => c.Month)
	            .ThenByDescending(c => c.TotalSum)
	            .ThenBy(c => c.CompanyName);
	        foreach (var cust in customers)
	        {
	            ObjectDumper.Write(cust);
	        }
	    }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 006")]
	    [Description("This sample return all customers who have a non-digital postal code or the region is not filled or the operator code is not specified in the phone")]

	    public void Linq006()
	    {
	        var rgx = new Regex(@"^\d+$");
	        var customers = dataSource.Customers
	            .Where(cust => string.IsNullOrWhiteSpace(cust.PostalCode)
	                           || !rgx.IsMatch(cust.PostalCode)
	                           || string.IsNullOrWhiteSpace(cust.Region)
	                           || string.IsNullOrWhiteSpace(cust.Phone)
	                           || cust.Phone[0] != '(' );

	        foreach (var cust in customers)
	        {
	            ObjectDumper.Write(cust);
	        }
	    }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 007")]
	    [Description("This sample return all products are grouped by category, inside - by stock, within the last group sort by cost")]

	    public void Linq007()
	    {
	        var products = dataSource.Products.GroupBy(prod => prod.Category)
	            .Select(gprod => new
	            {
	                Category = gprod.Key,
	                Products = gprod
	                    .GroupBy(pr => pr.UnitsInStock)
	                    .Select(p => new
	                    {
	                        InStock = p.Key,
	                        Products = p.OrderBy(_=>_.UnitPrice)

	                    })
	            });

	        foreach (var prod in products)
	        {
	            ObjectDumper.Write(prod);
	            foreach (var pr in prod.Products)
	            {
	                ObjectDumper.Write(pr);
	                foreach (var p in pr.Products)
	                {
	                    ObjectDumper.Write(p);
                    }
                }
            }
	    }


	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 008")]
	    [Description("This sample return all products are grouped by groups 'Cheap', 'Average price', 'Expensive'.")]

	    public void Linq008()
	    {
            var products = dataSource.Products.GroupBy(prod => prod.UnitPrice < 10 ? "cheap" :
                prod.UnitPrice > 60 ? "expensive" : "average")
                .Select(_ => new
                {
                    Type = _.Key,
                    Prods = _
                });

            foreach (var prod in products)
            {
                ObjectDumper.Write(prod);
	            foreach (var pr in prod.Prods)
	            {
	                ObjectDumper.Write(pr);
	            }
            }
        }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 009")]
	    [Description("This sample return the average profitability of each city and the average intensity.")]

	    public void Linq009()
	    {
	        var cityStat = dataSource.Customers.Where(c => c.Orders != null && c.Orders.Length > 0)
	            .GroupBy(cust => cust.City)
	            .Select(city => new
	            {
	                City = city.Key,
	                Avsum = city.Select(cust => cust.Orders.Average(ord => ord.Total)).Sum(),
	                Avcount = city.Select(cust => cust.Orders.Length).Average()
	            });

	        foreach (var city in cityStat)
	        {
	            ObjectDumper.Write(city);
	        }
	    }

	    [Category("Mentoring program Task3")]
	    [Title("Where - Task 010")]
	    [Description("This sample return the average annual activity statistics for clients by months, statistics by years, by years and by months.")]

	    public void Linq010()
	    {
	        var custMonthStat = dataSource.Customers.Select(c => new
	        {
	            c.CompanyName,
	            Ords = c.Orders
	                .GroupBy(o => o.OrderDate.Month)
	                .Select(_ => new
	                {
	                    Month = _.Key,
	                    Activity = 1.0 * _.Count() / _.GroupBy(or=>or.OrderDate.Year).Count()
	                })
	        });
	        foreach (var cStat in custMonthStat)
	        {
	            ObjectDumper.Write(cStat);
	            foreach (var ord in cStat.Ords)
	            {
	                ObjectDumper.Write(ord);
                }
            }

	        var custYearStat = dataSource.Customers.Select(c => new
	        {
	            c.CompanyName,
	            Ords = c.Orders.GroupBy(o => o.OrderDate.Year)
	                .Select(_ => new
	                {
	                    Yaer = _.Key,
	                    Activity = _.Count()
	                })
	        });
            foreach (var cStat in custYearStat)
	        {
	            ObjectDumper.Write(cStat);
	            foreach (var ord in cStat.Ords)
	            {
	                ObjectDumper.Write(ord);
	            }
	        }

            var custMonthYearStat = dataSource.Customers.Select(c => new
            {
                c.CompanyName,
                Years =	c.Orders.GroupBy(o => o.OrderDate.Year)
	                .Select(_ => new
	                {
	                    Year = _.Key,
	                    Months = _.GroupBy(mo => mo.OrderDate.Month).Select(m => new
	                    {
	                        Month = m.Key,
	                        Activity = m.Count()
	                    })
	                })
            });
	        foreach (var cStat in custMonthYearStat)
	        {
	            ObjectDumper.Write(cStat);
	            foreach (var yr in cStat.Years)
	            {
	                ObjectDumper.Write(yr);
	                foreach (var ord in yr.Months)
	                {
	                    ObjectDumper.Write(ord);
	                }
                }
            }
        }
    }
}
