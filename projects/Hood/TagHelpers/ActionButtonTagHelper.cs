using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("action", Attributes = HoodIdAttribute)]
    public class ActionButtonTagHelper : TagHelper
    {
        // the name of attribute has been moved to a variable to keep things DRY
        private const string HoodIdAttribute = "hood-id";
        private const string HoodTypeAttribute = "hood-type";
        private const string HoodActionAttribute = "hood-action";

        [HtmlAttributeName(HoodIdAttribute)]
        public string ItemId { get; set; }

        [HtmlAttributeName(HoodTypeAttribute)]
        public string ItemType { get; set; }

        [HtmlAttributeName(HoodActionAttribute)]
        public string ItemAction { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string classValue;
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"].Value, "action-button");
            }
            else
            {
                classValue = "btn btn-sm action-button";
            }
            output.Attributes.SetAttribute("class", classValue);
            switch (ItemAction)
            {
                case "design":
                    output.Content.AppendHtml("<i class=\"fa fa-pencil\"></i><span>&nbsp;Design</span>");
                    classValue = string.Format("btn-info {0}", output.Attributes["class"].Value);
                    output.Attributes.SetAttribute("href", "/content/show/" + ItemId + "?editMode=true");
                    output.Attributes.SetAttribute("class", classValue);
                    break;
                case "edit":
                    output.Content.AppendHtml("<i class=\"fa fa-edit\"></i><span>&nbsp;Edit</span>");
                    classValue = string.Format("btn-primary {0}", output.Attributes["class"].Value);
                    output.Attributes.SetAttribute("href", "/admin/" + ItemType.ToLower() + "/edit/" + ItemId);
                    output.Attributes.SetAttribute("class", classValue);
                    break;
                case "delete":
                    output.Content.AppendHtml("<i class=\"fa fa-trash-o\"></i><span>&nbsp;Delete</span>");
                    output.Attributes.SetAttribute("data-id", ItemId);
                    classValue = string.Format("btn-danger {0} {1}", output.Attributes["class"].Value, "delete-" + ItemType.ToLower());
                    output.Attributes.SetAttribute("class", classValue);
                    break;
            }
            output.TagName = "a";
        }
    }
}
