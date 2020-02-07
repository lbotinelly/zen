using System.Collections.Generic;
using Zen.Base.Module;
using Zen.Base.Module.Data;

namespace Zen.Web.Data.Pipeline.Moderation.Shared
{
    public interface ICustomModerationPipeline
    {
        bool AllowCustomAbstracts();
        List<string> OnInsertAbstracts();
        List<string> OnUpdateAbstracts();
        List<string> OnRemoveAbstracts();
        States.ModerationActions GetModerationActions<T>(EActionType current, EActionScope scope, Mutator mutator, T model, T original) where T : Data<T>;
    }
}