namespace Zen.App.Core.Person
{
    public class PersonAction : IPersonAction
    {
        public string Id { get; set; }
        public string Locator { get; set; }
        public bool Active { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
    }
}