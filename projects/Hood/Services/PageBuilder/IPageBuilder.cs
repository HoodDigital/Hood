using Hood.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Services
{
    interface IPageBuilder
    {
        void AddInlineScriptParts(ResourceLocation location, string script);
        void AddScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync);
        void AppendInlineScriptParts(ResourceLocation location, string script);
        void AppendScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync);
        string GenerateInlineScripts(IUrlHelper urlHelper, ResourceLocation location);
        string GenerateScripts(IUrlHelper urlHelper, ResourceLocation location, bool bundleScripts = false);
    }
}