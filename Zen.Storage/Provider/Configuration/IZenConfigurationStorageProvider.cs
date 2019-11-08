﻿using Zen.Storage.Provider.Configuration;

namespace Zen.Storage.Provider.Configuration {
    public interface IZenConfigurationStorageProvider
    {
        void Initialize(ZenConfigurationStorageAttribute config);
        bool IsValid(object sourceModel);
        object Load();
        void Save(object sourceModel);
        string Descriptor { get; }
        void SaveTemplateFile(string name, string source);
        string LoadTemplateFile(string name);
    }
}