using Microsoft.AspNetCore.Mvc;
using Zen.Web.Data.Controller;
using System.Collections.Generic;
using Zen.Base.Module.Data;
using Zen.Web.Auth.Model;
using System.Linq;
using System;

namespace Zen.Web.Auth.Controller
{
    [Route("api/user/favorites")]
    public class FavoritesController : DataController<Favorites>
    {
        private string UserId  => Current.AuthEventHandler?.GetIdentifier(User);

        public override void BeforeModelFetch(EHttpMethod method, EActionType type, ref Mutator mutator, ref string key)
        {
            var _key = key;

            var probe = Favorites.Where(i => i.User == UserId && i.Id == _key).FirstOrDefault();

            if (probe == null) throw new Exception("Invalid Favorite ID");

            base.BeforeModelFetch(method, type, ref mutator, ref key);
        }

        public override void BeforeCollectionAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref IEnumerable<Favorites> model, string key = null)
        {
            mutator.Transform = new QueryTransform() ;
            mutator.Transform.AddFilter(new { User = UserId });

            base.BeforeCollectionAction(method, type, ref mutator, ref model, key);
        }

        public override void BeforeModelAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref Favorites model, Favorites originalModel = null, string key = null)
        {
            model.User = UserId;
            model.Timestamp = DateTime.Now;

            base.BeforeModelAction(method, type, ref mutator, ref model, originalModel, key);
        }

    }
}