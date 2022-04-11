using System.Drawing;

namespace HttpExecutor.Services
{
    public static class ResponseColours
    {
        public static Color StatusCode => Color.FromArgb(214, 137, 16);
        
        public static Color Normal => Color.FromArgb(236, 240, 241);

        public static Color HeaderName => Color.FromArgb(176, 58, 46);

        public static Color HeaderValue => Color.FromArgb(236, 240, 241);

        public static Color JsonPropertyName => Color.FromArgb(241, 196, 15);

        public static Color JsonPropertyValueString => Color.FromArgb(39, 174, 96);

        public static Color JsonPropertyValueStrut => Color.FromArgb(214, 137, 16);
    }
}