using Hood.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Services
{
    public interface IPageBuilder
    {
        void AddInlineScriptParts(ResourceLocation location, string script);
        void AddScriptParts(ResourceLocation location, string src, bool excludeFromBundle, bool isAsync, bool isDefer);
        void AppendInlineScriptParts(ResourceLocation location, string script);
        void AppendScriptParts(ResourceLocation location, string src, bool excludeFromBundle, bool isAsync, bool isDefer);
        string GenerateInlineScripts(IUrlHelper urlHelper, ResourceLocation location);
        string GenerateScripts(IUrlHelper urlHelper, ResourceLocation location, bool bundleScripts = false);
    }
}