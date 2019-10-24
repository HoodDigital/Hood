using Hood.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Services
{
    public interface IPageBuilder
    {
        void AddInlineScript(ResourceLocation location, string script);
        void AddScript(ResourceLocation location, string src, bool bundle, bool isAsync, bool isDefer);
        string GenerateInlineScripts(IUrlHelper urlHelper, ResourceLocation location);
        string GenerateScripts(IUrlHelper urlHelper, ResourceLocation location, bool bundleScripts = false);
    }
}