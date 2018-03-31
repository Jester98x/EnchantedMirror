using Windows.UI.Xaml;

namespace EnchantedMirror.Extensions
{
    public static class StringExtensions
    {
        public static VerticalAlignment ToVerticalAlignment(this string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "CENTER":
                    return VerticalAlignment.Center;

                case "BOTTOM":
                    return VerticalAlignment.Bottom;

                case "STRETCH":
                    return VerticalAlignment.Stretch;

                default:
                    return VerticalAlignment.Top;
            }
        }

        public static HorizontalAlignment ToHorizontalAlignment(this string value)
        {
            switch(value.ToUpperInvariant())
            {
                case "CENTER":
                    return HorizontalAlignment.Center;

                case "RIGHT":
                    return HorizontalAlignment.Right;

                case "STRETCH":
                    return HorizontalAlignment.Stretch;

                default:
                    return HorizontalAlignment.Left;
            }
        }
    }
}
