using System.Text.RegularExpressions;
using UnityEngine;

namespace Synapse.Client.Util
{
    public static class UIUtil
    {
        public enum TextColor
        {
            White,
            Green,
            Yellow,
            Red,
            Gray
        }
        
        public static string GetColoredString(string input, TextColor color)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = RemoveColorTags(input);
            return $"<color={GetColorHex(color)}>{input}</color>";
        }

        private static string RemoveColorTags(string input)
        {
            input = Regex.Replace(input, @"<\s*color\s*=\s*#?[0-9a-fA-F]{6,8}\s*>", string.Empty);
            input = Regex.Replace(input, @"<\s*/\s*color\s*>", string.Empty);
            return input;
        }

        private static string GetColorHex(TextColor color)
        {
            switch (color)
            {
                case TextColor.Green:  return "#2ECC71";
                case TextColor.Yellow: return "#F1C40F";
                case TextColor.Red:    return "#E74C3C";
                case TextColor.Gray:   return "#A0A0A0";
                case TextColor.White:
                default:
                    return "#FFFFFF";
            }
        }
    }    
}