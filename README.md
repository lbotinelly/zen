<p align="center">
  <img src="https://raw.githubusercontent.com/lbotinelly/zen/master/static/res/zen-banner.png">
</p>

Zen is platform-independent .NET Core middleware that simplifies the implementation of data-driven, API-focused apps.

![Publish NuGet packages](https://github.com/lbotinelly/zen/workflows/Publish%20NuGet%20packages/badge.svg)

## Yet another do-it-all framework? Really?

Not quite. Zen provides several features like ORM, caching, encryption and RESTful endpoints out of the box with zero setup in some cases. Think of it like a drop-in package set that'll get your app working with persistent storage to REST endpoints in no time and with minimal scaffolding.

Its core aspect is the ability to handle data for multiple connectors at the same time, regardless of the target database: Both relational and document (no-SQL) models are supported.

## Installation

To have it working straight out of the box with no setup required, add a reference to `Zen.Base` and `Zen.Module.Data.LiteDB`; Compile from source, or check NuGet for these packages:

- [📦 Zen.Base](https://www.nuget.org/packages/Zen.Base/)
- [📦 Zen.Module.Data.LiteDB](https://www.nuget.org/packages/Zen.Module.Data.LiteDB/)

## Usage

Extremely complicated example ahead, you better pay attention!
 - Create a class that inherits from the `Data<>` generic class;
 - Mark the property you want to use as a unique identifier with the [Key] Data Annotation attribute.

C# example:

    using System.ComponentModel.DataAnnotations;
    using System;
    
    namespace Test
    {
        public class Person : Data<Person>
        {
            [Key]
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }

## W-wait, what have I just done?

Congratulations! You created a LiteDB-backed ORM class. A default database was created, together with a collection to store entries.

## You mentioned something about REST

Oh, REST! Right. So, once you decide you want to expose your ORM class data through a REST endpoint, do this:

- Add a reference to [📦 Zen.Web](https://www.nuget.org/packages/Zen.Web/)
- Implement a class deriving from `Zen.Web.Data.Controller.DataController<>`, and assign a route to it:
```
[Route("api/people")]
public class PersonController : DataController<Person> {}
```
- Add Zen to the Service collection and configure it with the Application builder:
```
using Zen.Base.Service.Extensions;
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    { services.AddZen(); }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    { app.UseZen(); }
}
```
- Optionally use the simplified Builder:
```
using Zen.Web.Host;
namespace Sample
{
    public class Program
    {
        public static void Main(string[] args)
        { Builder.Start<Startup>(args); }
    }
}
```
- ...and that's it.

Now run your project, and reach the endpoint you specified. If you're running the sample provided (`Nyan.Samples.REST`), you can try the following URLs:

- **`http://localhost/Nyan.Samples.REST/users`**  
 ```
[{"id":1,"Name":"Maximus Howell III","Surname":null,"isAdmin":false,"BirthDate":"2002-05-13T00:00:00"},{"id":2,"Name":"Odie Yost","Surname":null,"isAdmin":false,"BirthDate":"1989-04-21T00:00:00"},{"id":3,"Name":"Vincent Pouros","Surname":null,"isAdmin":true,"BirthDate":"2002-02-23T00:00:00"},{"id":4,"Name":"Russel Fadel","Surname":null,(...)
```
- **`http://localhost/Nyan.Samples.REST/users/1`**  
 ```
{"id":1,"Name":"Maximus Howell III","Surname":null,"isAdmin":false,"BirthDate":"2002-05-13T00:00:00"}
```

- **`http://localhost/Nyan.Samples.REST/users/new`**  
 `{"id":0,"Name":null,"Surname":null,"isAdmin":false,"BirthDate":null}`  






## Core dependencies

The relational Data module wraps around [Stack Exchange Dapper](https://github.com/StackExchange/dapper-dot-net), an abusively fast IDbConnection interface extender.

## Zen Development Team

- [Leo Botinelly](https://www.linkedin.com/in/lbotinelly) (http://pt.stackoverflow.com/users/1897/onosendai)

## License
MIT - a permissive free software license originating at the Massachusetts Institute of Technology (MIT), it puts only very limited restriction on reuse and has, therefore, an excellent license compatibility. It permits reuse within proprietary software provided that all copies of the licensed software include a copy of the MIT License terms and the copyright notice.

Check the [LICENSE file](LICENSE.txt) for more details.
