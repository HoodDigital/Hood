using System.Collections.Generic;

namespace Hood.Models
{
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
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "SliderTemplates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.Slider.CallToAction.Url", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Slider.CallToAction.Style", Default = "", System = true, Type="System.String" },
                            new CustomField() { Name = "Content.Slider.CallToAction.Text", Default = "", System = true, Type="System.String" }
                        })
                    },
                    new ContentType() {
                        BaseName = "News",
                        Slug = "news",
                        Type = "news",
                        Search = "news",
                        Title = "News Stories",
                        Icon = "fa-newspaper",
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
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(new List<CustomField>())
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
                        Gallery = false,
                        Templates = false,
                        TemplateFolder = "Templates",
                        CustomFields = BaseFields(
                        new List<CustomField>()
                        {
                            new CustomField() { Name = "Content.FAQ.Department", Default = "", System = true, Type="System.String" }
                        })
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
                            new CustomField() { Name = "Content.Team.GooglePlus", Default = "", System = true, Type="System.String" }
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
                    }
                };
            }
        }
        public static List<CustomField> BaseFields(List<CustomField> fields)
        {
            fields.AddRange(new List<CustomField>()
            {
                new CustomField() {Name="DisplayOrder", Default="0", System=true, Type="System.Int32" },

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

