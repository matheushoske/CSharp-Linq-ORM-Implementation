using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    public class Class1 : DbContext
    {
    }
    public class MyDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        // other entity models...

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var a = Customers.Find(1);
        //    optionsBuilder.UseSqlServer("Server=localhost;Database=mydb;Trusted_Connection=True;");
        //}
    }

    public class Customer
    {
        public int Id { get; set; }
    }
}
