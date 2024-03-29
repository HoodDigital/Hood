﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Hood.Core;

namespace Hood.Extensions
{
    public static class StringExtensions
    {
        #region "Urls"

        public static bool IsAbsoluteUrl(this string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    IdnMapping idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string ToUrl(this string url)
        {
            Uri formattedUrl = null;
            try
            {
                formattedUrl = new UriBuilder(url).Uri;
                return formattedUrl.ToString();
            }
            catch (ArgumentNullException)
            {
                return url;
            }
            catch (UriFormatException)
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || Uri.TryCreate("http://" + url, UriKind.Absolute, out uri))
                {
                    return uri.ToString();
                }
            }
            return url;
        }
        public static string ToTwitterUrl(this string url)
        {
            if (url.ToLower().Contains("twitter.com"))
            {
                return url.ToUrl();
            }

            if (url.StartsWith("@"))
            {
                return ("https://twitter.com/" + url.Replace("@", "")).ToUrl();
            }

            return ("https://twitter.com/" + url).ToUrl();
        }
        public static string ToInstagramUrl(this string url)
        {
            if (url.ToLower().Contains("instagram.com"))
            {
                return url.ToUrl();
            }

            if (url.StartsWith("@"))
            {
                return ("https://www.instagram.com/" + url.Replace("@", "")).ToUrl();
            }

            return ("https://www.instagram.com/" + url).ToUrl();
        }
        public static string ToFacebookUrl(this string url)
        {
            if (url.ToLower().Contains("facebook.com"))
            {
                return url.ToUrl();
            }

            return ("https://www.facebook.com/" + url).ToUrl();
        }
        public static string ToGooglePlusUrl(this string url)
        {
            if (url.ToLower().Contains("plus.google.com"))
            {
                return url.ToUrl();
            }

            if (url.StartsWith("+"))
            {
                return ("https://plus.google.com/" + url).ToUrl();
            }

            return ("https://plus.google.com/+" + url).ToUrl();
        }


        #endregion
        /// <summary>
        /// Adds HTML style line breaks. <br /> in place of newlines.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddHtmlLineBreaks(this string str)
        {
            if (!str.IsSet())
            {
                return str;
            }
            return str.Replace(Environment.NewLine, "<br />").Replace("\r\n", "<br />").Replace("\r", "<br />").Replace("\n", "<br />");
        }
        public static string EncodeHtml(this string content)
        {
            string encoded = HtmlEncoder.Default.Encode(content);
            return encoded;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsSet(this string str)
        {
            return !string.IsNullOrEmpty(str) && !string.Equals("null", str) && !string.Equals("\"null\"", str);
        }
        /// <summary>
        /// Use the current thread's culture info for conversion
        /// </summary>
        public static string ToTitleCase(this string str)
        {
            string asTitleCase = new string(CharsToTitleCaseArry(str).ToArray());
            return asTitleCase;
        }


        /// <summary>
        /// Overload which uses the culture info with the specified name
        /// </summary>
        public static string ToTitleCase(this string str, string cultureInfoName)
        {
            CultureInfo cultureInfo = new CultureInfo(cultureInfoName);
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Overload which uses the specified culture info
        /// </summary>
        public static string ToTitleCase(this string str, CultureInfo cultureInfo)
        {
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Use the current thread's culture info for conversion
        /// </summary>
        public static string ToPascalCase(this string str)
        {
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower()).StripSpaces();
        }

        public static string ToSentenceCase(this string str)
        {
            return Regex.Replace(str, "[a-z][A-Z][0-9]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
        }

        public static string CamelCaseToString(this string str, bool titleCase = true)
        {
            if (str == null || str.Length == 0)
            {
                return null;
            }

            StringBuilder retVal = new StringBuilder(32);

            retVal.Append(char.ToUpper(str[0]));
            for (int i = 1; i < str.Length; i++)
            {
                if ((char.IsUpper(str[i]) && !char.IsUpper(str[i - 1])) || (char.IsNumber(str[i]) && !char.IsNumber(str[i - 1])))
                {
                    retVal.Append(" ");
                    if (titleCase)
                    {
                        retVal.Append(char.ToUpper(str[i]));
                    }
                    else
                    {
                        retVal.Append(char.ToLower(str[i]));
                    }
                }
                else
                {
                    retVal.Append(str[i]);
                }
            }

            return retVal.ToString();
        }
        public static string StripLineBreaks(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty);
        }

        public static string StripDoubleSpaces(this string value)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            value = regex.Replace(value, @" ");
            return value;
        }

        public static string StripSpaces(this string value)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{1,}", options);
            value = regex.Replace(value, @"");
            return value;
        }

