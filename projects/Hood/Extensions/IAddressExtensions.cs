using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Geocoding;
using Hood.Enums;
using Hood.Interfaces;

namespace Hood.Extensions
{
    public static class IAddressExtensions
    {
        public static void CopyTo(this IAddress from, IAddress to)
        {
            to.ContactName = from.ContactName;
            to.Address1 = from.Address1;
            to.Address2 = from.Address2;
            to.City = from.City;
            to.County = from.County;
            to.Postcode = from.Postcode;
            to.Country = from.Country;
            to.Latitude = from.Latitude;
            to.Longitude = from.Longitude;
        }

        public static TAddress CloneTo<TAddress>(this IAddress from)
            where TAddress : IAddress, new()
        {
            var to = new TAddress();
            from.CopyTo(to);
            return to;
        }

        public static void SetLocation(this IAddress address, Location location)
        {
            if (location != null)
            {
                address.Latitude = location.Latitude;
                address.Longitude = location.Longitude;
            }
        }
        public static bool IsSet(this IAddress from)
        {
            if (from != null && (
                !from.Number.IsSet()
                || !from.Address1.IsSet()
                || !from.Address2.IsSet()
                || !from.City.IsSet()
                || !from.County.IsSet()
                || !from.Postcode.IsSet()
                || !from.Country.IsSet()
                ))
                return true;
            return false;
        }

        public static string ToFormat(this IAddress from, AddressFormat format, bool showPostcode = true, bool showCountry = false)
        {
            string ret = "";
            if (from == null || !from.Address1.IsSet())
                return ret;
            switch (format)
            {
                case AddressFormat.Short:
                    ret = from.Address1;
                    ret += from.City.IsSet() ? ", " + from.City : "";
                    ret += from.Postcode.IsSet() ? ", " + from.Postcode : "";
                    break;
                case AddressFormat.SingleLine:
                    ret = "";
                    ret += from.Number.IsSet() ? from.Number + (from.Address1.IsSet() ? ", " : "") : "";
                    ret += from.Address1.IsSet() ? from.Address1 : "";
                    ret += from.Address2.IsSet() ? ", " + from.Address2 : "";
                    ret += from.City.IsSet() ? ", " + from.City : "";
                    ret += from.County.IsSet() ? ", " + from.County : "";
                    ret += from.Postcode.IsSet() && showPostcode ? ", " + from.Postcode : "";
                    ret += from.Country.IsSet() && showCountry ? ", " + from.Country : "";
                    return ret;
                case AddressFormat.MultiLine:
                    ret = "";
                    ret += from.Number.IsSet() ? from.Number + (from.Address1.IsSet() ? ", " : "") : "";
                    ret += from.Address1.IsSet() ? from.Address1 : "";
                    ret += from.Address2.IsSet() ? Environment.NewLine + from.Address2 : "";
                    ret += from.City.IsSet() ? Environment.NewLine + from.City : "";
                    ret += from.County.IsSet() ? Environment.NewLine + from.County : "";
                    ret += from.Postcode.IsSet() && showPostcode ? Environment.NewLine + from.Postcode : "";
                    ret += from.Country.IsSet() && showCountry ? Environment.NewLine + from.Country : "";
                    return ret;
            }
            return ret;
        }

        public static IHtmlContent FormatAddress(this IHtmlHelper html, IAddress address, AddressFormat format, bool showPostcode = true, bool showCountry = false)
        {
            string ret = "";
            if (address == null || !address.Address1.IsSet())
                return html.Raw(ret);
            switch (format)
            {
                case AddressFormat.Short:
                    ret = address.Address1;
                    ret += address.City.IsSet() ? ", " + address.City : "";
                    ret += address.Postcode.IsSet() ? ", " + address.Postcode : "";
                    break;
                case AddressFormat.SingleLine:
                    ret = address.Address1;
                    ret += address.Address2.IsSet() ? ", " + address.Address2 : "";
                    ret += address.City.IsSet() ? ", " + address.City : "";
                    ret += address.County.IsSet() ? ", " + address.County : "";
                    ret += address.Postcode.IsSet() && showPostcode ? ", " + address.Postcode : "";
                    ret += address.Country.IsSet() && showCountry ? ", " + address.Country : "";
                    break;
                case AddressFormat.MultiLine:
                    ret = address.Address1;
                    ret += address.Address2.IsSet() ? "<br />" + address.Address2 : "";
                    ret += address.City.IsSet() ? "<br />" + address.City : "";
                    ret += address.County.IsSet() ? "<br />" + address.County : "";
                    ret += address.Postcode.IsSet() && showPostcode ? "<br />" + address.Postcode : "";
                    ret += address.Country.IsSet() && showCountry ? "<br />" + address.Country : "";
                    break;
            }
            return html.Raw(ret);
        }

    }
}

