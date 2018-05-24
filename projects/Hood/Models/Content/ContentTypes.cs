using Hood.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class ContentType
    {
        public string BaseName { get; set; }
        public bool Enabled { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string Description { get; set; }
        public string Search { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string TypeNamePlural { get; set; }
        public string Icon { get; set; }
        public bool HasPage { get; set; }
        public bool IsPublic { get; set; }
        public string TitleName { get; set; }
        public string ExcerptName { get; set; }
        public bool ShowPreview { get; set; }
        public bool ShowEditor { get; set; }
        public bool ShowDesigner { get; set; }
        public bool MultiLineExcerpt { get; set; }
        public bool RichTextExcerpt { get; set; }
        public bool ShowCategories { get; set; }
        public bool ShowImage { get; set; }
        public bool ShowBanner { get; set; }
        public bool ShowMeta { get; set; }
        public bool Templates { get; set; }
        public bool Gallery { get; set; }
        public string CustomFieldsJson { get; set; }
        public string TemplateFolder { get; set; }
        public string UrlFormatting { get; set; }

        public List<CustomField> CustomFields
        {
            get
            {
                if (!CustomFieldsJson.IsSet())
                    return new List<CustomField>();
                return JsonConvert.DeserializeObject<List<CustomField>>(CustomFieldsJson);
            }
            set
            {
                CustomFieldsJson = JsonConvert.SerializeObject(value);
            }
        }

        public bool CachedByType { get; internal set; }

        public CustomField GetMetaDetails(string name)
        {
            var field = CustomFields.SingleOrDefault(c => c.Name == name);
            if (field != null)
                return field;
            var baseType = ContentTypes.All.SingleOrDefault(t => t.TypeName == TypeName);
            if (baseType != null)
            {
                field = baseType.CustomFields.SingleOrDefault(c => c.Name == name);
                if (field != null)
                    return field;
            }
            return new CustomField()
            {
                Default = "",
                Name = name,
                System = false,
                Type = "System.String"
            };
        }
    }

    public class CustomField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Default { get; set; }
        public bool System { get; set; }
    }

    // Static Functionality
    public static class ContentTypes
    {
        public static List<ContentType> All
        {
            get
            {
                return new List<ContentType>() {

                    new ContentType() {
                        BaseName = "Slider",
                        Slug = "slider",
                        Type = "slider",
                        Search = "sliders",
                        Title = "Sliders",
                        Icon = "fa-object-group",
                        TypeName = "Slider",
                        Enabled = true,
                        IsPublic = false,
                        HasPage = false,
                        TypeNamePlural = "Sliders",
                        TitleName = "Title",
                        ExcerptName = "Content",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = true,
                        ShowCategories = true,
                        ShowBanner = true,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "SliderTemplates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.Slider.CallToAction.Url", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Slider.CallToAction.Style", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Slider.CallToAction.Text", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Slider.DisplayOrder", Default = "0", System = true, Type="System.Int32" }
                        })
                    },
                    new ContentType() {
                        BaseName = "Event",
                        Slug = "event",
                        Type = "event",
                        Search = "events",
                        Title = "Events",
                        Icon = "fa-calendar",
                        TypeName = "Event",
                        Enabled = false,
                        IsPublic = true,
                        HasPage = false,
                        TypeNamePlural = "Events",
                        TitleName = "Title",
                        ExcerptName = "Description",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = true,
                        ShowCategories = true,
                        ShowBanner = true,
                        ShowImage = true,
                        UrlFormatting = "news-title",
                        ShowMeta = true,
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.Event.Start", Default = "", System = true, Type="System.DateTime" },
                            new CustomField() { Name = "Content.Event.End", Default = "", System = true, Type="System.DateTime" }
                        })
                    },
                    new ContentType() {
                        BaseName = "News",
                        Slug = "news",
                        Type = "news",
                        Search = "news",
                        Title = "News Stories",
                        Icon = "fa-newspaper-o",
                        TypeName = "Story",
                        Enabled = true,
                        IsPublic = true,
                        HasPage = true,
                        TypeNamePlural = "News",
                        TitleName = "Title",
                        ExcerptName = "Excerpt",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = true,
                        ShowCategories = true,
                        ShowBanner = true,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = true,
                        UrlFormatting = "news-title",
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.News.Video", Default = "", System = true, Type="Hood.MultiLineString" },
                            new CustomField() { Name = "Content.News.Headline", Default = "", System = true, Type="System.String" }
                        })
                    },
                    new ContentType() {
                        BaseName = "Client",
                        Slug = "client",
                        Type = "client",
                        Search = "client",
                        Title = "Clients",
                        Icon = "fa-users",
                        TypeName = "Client",
                        Enabled = false,
                        IsPublic = false,
                        HasPage = false,
                        TypeNamePlural = "Clients",
                        TitleName = "Client Name",
                        ExcerptName = "Website URL",
                        MultiLineExcerpt = false,
                        ShowDesigner = false,
                        ShowEditor = false,
                        ShowCategories = true,
                        ShowBanner = false,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = new List<CustomField>()
                    },
                    new ContentType() {
                        BaseName = "FAQ",
                        Slug = "faq",
                        Type = "faq",
                        Search = "faq",
                        Title = "FAQs",
                        Icon = "fa-question-circle",
                        TypeName = "FAQ",
                        Enabled = false,
                        IsPublic = true,
                        HasPage = false,
                        TypeNamePlural = "FAQs",
                        TitleName = "Question",
                        ExcerptName = "Answer",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = false,
                        ShowCategories = true,
                        ShowBanner = false,
                        ShowImage = false,
                        ShowMeta = true,
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.FAQ.Department", Default = "", System = true, Type="System.String" }
                        }
                    },
                    new ContentType() {
                        BaseName = "Page",
                        Slug = "pages",
                        Type = "page",
                        Search = "search",
                        Title = "Pages",
                        Icon = "fa-file",
                        TypeName = "Page",
                        Enabled = true,
                        IsPublic = false,
                        HasPage = true,
                        TypeNamePlural = "Pages",
                        TitleName = "Page Title",
                        ExcerptName = "Page Description",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = true,
                        ShowCategories = true,
                        ShowBanner = true,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = false,
                        Templates = true,
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Settings.Security.Subscription", Default = "", System = true, Type="System.String" }
                        })
                    },
                    new ContentType() {
                        BaseName = "Team",
                        Slug = "team",
                        Type = "team",
                        Search = "team",
                        Title = "Team Members",
                        Icon = "fa-user-secret",
                        TypeName = "Team Member",
                        Enabled = false,
                        IsPublic = true,
                        HasPage = false,
                        TypeNamePlural = "Team Members",
                        TitleName = "Name",
                        ExcerptName = "Bio",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = false,
                        ShowCategories = true,
                        ShowBanner = true,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.Team.Email", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.JobTitle", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.Phone", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.Mobile", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.Twitter", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.LinkedIn", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.Facebook", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.GooglePlus", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Team.DisplayOrder", Default = "", System = true, Type="System.String" }
                        })
                    },
                    new ContentType() {
                        BaseName = "Testimonial",
                        Slug = "testimonials",
                        Type = "testimonial",
                        Search = "testimonial",
                        Title = "Testimonials",
                        Icon = "fa-comments",
                        Enabled = false,
                        IsPublic = false,
                        HasPage = false,
                        TypeName = "Testimonial",
                        TypeNamePlural = "Testimonials",
                        TitleName = "Client Name",
                        ExcerptName = "Testimonial",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = false,
                        ShowCategories = true,
                        ShowBanner = false,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.Testimonial.CompanyName", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Testimonial.JobTitle", Default = "", System = true, Type="System.String" }
                        })
                    },
                    new ContentType() {
                        BaseName = "Portfolio",
                        Slug = "portfolio",
                        Type = "portfolio",
                        Search = "portfolio",
                        Title = "Portfolio",
                        Icon = "fa-folder-open",
                        TypeName = "Portfolio",
                        Enabled = false,
                        IsPublic = true,
                        HasPage = true,
                        TypeNamePlural = "Portfolio",
                        TitleName = "Title",
                        ExcerptName = "Short Description",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = true,
                        ShowCategories = true,
                        ShowBanner = true,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = true,
                        Templates = true,
                        UrlFormatting = "news-title",
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.Portfolio.Client", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Portfolio.ServiceType", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Portfolio.CaseStudy", Default = "false", System = true, Type="System.Boolean" },
                            new CustomField() { Name = "Content.Portfolio.ExternalUrl", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Portfolio.DeliveryDate", Default = "", System = true, Type="System.DateTime" },
                            new CustomField() { Name = "Content.Portfolio.Headline", Default = "", System = true, Type="System.String" }
                        })
                    },
                    new ContentType() {
                        BaseName = "Product",
                        Slug = "products",
                        Type = "product",
                        Search = "products",
                        Title = "Products",
                        Icon = "fa-shopping-bag",
                        TypeName = "Product",
                        Enabled = false,
                        IsPublic = true,
                        HasPage = true,
                        TypeNamePlural = "Products",
                        TitleName = "Title",
                        ExcerptName = "Short Description",
                        MultiLineExcerpt = true,
                        ShowDesigner = false,
                        ShowEditor = true,
                        ShowCategories = true,
                        ShowBanner = true,
                        ShowImage = true,
                        ShowMeta = true,
                        Gallery = true,
                        Templates = false,
                        UrlFormatting = "news-title",
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.Product.Supplier", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Product.SKU", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Product.Price", Default = "0", System = true, Type="System.Decimal" },
                            new CustomField() { Name = "Content.Product.Tax", Default = "20", System = true, Type="System.Decimal" },
                            new CustomField() { Name = "Content.Product.ListPrice", Default = "0", System = true, Type="System.Decimal" },
                            new CustomField() { Name = "Content.Product.Discount", Default = "0", System = true, Type="System.Decimal" },
                            new CustomField() { Name = "Content.Product.RecommendedPrice", Default = "0", System = true, Type="System.Decimal" },
                            new CustomField() { Name = "Content.Product.MinCart", Default = "0", System = true, Type="System.Int32" },
                            new CustomField() { Name = "Content.Product.MaxCart", Default = "0", System = true, Type="System.Int32" },
                            new CustomField() { Name = "Content.Product.Width", Default = "0", System = true, Type="System.Int32" },
                            new CustomField() { Name = "Content.Product.Height", Default = "0", System = true, Type="System.Int32" },
                            new CustomField() { Name = "Content.Product.Depth", Default = "0", System = true, Type="System.Int32" },
                            new CustomField() { Name = "Content.Product.Weight", Default = "0", System = true, Type="System.Int32" },
                            new CustomField() { Name = "Content.Product.SellByDate", Default = "false", System = true, Type="System.Boolean" },
                            new CustomField() { Name = "Content.Product.SellStartDate", Default = "", System = true, Type="System.DateTime" },
                            new CustomField() { Name = "Content.Product.SellEndDate", Default = "", System = true, Type="System.DateTime" },
                            new CustomField() { Name = "Content.Product.Available", Default = "true", System = true, Type="System.Boolean" },
                            new CustomField() { Name = "Content.Product.StockLevel", Default = "0", System = true, Type="System.Int32" },
                            new CustomField() { Name = "Content.Product.TrackStock", Default = "false", System = true, Type="System.Boolean" }
                        })
                    }
                };
            }
        }

        private static List<CustomField> BaseFields(List<CustomField> fields)
        {
            fields.AddRange(new List<CustomField>()
            {
                new CustomField() {Name="Settings.Template", Default = "_Blank", System = true, Type = "System.String" },

                new CustomField() {Name="Settings.SubType", Default = "", System = true, Type = "System.String" },

                new CustomField() {Name="SEO.Meta.Title", Default = "", System = true, Type = "System.String" },
                new CustomField() {Name="SEO.Meta.Description", Default = "", System = true, Type = "System.String" },

                new CustomField() {Name="SEO.Facebook.Title", Default = "", System = true, Type = "System.String" },
                new CustomField() {Name="SEO.Facebook.Description", Default = "", System = true, Type = "System.String" },
                new CustomField() {Name="SEO.Facebook.ImageUrl", Default = "", System = true, Type = "Hood.Image" },

                new CustomField() {Name="SEO.Twitter.Title", Default = "", System = true, Type = "System.String" },
                new CustomField() {Name="SEO.Twitter.Description", Default = "", System = true, Type = "System.String" },
                new CustomField() {Name="SEO.Twitter.ImageUrl", Default = "", System = true, Type = "Hood.Image" },
                new CustomField() {Name="SEO.Twitter.Creator", Default = "", System = true, Type = "System.String" },

                new CustomField() {Name="Settings.Security.AdminOnly", Default = "false", System = true, Type = "System.Boolean" },
                new CustomField() {Name="Settings.Security.Public", Default = "true", System = true, Type = "System.Boolean" },

                new CustomField() {Name="Settings.Image.Featured.Align", Default = "center", System = true, Type = "System.String" },
                new CustomField() {Name="Settings.Image.Featured.Fit", Default = "cover", System = true, Type = "System.String" },
                new CustomField() {Name="Settings.Image.Featured.Background", Default = "transparent", System = true, Type = "System.String" }
            });
            return fields;
        }
    }
}

