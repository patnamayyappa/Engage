using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common
{
    public interface ILanguageService
    {
        void TranslatePicklistValues<T>(IList<T> records, int? languageCode)
            where T : Entity;
        Dictionary<string, string> Get(IList<string> keys, int? languageCode);
        string Get(string key, Guid? userId = null);
        Dictionary<string, string> Get(IList<string> keys, Guid? userId = null);
        int? GetCurrentUserLanguageCode(Guid? userId = null);

        void RetrieveMultiLingualValues(Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext context);
    }
}
