using System.Collections.Generic;

namespace Zen.App.Data.Pipeline.Moderation {
    public interface IModerationAbstractProvider
    {
        bool AllowCustomAbstracts();

        List<string> OnInsertAbstracts();
        List<string> OnUpdateAbstracts();
        List<string> OnRemoveAbstracts();

    }
}