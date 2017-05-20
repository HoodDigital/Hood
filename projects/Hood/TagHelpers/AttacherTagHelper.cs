using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("attacher", Attributes = IdAttribute)]
    public class AttacherTagHelper : TagHelper
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
            output.TagName = "button";
            output.TagMode = TagMode.StartTagAndEndTag;
            string classValue = "";
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"].Value, "hood-image-attach ");
            }
            else
            {
                classValue = "hood-attach";
            }
            output.Attributes.SetAttribute("class", classValue);
            output.Attributes.SetAttribute("type", "button");
            output.Attributes.SetAttribute("data-id", Id);
            output.Attributes.SetAttribute("data-entity", Entity);
            output.Attributes.SetAttribute("data-field", Field);
            output.Attributes.SetAttribute("data-refresh", Refresh);
            output.Attributes.SetAttribute("data-tag", Tag);
        }
    }
}
