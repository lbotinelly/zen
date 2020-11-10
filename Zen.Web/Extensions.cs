using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Zen.Base.Extension;
using Zen.Storage.Model;

namespace Zen.Web
{
    public static class Extensions
    {
        public static ZenFile ToZenFile(this IFormFile formFile)
        {
            var res = new ZenFile();

            var stream = formFile.OpenReadStream();

            res.FileSize = formFile.Length;
            res.Id = stream.HashGuid();
            res.Locator = res.Id;
            res.OriginalName = formFile.Name;
            res.StorageName = res.Id + "-" + formFile.FileName.ToFriendlyUrl() + Path.GetExtension(formFile.FileName);
            res.MimeType = formFile.ContentType;
            res.Creation = DateTime.Now;
            stream.Position = 0;
            res.Store(stream).Wait();
            res.Save();

            return res;
        }
    }
}