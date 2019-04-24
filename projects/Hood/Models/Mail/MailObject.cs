using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.IO;

namespace Hood.Services
{
    public class MailObject
    {
        public EmailAddress To { get; set; }
        public string Subject { get; set; }
        public string PreHeader { get; set; }
        public string ToName { get; set; }
        public string Template { get; set; } = Models.MailSettings.PlainTemplate;

        public bool ShowSiteSocials { get; set; } = true;
        public string EmailSmallPrint { get; set; } = "<a href='http://hooddigital.com/' style='color: #999999;text-decoration: none;'>Powered by HoodCMS by Hood Digital</a>.";
        public string Logo { get; set; } = "";
        public string Title { get; set; } = "";

        public string HeaderContent
        {
            get
            {
                BasicSettings _basicSettings = Engine.Current.Resolve<ISettingsRepository>().GetBasicSettings();
                Models.MailSettings _mailSettings = Engine.Current.Resolve<ISettingsRepository>().GetMailSettings();

                if (!Title.IsSet())
                    Title = _basicSettings.SiteTitle;

                if (!Logo.IsSet())
                    if (_mailSettings.Logo.IsSet())
                        Logo = _mailSettings.Logo;
                    else
                        if (_basicSettings.SiteLogo.IsSet())
                        Logo = _basicSettings.SiteLogo;

                if (Logo.IsSet())
                    return $"<h1 class='align-center' style='color:#222222;font-family:sans-serif;font-weight:300;line-height:1.4;margin:0;margin-bottom:30px;font-size:25px;text-transform:capitalize;text-align:center;text-decoration:none;'>" +
                       $"    <img src='{Logo}' alt='{Title}' align='center' style='border:none;-ms-interpolation-mode:bicubic;max-width:75%;max-height:100px;'>" +
                       $"</h1>";
                else
                    return $"<h1 class='align-center' style='color:#222222;font-family:sans-serif;font-weight:300;line-height:1.4;margin:0;margin-bottom:30px;font-size:25px;text-transform:capitalize;text-align:center;text-decoration:none;'>" +
                       $"    {Title}" +
                       $"</h1>";
            }
        }

        public MailObject()
        {
            _textBody = new StringWriter();
            _body = new StringWriter();
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
            _body.WriteLine(string.Format(@"<h1 class='align-{2}' style='color: {1}; font-family: sans-serif; font-weight: 300; line-height: 1.4em; margin-top: 2em; margin-bottom: 1em;  font-size: 35px; text-transform: capitalize; text-align: {2};'>{0}</h1>", content, colour, align));
        }
        public void AddH2(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine();
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine(string.Format(@"<h2 class='align-{2}' style='color: {1}; font-family: sans-serif; font-weight: 300; line-height: 1.4em; margin-top: 2em; margin-bottom: 1em;  font-size: 30px; text-transform: capitalize; text-align: {2};'>{0}</h2>", content, colour, align));
        }
        public void AddH3(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine();
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine(string.Format(@"<h3 class='align-{2}' style='color: {1}; font-family: sans-serif; font-weight: 300; line-height: 1.4em; margin-top: 2em; margin-bottom: 1em; font-size: 25px; text-transform: capitalize; text-align: {2};'>{0}</h3>", content, colour, align));
        }
        public void AddH4(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine();
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _textBody.WriteLine();
            _body.WriteLine(string.Format(@"<h4 class='align-{2}' style='color: {1}; font-family: sans-serif; font-weight: 300; line-height: 1.4em; margin-top: 2em; margin-bottom: 1em; font-size: 20px; text-transform: capitalize; text-align: {2};'>{0}</h4>", content, colour, align));
        }
        public void AddParagraph(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _body.WriteLine(string.Format(@"<p class='align-{2}' style='color: {1}; font-family: sans-serif; font-size: 14px; font-weight: normal; margin-top: 0; margin-bottom: 1em;  text-align: {2};'>{0}</p>", content, colour, align));
        }
        public void AddDiv(string content, string colour = "#222222", string align = "left")
        {
            _textBody.WriteLine(content);
            _textBody.WriteLine();
            _body.WriteLine(string.Format(@"<div class='align-{2}' style='color: {1}; font-family: sans-serif; font-size: 14px; font-weight: normal; margin-top: 2em; margin-bottom: 1em;  text-align: {2};'>{0}</p>", content, colour, align));
        }
        public void AddUnorderedList(List<string> items, string colour = "#222222")
        {
            foreach (string item in items)
            {
                _textBody.WriteLine($"- {item}");
            }
            _textBody.WriteLine();

            _body.WriteLine(string.Format(@"<ul style='color: {0}; font-family: sans-serif; font-size: 14px; font-weight: normal; margin-top: 2em; margin-bottom: 1em;'>", colour));
            foreach (string item in items)
            {
                _body.WriteLine(string.Format(@"<li>{0}</li>", item, colour));
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

            _body.WriteLine(string.Format(@"<ol style='color: {0}; font-family: sans-serif; font-size: 14px; font-weight: normal; margin-top: 2em; margin-bottom: 1em;'>", colour));
            foreach (string item in items)
            {
                _body.WriteLine(string.Format(@"<li>{0}</li>", item, colour));
            }
            _body.WriteLine("</ol>");
        }
        public void AddImage(string url, string altText)
        {
            _textBody.WriteLine("Image: " + url + "(" + altText + ")");
            _body.WriteLine(string.Format(@"<p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; margin-bottom: 15px;'><img src='{0}' alt='{1}' width='520' class='img-responsive img-block' style='border: none; -ms-interpolation-mode: bicubic; max-width: 100%; display: block;'></p>", url, altText));
        }
        public void AddCallToAction(string content, string url, string colour = "#3498db", string align = "left")
        {
            _textBody.WriteLine(content + ": " + url);
            _body.WriteLine(string.Format(@"<table border='0' cellpadding='0' cellspacing='0' class='btn btn-primary' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; box-sizing: border-box; min-width: 100% !important;' width='100%'>
    <tbody>
        <tr>
            <td align='{3}' style='font-family: sans-serif; font-size: 14px; vertical-align: top; padding-bottom: 15px;' valign='top'>
                <table border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;'>
                    <tbody>
                        <tr>
                            <td style='font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: {2}; border-radius: 5px; text-align: {3};' valign='top' bgcolor='{2}' align='{3}'>
                                <a href='{1}' target='_blank' style='display: inline-block; color: #ffffff; background-color: {2}; border: solid 1px {2}; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: {2};'>
                                    {0}
                                </a>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </td>
        </tr>
    </tbody>
</table>
", content, url, colour, align));
        }
    }
}
