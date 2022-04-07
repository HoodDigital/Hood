using Hood.Core;
using Hood.Extensions;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hood.Models
{
    public class MailObject
    {
        public EmailAddress To { get; set; }
        public string Subject { get; set; }
        public string PreHeader { get; set; }
        public string ToName { get; set; }
        public string Template { get; set; } = Models.MailSettings.PlainTemplate;

        public bool ShowSiteSocials { get; set; } = true;
        public string EmailSmallPrint { get; set; } = "";
        public string Logo { get; set; } = "";
        public string Title { get; set; } = "";
        public string HeroImage { get; set; } = "";
        public string FontFamily { get; set; } = "Poppins,sans-serif";
        public string LogoAlign { get; set; } = "left";
        public string BackgroundColour { get; set; }
        public int BaseFontSize { get; set; } = 18;
        public string MailPadding { get; set; } = "30px";

        public string GetBackgroundColor()
        {
            Models.MailSettings mailSettings = Engine.Settings.Mail;

            if (!BackgroundColour.IsSet())
                if (mailSettings.BackgroundColour.IsSet())
                    Logo = mailSettings.BackgroundColour;

            return BackgroundColour;
        }

        public string GetHeader()
        {
            BasicSettings basicSettings = Engine.Settings.Basic;
            Models.MailSettings mailSettings = Engine.Settings.Mail;

            if (!Title.IsSet())
                Title = basicSettings.FullTitle;

            if (!Logo.IsSet())
                if (mailSettings.Logo.IsSet())
                    Logo = mailSettings.Logo;
                else
                    if (basicSettings.Logo.IsSet())
                    Logo = basicSettings.Logo;

            if (Logo.IsSet())
                return
                @$"<tr>
                    <td style='padding:{MailPadding};text-align:{LogoAlign}'>
                        <h1 class='align-{LogoAlign}' style='color:#222222;margin:0;margin-bottom:0px;text-align:{LogoAlign};text-decoration:none;'>
                            <img src='{Logo}' alt='{Title}' align='center' style='border:none;-ms-interpolation-mode:bicubic;width:250px;height:auto;' width='300'>
                        </h1>
                    </td>
                </tr>";
            else
                return
                @$"<tr>
                    <td style='padding:{MailPadding};text-align:{LogoAlign}'>
                        <h1 class='align-{LogoAlign}' style='color:#222222;{GetFontStyles(2.5)}margin-bottom:0px;text-align:{LogoAlign};text-decoration:none;'>
                            {Title}
                        </h1>
                    </td>
                </tr>";
        }

        public string GetHero()
        {
            Models.MailSettings mailSettings = Engine.Settings.Mail;

            if (!HeroImage.IsSet())
                if (mailSettings.HeroImage.IsSet())
                    HeroImage = mailSettings.HeroImage;

            if (HeroImage.IsSet())
                return
                @$"<tr>
                    <td style='background-color: #ffffff;'>
                        <img src='{HeroImage}' width='680' height='' alt='alt_text' border='0'
                            style='width:100%;max-width:680px;height:auto;background:#dddddd;margin:auto;display:block;' class='g-img'>
                    </td>
                </tr>";
            else
                return $"";
        }
        public string GetFontStyles(double fontSizeMultiplier = 1, double lineHeightMultiplier = 1.25)
        {
            double size = BaseFontSize * fontSizeMultiplier;
            double lineHeight = BaseFontSize * fontSizeMultiplier * lineHeightMultiplier;
            return $"font-family:{FontFamily};font-size:{Math.Round(size)}px;line-height:{Math.Round(lineHeight)}px;margin-top:0;margin-bottom:{BaseFontSize}px;";
        }

        public MailObject()
        {
            BasicSettings basicSettings = Engine.Settings.Basic;
            Models.MailSettings mailSettings = Engine.Settings.Mail;

            _textBody = new StringWriter();
            _body = new StringWriter();

            BackgroundColour = mailSettings.BackgroundColour;
        }

        private StringWriter _body;
        public string Html
        {
            get
            {
                return _body.ToString();
            }
            set
            {
                _body.Close();
                _body = new StringWriter();
                _body.Write(value);
            }
        }

        private StringWriter _textBody;
        public string Text
        {
            get
            {
                return _textBody.ToString();
            }
            set
            {
                _textBody.Close();
                _textBody = new StringWriter();
                _textBody.Write(value);
            }
        }
        public void AddCustomHtml(string htmlContent, string textContent)
        {
            if (textContent.IsSet())
                _textBody.WriteLine(textContent);
            _body.WriteLine(htmlContent);
        }
        public void AddHorizontalRule()
        {
            _textBody.WriteLine();
            _textBody.WriteLine("--------------------");
            _textBody.WriteLine();
            _body.WriteLine(string.Format(@"<hr />"));
        }
        public void AddLineBreak()
        {
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine(string.Format(@"<br />"));
        }
        public void AddH1(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine();
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine($"<h1 class='align-{align}' style='color:{colour};{GetFontStyles(2.5, 1)}text-align:{align};'>{content}</h1>");
        }
        public void AddH2(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine();
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine($"<h2 class='align-{align}' style='color:{colour};{GetFontStyles(2, 1.1)}text-align:{align};'>{content}</h2>");
        }
        public void AddH3(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine();
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine($"<h3 class='align-{align}' style='color:{colour};{GetFontStyles(1.75)}text-align:{align};'>{content}</h3>");
        }

        public void AddH4(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine();
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine($"<h4 class='align-{align}' style='color:{colour};{GetFontStyles(1.3)}text-align:{align};'>{content}</h4>");
        }
        public void AddParagraph(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _body.WriteLine($"<p class='align-{align}' style='color:{colour};{GetFontStyles(1, 1.5)}text-align:{align};'>{content}</p>");
        }
        public void AddDiv(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _body.WriteLine($"<div class='align-{align}' style='color:{colour};{GetFontStyles(1)}text-align:{align};'>{content}</div>");
        }
        public void AddUnorderedList(List<string> items, string colour = "#222222")
        {
            foreach (string item in items)
            {
                _textBody.WriteLine($"- {item}");
            }
            _textBody.WriteLine();

            _body.WriteLine($"<ul style='color:{colour};{GetFontStyles(1)}margin-top:0;margin-bottom:1em;'>");
            foreach (string item in items)
            {
                _body.WriteLine($"<li style='color:{colour};{GetFontStyles(1)}margin-top:0;margin-bottom:1em;'>{item}</li>");
            }
            _body.WriteLine("</ul>");
        }
        public void AddOrderedList(List<string> items, string colour = "#222222")
        {
            foreach (string item in items)
            {
                _textBody.WriteLine($"- {item}");
            }
            _textBody.WriteLine();

            _body.WriteLine($"<ol style='color:{colour};{GetFontStyles(1)}margin-top:0;margin-bottom:1em;'>");
            foreach (string item in items)
            {
                _body.WriteLine($"<li style='color:{colour};{GetFontStyles(1)}margin-top:0;margin-bottom:1em;'>{item}</li>");
            }
            _body.WriteLine("</ol>");
        }
        public void AddImage(string url, string altText)
        {
            _textBody.WriteLine("Image: " + url + "(" + altText + ")");
            _body.WriteLine($"<p style='{GetFontStyles(1)}margin: 0;margin-bottom: 1em;'><img src='{url}' alt='{altText}' width='520' class='img-responsive img-block' style='border:none;-ms-interpolation-mode:bicubic;max-width:100%;display:block;'></p>");
        }
        public void AddCallToAction(string content, string url, string colour = "#3498db", string align = "left")
        {
            _textBody.WriteLine(content + ": " + url);
            _body.WriteLine($@"<table border='0' cellpadding='0' cellspacing='0' class='btn btn-primary' style='margin-bottom:{BaseFontSize}px;border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width;100%;box-sizing:border-box;min-width:100%!important;' width='100%'>
    <tbody>
        <tr>
            <td align='{align}' style='{GetFontStyles(1)}margin-bottom:0px;vertical-align:top;text-align:{align}' valign='top'>
                <table border='0' cellpadding='0' cellspacing='0' style='border-collapse:separate;mso-table-lspace: 0pt;mso-table-rspace:0pt;width:auto;'>
                    <tbody>
                        <tr>
                            <td style='vetical-align:top;background-color:{colour};border-radius:5px;text-align:{align};' valign='top' bgcolor='{colour}' align='{align}'>
                                <a href='{url}' target='_blank' style='display:inline-block;color:#ffffff;background-color:{colour};border:solid 1px {colour};border-radius:5px;box-sizing:border-box;cursor:pointer;text-decoration:none;{GetFontStyles(1)}margin-bottom:0px;font-weight:bold;margin:0;padding:12px 25px;border-color:{colour};'>
                                    {content}
                                </a>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </td>
        </tr>
    </tbody>
</table>
");
        }
    }
}
