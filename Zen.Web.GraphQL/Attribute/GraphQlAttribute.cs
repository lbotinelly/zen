using System.ComponentModel.DataAnnotations;

namespace Zen.Web.GraphQL.Attribute
{
    public class GraphQlAttribute : System.Attribute
    {
        public string Alias { get; set; }
        public string SetName { get; set; }
        public string Description { get; set; }
    }
}