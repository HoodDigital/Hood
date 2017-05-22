using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Hood.TagHelpers
{
    [HtmlTargetElement("showField", Attributes = FieldAttr)]
    public class ShowFieldTagHelper : TagHelper
    {
        // the name of attribute has been moved to a variable to keep things DRY
        private const string FieldAttr = "db-field";
        private const string TypeAttr = "field-type";
        private const string PlaceholderAttr = "placeholder";
        private const string FieldNameAttr = "field-name";
        private const string ValueAttr = "db-value";
        private const string InputClassAttr = "input-class";
        private const string FormLayoutName = "form-layout";

        [HtmlAttributeName(FieldAttr)]
        public string Field { get; set; }

        [HtmlAttributeName(TypeAttr)]
        public string Type { get; set; }

        [HtmlAttributeName(PlaceholderAttr)]
        public string Placeholder { get; set; }

        [HtmlAttributeName(FieldNameAttr)]
        public string FieldName { get; set; }

        [HtmlAttributeName(ValueAttr)]
        public string Value { get; set; }

        [HtmlAttributeName(InputClassAttr)]
        public string InputClass { get; set; }

        [HtmlAttributeName(FormLayoutName)]
        public string FormLayout { get; set; }

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
            string content = "";
            switch (Type)
            {
                case "text":
                case "email":
                case "number":
                case "datetime":
                case "month":
                case "search":
                case "tel":
                case "url":
                case "week":
                case "password":
                    content = string.Format(@"
<dd class='{0} margin-bottom-5'><strong>{1}</strong>&nbsp;<small>Click to select</small></label>
<div class='{2}'>
    <input id='{3}' name='{3}' type='{4}' class='select-text {5}' value='{6}' placeholder='{1}' readonly='readonly'  />
</div>", LabelClass, FieldName, FieldDivClass, Field, Type, InputClass, Value);
                    break;

                case "textarea":
                    content = string.Format(@"
<dd class='{0} margin-bottom-5'><strong>{1}</strong>&nbsp;<small>Click to select</small></label>
<div class='{2}'>
    <textarea id='{3}' name='{3}' rows='3' class='select-text {4}' placeholder='{1}' readonly='readonly'>{5}</textarea>
</div>", LabelClass, FieldName, FieldDivClass, Field, InputClass, Value);
                    break;

                case "switch":
                    content = string.Format(@"
<span>
    {0}
</span>
<div class='switch'>
    <div class='onoffswitch'>
        <input type='checkbox' class='onoffswitch-checkbox' name='{1}' id='{1}'>
        <label class='onoffswitch-label' for='{1}'>
            <span class='onoffswitch-inner'></span>
            <span class='onoffswitch-switch'></span>
        </label>
    </div>
</div>", FieldName, Field, InputClass, Value);
                    break;


            }
            output.Content.AppendHtml(content);

        }
    }
}
