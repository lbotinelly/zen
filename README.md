<p align="center">
  <img src="https://raw.githubusercontent.com/lbotinelly/zen/master/static/res/zen-banner.png">
</p>

Zen is platform-independent .NET Core middleware that simplifies the implementation of data-driven, API-focused apps.

![Publish NuGet packages](https://github.com/lbotinelly/zen/workflows/Publish%20NuGet%20packages/badge.svg)

## Oh, c'mon! Yet another do-it-all framework? Really?

Not quite. Zen provides several features like ORM, caching, encryption and RESTful endpoints out of the box with zero setup in most cases. Think of it like a drop-in middleware that'll get your PoC app working with persistent storage to REST endpoints in no time and with minimal scaffolding - and you can add more features as the need arises.

Its core aspect is the ability to handle data for multiple connectors at the same time, regardless of the target database: Both relational and document (no-SQL) models are supported. Pull data from Oracle and write to Mongo, this kind of stuff.

## Something basic: a database-backed console app

To have it working straight out of the box we need `Zen.Base` (the ORM handler) and a database adapter; let's pick `Zen.Module.Data.LiteDB` for this example.

- [📦 Zen.Base](https://www.nuget.org/packages/Zen.Base/)
- [📦 Zen.Module.Data.LiteDB](https://www.nuget.org/packages/Zen.Module.Data.LiteDB/)

Extremely complicated example ahead. Pay attention!
 - Create a class that inherits from the `Zen.Base.Module.Data<>` generic class;
 - Mark the property you want to use as a unique identifier with the [Key] Data Annotation attribute.

C# example:

    using System.ComponentModel.DataAnnotations;
    using System;
    using Zen.Base.Module;
    
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
- Use the simplified Builder:
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
- Implement a class deriving from `Zen.Web.Data.Controller.DataController<>`, and assign a route to it:
```
[Route("api/people")]
public class PersonController : DataController<Person> {}
```
- ...and that's it.

Now run your project, and reach the endpoint you specified. If you're running the sample provided (`Sample02-REST`), you can try the following URLs:

- **`https://localhost:5001/api/people`**  
```
[{"Id":"1","Name":"Halie","LastName":"Ebert","Email":"Blanca.Koss@gmail.com"},{"Id":"2","Name":"Meta","LastName":"Mayert","Email":"Kyle64@hotmail.com"},{"Id":"3","Name":"Deonte","LastName":"Orn","Email":"Joshua23@gmail.com"},,(...)
```
- **`https://localhost:5001/api/people/1`**  
```
{"Id":"1","Name":"Halie","LastName":"Ebert","Email":"Blanca.Koss@gmail.com"}
```
- **`https://localhost:5001/api/people/new`**  
```
{"Id":"fc8dbb27-42fe-45db-be09-18a84361d509","Name":null,"LastName":null,"Email":null}
```

## Core dependencies

The relational Data module wraps around [Stack Exchange Dapper](https://github.com/StackExchange/dapper-dot-net), an abusively fast IDbConnection interface extender.

## Zen Development Team

- [Leo Botinelly, Bucknell University](https://www.linkedin.com/in/lbotinelly) (http://pt.stackoverflow.com/users/1897/onosendai)

## License
MIT - a permissive free software license originating at the Massachusetts Institute of Technology (MIT), it puts only very limited restriction on reuse and has, therefore, an excellent license compatibility. It permits reuse within proprietary software provided that all copies of the licensed software include a copy of the MIT License terms and the copyright notice.

Check the [LICENSE file](LICENSE.txt) for more details.
