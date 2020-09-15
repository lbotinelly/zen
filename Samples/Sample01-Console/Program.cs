using System;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Data.LINQ;

namespace Sample01_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = new Model.Person();
            model.Save();

            var all = Model.Person.All().ToList();

            foreach (var person in all)
            {
                Console.WriteLine(person.ToJson());
            }

            Console.WriteLine($"Current record count: {Model.Person.Count()}");

            var query = from person in new DataContext<Model.Person>()
                        where person.Name != null
                        select person;

            var results = query.ToList();

            Console.WriteLine($"LINQ query count: {results.Count()}");
        }
    }
}
