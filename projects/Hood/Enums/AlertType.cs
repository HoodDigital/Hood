namespace Hood.Enums
{
    public enum AlertType
    {
        Success,
        Info,
        Warning,
        Danger,
        Dark,
        Light,
        Primary,
        Secondary
    }
    public enum AlertSize
    {        
        Small,
        Medium,
        Large,
        Epic
    }
    public static class AlertExtensions
    {
        public static string ToCssClass(this AlertType alertType)
        {
            return $"alert-{alertType.ToString().ToLower()}";
        }
        public static string ToIconSizeCssClass(this AlertSize alertSize)
        {
            switch (alertSize)
            {
                case AlertSize.Large:
                    return $"fa fa-2x";
                case AlertSize.Epic:
                    return $"fa fa-3x";
            }
            return "fa";
        }
    }
}
