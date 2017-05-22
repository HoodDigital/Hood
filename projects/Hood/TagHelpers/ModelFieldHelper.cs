using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("modelField")]
    public class ModelFieldTagHelper : TagHelper
    {
        // the name of attribute has been moved to a variable to keep things DRY
        private const string TypeAttr = "field-type";
        private const string PlaceholderAttr = "placeholder";
        private const string FieldAttr = "db-field";
        private const string FieldNameAttr = "field-name";
        private const string ValueAttr = "db-value";
        private const string FormLayoutName = "form-layout";
        private const string InputClassAttr = "input-class";
        private const string FormatAttr = "format";

        [HtmlAttributeName(TypeAttr)]
        public string Type { get; set; }

        [HtmlAttributeName(PlaceholderAttr)]
        public string Placeholder { get; set; }

        [HtmlAttributeName(FieldAttr)]
        public string Field { get; set; }

        [HtmlAttributeName(FormLayoutName)]
        public string FormLayout { get; set; }

        [HtmlAttributeName(FieldNameAttr)]
        public string FieldName { get; set; }

        [HtmlAttributeName(ValueAttr)]
        public string Value { get; set; }

        [HtmlAttributeName(InputClassAttr)]
        public string InputClass { get; set; }

        [HtmlAttributeName(FormatAttr)]
        public string Format { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            string classValue;
            if (output.Attributes.ContainsName("class"))
            {
                classValue = string.Format("{0} {1}", output.Attributes["class"], "form-group");
            }
            else
            {
                classValue = "form-group";
            }
            output.Attributes.SetAttribute("class", classValue);

            string LabelClass = "";
            string FieldDivClass = "";
            if (FormLayout == "horizontal")
            {
                LabelClass = "col-sm-4 control-label";
                FieldDivClass = "col-sm-8";
            }
            else
            {
                LabelClass = "control-label";
                FieldDivClass = "";
            }
            if (string.IsNullOrEmpty(Placeholder))
            {
                Placeholder = FieldName;
            }

            if (string.IsNullOrEmpty(InputClass))
                InputClass = "form-control";
            else
                InputClass = string.Format("{0} {1}", InputClass, "form-control");

            if (!string.IsNullOrEmpty(Format))
                Format = string.Format("data-format='{0}'", Format);

            if (Type != "textarea")
            {
                string content = string.Format(@"
<label class='{0}' for='{3}'>{1}</label>
<div class='{2}'>
    <input id='{3}' name='{3}' type='{4}' class='{5}' value='{6}' {7} />
</div>", LabelClass, FieldName, FieldDivClass, Field, Type, InputClass, Value, Format);
                output.Content.AppendHtml(content);
            }
            else
            {
                string content = string.Format(@"
<label class='{0}' for='{3}'>{1}</label>
<div class='{2}'>
    <textarea id='{3}' name='{3}' type='{4}' class='{5}' value='{6}' {7}></textarea>
</div>", LabelClass, FieldName, FieldDivClass, Field, Type, InputClass, Value, Format);
                output.Content.AppendHtml(content);
            }
        }
    }
}
