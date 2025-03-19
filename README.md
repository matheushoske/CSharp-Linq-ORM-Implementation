## Select your README Language / Selecione o Idioma do README

[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/matheushoske/CSharp-Linq-ORM-Implementation/blob/master/README.md)
[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg)](https://github.com/matheushoske/CSharp-Linq-ORM-Implementation/blob/master/README.pt-br.md)

# Simple .NET ORM Project

Welcome to the Simple .NET ORM Project! This project is designed to offer developers a clear and concise resource for integrating object-relational mapping (ORM) into their .NET applications using LINQ to manage data efficiently. Our ORM solution comes in two variations: `ORMExemploMultiple` supporting multiple database providers and `ORMExemploSingle` which is tailored specifically for MySQL.
This application was designed with the portuguese language, but you can feel free to adapt it to your own language. Maybe I'll make it in english as well depending on the amount of requests.
There is a portuguese article about the implementation:
https://medium.com/@matheushoske/27e62a06e030
## Description

This project provides a basic implementation of an ORM using .NET, demonstrating the construction of a query provider that can translate LINQ queries into SQL commands and map the results back to .NET objects. The core aim is to provide a fundamental understanding and a functional baseline for more extensive or specific use cases.

## Project Structure

- **ORMExemploSingle**
  - This version of the ORM is configured to interact exclusively with MySQL databases. It includes all necessary setups and configurations for managing database operations using MySQL.
- **ORMExemploMultiple**
  - A more versatile version of the ORM, designed to support multiple database providers. This allows the application to interface with various types of databases beyond MySQL, including but not limited to SQL Server, PostgreSQL, and others as configured.

## Features

- **LINQ Compatibility**: Write LINQ queries that are automatically translated to the appropriate SQL for targeted database providers.
- **Database Abstraction**: Minimal changes required when switching between supported databases.
- **Easy Configuration**: Simple setup for database connections and entity mappings.
- **Efficient Querying**: Direct execution of SQL on the database server, reducing the overhead of retrieving large datasets into memory.

## Getting Started

### Prerequisites

1. .NET installed.
2. Appropriate database server installed (MySQL, PostgreSQL, etc., depending on the ORM version you are working with).
3. Connection credentials for the database server.

### Installation

Clone the repository to your local machine:

```bash
git clone https://github.com/matheushoske/CSharp-Linq-ORM-Implementation.git
cd CSharp-Linq-ORM-Implementation
```
### Examples

Here is a simple example of how you can use the ORM to fetch data

```cs
using (var context = new MyDbContext())
{
    var products = context.Products.Where(p => p.Category == "Electronics").ToList();
    foreach(var product in products)
    {
        Console.WriteLine($"Product: {product.Name}, Price: {product.Price}");
    }
}
```
You can find More examples inside the Directory Examples

## Contributing

Contributions to enhance the functionality, adding support for more database providers, or improving the existing codebase are welcome. Please feel free to fork the repository and submit pull requests.