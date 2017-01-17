namespace Hood.Extensions
{
    public static class IntegerExtensions
    {
        public static string ToCurrencyFormat(this int value)
        {
            return string.Format("{0:C}", ((decimal)value / 100));
        }
    }
}
