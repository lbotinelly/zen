using System;
using System.Linq;
using Sample01_Console.Model;
using Zen.Base.Extension;
using Zen.Base.Module.Data.LINQ;

namespace Sample01_Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var model = new Person();
            model.Save();

            var all = Person.All().ToList();

            foreach (var person in all) Console.WriteLine(person.ToJson());

            Console.WriteLine($@"Current record count: {Person.Count()}");

            var query = from person in new DataContext<Person>()
                where person.Name != null
                select person;

            var results = query.ToList();

            Console.WriteLine($@"LINQ query count: {results.Count}");
        }
    }
}