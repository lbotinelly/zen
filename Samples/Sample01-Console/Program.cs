using System;
using System.Linq;
using Zen.Base.Extension;

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
        }
    }
}
