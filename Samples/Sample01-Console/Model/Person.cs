using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Log;

namespace Sample01_Console.Model
{
    public class Person : Data<Person>
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = DateTime.Now.ToString(CultureInfo.InvariantCulture).Md5Hash();
    }
}