        public static string ToSeoUrl(this string url)
        {
            // make the url lowercase
            string encodedUrl = (url ?? "").ToLower();

            // replace & with and
            encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

            // remove characters
            encodedUrl = encodedUrl.Replace("'", "");

            // remove invalid characters
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "-");

            // remove duplicates
            encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");

            // trim leading & trailing characters
            encodedUrl = encodedUrl.Trim('-');

            return encodedUrl;
        }
        public static string ToAzureFilename(this string url)
        {
            // make the url lowercase
            string encodedUrl = (url ?? "").ToLower();

            // replace & with and
            encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

            // remove characters
            encodedUrl = encodedUrl.Replace("'", "");

            // remove invalid characters
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9/]", "-");

            // remove duplicates
            encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");

            // trim leading & trailing characters
            encodedUrl = encodedUrl.Trim('-');

            return encodedUrl;
        }

        public static string ToHashTag(this string url)
        {
            // make the url lowercase
            string encodedUrl = (url ?? "");

            // replace & with and
            encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

            // remove characters
            encodedUrl = encodedUrl.Replace("'", "");

            // remove invalid characters
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "-");

            // remove duplicates
            encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");

            // trim leading & trailing characters
            encodedUrl = encodedUrl.Trim('-');

            return encodedUrl;
        }

        private static IEnumerable<char> CharsToTitleCaseArry(string s)
        {
            bool newWord = true;
            foreach (char c in s)
            {
                if (newWord) { yield return char.ToUpper(c); newWord = false; }
                else
                {
                    yield return char.ToLower(c);
                }

                if (c == ' ')
                {
                    newWord = true;
                }
            }
        }

        public static string Truncate(this string value, int length)
        {
            if (value == null || value.Length <= length)
            {
                return value;
            }

            return value.Substring(0, length) + "...";
        }

        public static string TruncateAtWord(this string value, int length)
        {
            if (value == null || value.Length < length || value.IndexOf(" ", length) == -1)
            {
                return value;
            }

            return value.Substring(0, value.IndexOf(" ", length)) + "...";
        }

        public static string ToHexString(this string str)
        {
            StringBuilder sb = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(str);
            foreach (byte t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        public static string FromHexString(this string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }

        public static string ToFormattedJson(this string str)
        {
            string indentString = "    ";
            int indent = 0;
            bool quoted = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(indentString));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(indentString));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        int index = i;
                        while (index > 0 && str[--index] == '\\')
                        {
                            escaped = !escaped;
                        }

                        if (!escaped)
                        {
                            quoted = !quoted;
                        }

                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(indentString));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.Append(" ");
                        }

                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
        public static string ExtractTextBetween(this string theString, string startMarker, string endMarker)
        {
            int startIndex = theString.IndexOf(startMarker) + startMarker.Length;
            int endIndex = theString.IndexOf(endMarker, startIndex);
            return theString.Substring(startIndex, endIndex - startIndex);
        }

        public static string Link(this string s, string url)
        {
            return string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", url, s);
        }
        public static string ParseURL(this string s)
        {
            return Regex.Replace(s, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/\S\w[\w- ;,./?%&=]\S*)?", new MatchEvaluator(URL));
        }
        public static string ParseUsername(this string s)
        {
            return Regex.Replace(s, "(@)((?:[A-Za-z0-9-_]*))", new MatchEvaluator(Username));
        }
        public static string ParseHashtag(this string s)
        {
            return Regex.Replace(s, "(#)((?:[A-Za-z0-9-_]*))", new MatchEvaluator(Hashtag));
        }
        private static string Hashtag(Match m)
        {
            string x = m.ToString();
            string tag = x.Replace("#", "%23");
            return x.Link("http://twitter.com/search?q=" + tag);
        }
        private static string Username(Match m)
        {
            string x = m.ToString();
            string username = x.Replace("@", "");
            return x.Link("http://twitter.com/" + username);
        }
        private static string URL(Match m)
        {
            string x = m.ToString();
            return x.Link(x);
        }

        public static string ToFirstName(this string fullName)
        {
            if (!fullName.IsSet())
            {
                return fullName;
            }

            List<string> names = fullName.Split(' ').ToList();
            return names.First();
        }
        public static string ToLastName(this string fullName)
        {
            if (!fullName.IsSet())
            {
                return fullName;
            }

            List<string> names = fullName.Split(' ').ToList();
            names.RemoveAt(0);
            return string.Join(" ", names.ToArray());
        }
    }
}
