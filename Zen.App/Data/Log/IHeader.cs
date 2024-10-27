using System;
using System.Text;
using Zen.Base;

namespace Zen.App.Data.Log
{

    public interface IHeader
    {
        string Action { get; set; }
        string Type { get; set; }
        string AuthorLocator { get; set; }
        string ReferenceId { get; set; }
        string Message { get; set; }
        DateTime TimeStamp { get; set; }
    }

}
