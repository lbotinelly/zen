using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AutoBogus;
using Bogus;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Sample02_REST.Model
{
    public class Person : Data<Person>
    {
        public static Faker<Person> faker = new AutoFaker<Person>()
            .RuleFor(i => i.Id, i => i.Random.Guid().ToString())
            .RuleFor(i => i.Name, i => i.Name.FirstName())
            .RuleFor(i => i.LastName, i => i.Name.LastName())
            .RuleFor(i => i.Email, i => i.Internet.Email());

        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        [Display] public string Name { get; set; } = DateTime.Now.ToString(CultureInfo.InvariantCulture).Md5Hash();
        public string LastName { get; set; }
        public string Email { get; set; }


        public static Person Random()
        {
            var result = new Person();
            faker.Populate(result);
            return result;
        }
    }
}