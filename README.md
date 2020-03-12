<p align="center">
  <img src="https://i.stack.imgur.com/wpwwJ.png">
  # zen
</p>

Zen is platform-independent .NET Core middleware that simplifies the implementation of data-driven, API-focused apps.

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

## Core dependencies

The relational Data module wraps around [Stack Exchange Dapper](https://github.com/StackExchange/dapper-dot-net), an abusively fast IDbConnection interface extender.

## Zen Development Team

- [Leo Botinelly](https://www.linkedin.com/in/lbotinelly) (http://pt.stackoverflow.com/users/1897/onosendai)

## License
MIT - a permissive free software license originating at the Massachusetts Institute of Technology (MIT), it puts only very limited restriction on reuse and has, therefore, an excellent license compatibility. It permits reuse within proprietary software provided that all copies of the licensed software include a copy of the MIT License terms and the copyright notice.

Check the [LICENSE file](https://github.com/lbotinelly/zen/blob/master/LICENSE) for more details.
