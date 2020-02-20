using System.Collections.Generic;
using System.Linq;
using Zen.App.Core.Person;
using Zen.App.Provider;

namespace Zen.App.Core.Group
{
    public class ZenGroupAction : IGroupBase, IAction
    {

        private readonly List<string> _systemSuffixes = new List<string> { "_ADM", "_DEV", "_CUR" };
        public bool IsSystemGroup => _systemSuffixes.Any(i => Code.EndsWith(i));

        #region Implementation of IZenAction

        public string Action { get; set; }

        #endregion

        #region Implementation of IDataId

        public string Id { get; set; }

        #endregion

        #region Implementation of IDataCode

        public string Code { get; set; }

        #endregion

        #region Implementation of IDataActive

        public bool Active { get; set; }

        #endregion

        #region Implementation of IZenGroupBase

        public List<string> Permissions { get; set; }
        public string Name { get; set; }
        public bool? FromSettings { get; set; }
        public string ParentId { get; set; }
        public string ApplicationId { get; set; }

        #endregion

        public List<PersonAction> People { get; set; }
    }
}