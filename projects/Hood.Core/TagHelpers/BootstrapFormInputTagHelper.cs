using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Hood.Extensions;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.ViewModels
{
    public class BootstrapFormInputModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public BootstrapFormInputLayout Layout { get; set; }
        public string Type { get; set; }
        public string Disabled { get; set; }
        public bool Required { get; internal set; }
    }

    public enum BootstrapFormInputLayout
    {
        Vertical,
        Horizontal,
        Floating
    }
}
namespace Hood.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "bs-for")]
    public class BootstrapFormInputTagHelper : TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// The field which this editor is bound to.
        /// </summary>
        [HtmlAttributeName("bs-for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("bs-layout")]
        public BootstrapFormInputLayout Layout { get; set; } = BootstrapFormInputLayout.Horizontal;

        [HtmlAttributeName("bs-margin")]
        public string Margin { get; set; } = "mb-4";

        [HtmlAttributeName("type")]
        public string Type { get; set; } = "text";

        [HtmlAttributeName("disabled")]
        public bool Disabled { get; set; }

        private IHtmlHelper _htmlHelper;

        public BootstrapFormInputTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            BootstrapFormInputModel model = new BootstrapFormInputModel();

            model.Name = For.Name;
            model.Layout = Layout;
            
            model.Type = Type; // Autodetect type?

            model.Disabled = Disabled ? "disabled" : "";

            var property = For.Metadata
                .ContainerType
                .GetProperty(For.Metadata.Name);

            model.Required = Attribute.IsDefined(property, typeof(RequiredAttribute));

            model.DisplayName = For.Name;
            if (For.ModelExplorer.Metadata.DisplayName.IsSet())
            {
                model.DisplayName = For.ModelExplorer.Metadata.DisplayName;
            }

            if (For.ModelExplorer.Metadata.Description.IsSet())
            {
                model.Description = For.ModelExplorer.Metadata.Description;
            }

            model.Id = Guid.NewGuid().ToString();

            model.Value = For.Model != null ? For.Model.ToString() : "";

            output.TagName = "div";

            string styleValue = "";
            if (output.Attributes.ContainsName("style"))
                styleValue += $"{output.Attributes["style"].Value};";
            output.Attributes.SetAttribute("style", styleValue);

            if (output.Attributes.ContainsName("class"))
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} bootstrap-input {Margin}");
            else
                output.Attributes.SetAttribute("class", $"bootstrap-input {Margin}");

            output.Content.SetHtmlContent(await _htmlHelper.PartialAsync("BootstrapFormInput", model));
        }
    }
}