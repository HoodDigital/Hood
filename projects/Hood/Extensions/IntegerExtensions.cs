namespace Hood.Extensions
{
    public static class IntegerExtensions
    {
        public static string ToCurrencyFormat(this int value)
        {
            return string.Format("{0:C}", ((decimal)value / 100));
        }

        public static string ToPosition(this int number)
        {
            var outputNum = number % 100;
            return number.ToString() + ((outputNum % 10 == 1 && outputNum != 11) ? "st"
                    : (outputNum % 10 == 2 && outputNum != 12) ? "nd"
                    : (outputNum % 10 == 3 && outputNum != 13) ? "rd"
                    : "th");
        }

    }
}
