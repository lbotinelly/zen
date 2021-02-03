using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Bogus;
using Bogus.DataSets;
using Zen.Base.Module;
using Zen.Base.Module.Data.LINQ;
using Zen.Web.GraphQL.Attribute;

namespace Sample04_AuthenticatedWeb.Model
{
    [GraphQl(Alias = "person")]
    public class SamplePersonDataContext : DataContext<SamplePerson> { }

    public class SamplePerson : Data<SamplePerson>
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        [Display] public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Name.Gender? Gender { get; set; }

        public static List<SamplePerson> Generate(int count = 100)
        {
            var res = new List<SamplePerson>();

            var testSamplePerson = new Faker<SamplePerson>()
                    .RuleFor(o => o.FirstName, (f, u) => f.Name.FirstName())
                    .RuleFor(o => o.LastName, (f, u) => f.Name.LastName())
                    .RuleFor(o => o.Email, (f, u) => f.Internet.Email())
                ;

            for (var i = 0; i < count; i++) res.Add(testSamplePerson.Generate());

            return res;
        }
    }
}