using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.Core.TagHelpers
{
    [HtmlTargetElement("uploader", Attributes = IdAttribute)]
    public class UploaderTagHelper : TagHelper
    {
        // the name of attribute has been moved to a variable to keep things DRY
        private const string IdAttribute = "dataid";
        private const string TypeAttribute = "type";
        private const string EntityAttribute = "entity";
        private const string FieldAttribute = "field";
        private const string RefreshAttribute = "refresh";
        private const string TagAttribute = "tag";

        [HtmlAttributeName(IdAttribute)]
        public string Id { get; set; }

        [HtmlAttributeName(TypeAttribute)]
        public string Type { get; set; }

        [HtmlAttributeName(EntityAttribute)]
        public string Entity { get; set; }

        [HtmlAttributeName(FieldAttribute)]
        public string Field { get; set; }

        [HtmlAttributeName(RefreshAttribute)]
        public string Refresh { get; set; }

        [HtmlAttributeName(TagAttribute)]
        public string Tag { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.TagMode = TagMode.SelfClosing;
            string classValue = "";
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"].Value, "hood-fileupload ");
            }
            else
            {
                classValue = "hood-fileupload";
            }
            output.Attributes.SetAttribute("class", classValue);
            output.Attributes.SetAttribute("type", "file");
            output.Attributes.SetAttribute("id", "files");
            output.Attributes.SetAttribute("name", "files");
            output.Attributes.SetAttribute("data-id", Id);
            output.Attributes.SetAttribute("data-entity", Entity);
            output.Attributes.SetAttribute("data-field", Field);
            output.Attributes.SetAttribute("data-refresh", Refresh);
            output.Attributes.SetAttribute("data-tag", Tag);
        }
    }
}
