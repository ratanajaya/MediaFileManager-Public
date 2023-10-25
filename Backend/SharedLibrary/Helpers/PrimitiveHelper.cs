using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibrary.Helpers
{
    public static class PrimitiveHelper
    {
        #region Number
        public static int GenerateNumberFromSeed(int seed, int upperLimit) {
            int result = seed % upperLimit;
            return result;
        }

        public static int RoundDownToNearest10(this int source) {
            int result = 10 * (source / 10);
            return result;
        }

        public static string BytesToString(long byteCount) {
            string[] suf = { "b", "kb", "mb", "gb", "tb", "pb", "eb" }; //Longs run out around EB
            if(byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
        #endregion

        #region String
        public static string RemoveNonLetterDigit(this string source) {
            string result = new string(source.Where(x => char.IsLetterOrDigit(x)).ToArray());

            return result;
        }

        public static string MapToUpperAlphanumericString(this string source) {
            string result = "";
            foreach(char c in source) {
                result += c.MapToUpperAlphanumericChar();
            }
            return result;
        }

        //Take any Unicode character and return A-Z0-9 character
        public static char MapToUpperAlphanumericChar(this char source) {
            char result;
            //WARNING: Shotgun surgery with MapToUpperAlphanumericChar(this int source)
            const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            if(AllowedChars.Contains(source.ToString().ToUpper())) {
                result = source.ToString().ToUpper().First();
            }
            else {
                int charCode = source;
                int charMapIndex = charCode % AllowedChars.Length;

                result = AllowedChars[charMapIndex];
            }

            return result;
        }

        //WARNING: Shotgun surgery with MapToUpperAlphanumericChar
        public static char MapToUpperAlphanumericChar(this int source) {
            //WARNING: Shotgun surgery with MapToUpperAlphanumericChar(this char source)
            const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int charMapIndex = source % AllowedChars.Length;

            char result = AllowedChars[charMapIndex];

            return result;
        }

        public static string TakeUppercaseAlphanumeric(this string source, int length) {
            source = source.RemoveNonLetterDigit().MapToUpperAlphanumericString();

            var sb = new StringBuilder("");
            for(int i = 0; i < length; i++) {
                sb.Append("0");
            }

            int maxIteration = source.Length >= length ? length : source.Length;
            for(int i = 0; i < maxIteration; i++) {
                sb[i] = source[i];
            }

            string result = sb.ToString();
            return result;
        }

        public static List<string> CleanListString(this List<string> source) {
            return source.Where(s => !string.IsNullOrWhiteSpace(s))
                             .Select(s => s.Trim())
                             .OrderBy(s => s)
                             .ToList();
        }

        const string vowel = "aeiou";
        const string semiVowel = "hrwy";
        const string consonant = "bcdfgjklmnpqstvxz";
        const int offsetStart = 1;

        public static string DiskCensor(this string source) {
            if(string.IsNullOrEmpty(source)) return source;

            var sb = new StringBuilder();
            int offset = offsetStart;
            foreach(char c in source) {
                int vowelIdx = vowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
                if(vowelIdx != -1) {
                    var toAddChar = vowel[(vowelIdx + offset) % vowel.Length];
                    sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                    offset++;
                    continue;
                }

                int semiVowelIdx = semiVowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
                if(semiVowelIdx != -1) {
                    var toAddChar = semiVowel[(semiVowelIdx + offset) % semiVowel.Length];
                    sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                    offset++;
                    continue;
                }

                var consonantIdx = consonant.IndexOf(c, StringComparison.OrdinalIgnoreCase);
                if(consonantIdx != -1) {
                    var toAddChar = consonant[(consonantIdx + offset) % consonant.Length];
                    sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                    offset++;
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string DiskDecensor(this string source) {
            if(string.IsNullOrEmpty(source)) return source;

            int GetClampedIndex(int len, int decrement) {
                if(decrement >= 0) return decrement;

                return GetClampedIndex(len, len + decrement);
            }

            var sb = new StringBuilder();
            int offset = -1 * offsetStart;
            foreach(char c in source) {
                int vowelIdx = vowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
                if(vowelIdx != -1) {
                    var toAddChar = vowel[GetClampedIndex(vowel.Length, vowelIdx + offset)];
                    sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                    offset--;
                    continue;
                }

                int semiVowelIdx = semiVowel.IndexOf(c, StringComparison.OrdinalIgnoreCase);
                if(semiVowelIdx != -1) {
                    var toAddChar = semiVowel[GetClampedIndex(semiVowel.Length, semiVowelIdx + offset)];
                    sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                    offset--;
                    continue;
                }

                var consonantIdx = consonant.IndexOf(c, StringComparison.OrdinalIgnoreCase);
                if(consonantIdx != -1) {
                    var toAddChar = consonant[GetClampedIndex(consonant.Length, consonantIdx + offset)];
                    sb.Append(Char.IsUpper(c) ? Char.ToUpper(toAddChar) : toAddChar);
                    offset--;
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
        #endregion

        #region Collection
        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ContainsAny(this string haystack, params string[] needles) {
            foreach(string needle in needles) {
                if(haystack.ContainsIgnoreCase(needle)) {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsContains(this string[] haystack, string needle) {
            foreach(string s in haystack) {
                if(s.ToLower().Contains(needle.ToLower())) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsContains(this List<string> haystack, string needle) {
            foreach(string s in haystack) {
                if(s.ToLower().Contains(needle.ToLower())) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsIgnoreCase(this string haystack, string needle) {
            if(haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0) {
                return true;
            }
            return false;
        }

        public static bool Contains(this IEnumerable<string> source, string toCheck, StringComparison comp) {
            foreach(string s in source) {
                if(s.Contains(toCheck, comp)) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsExact(this IEnumerable<string> source, string toCheck, StringComparison comp) {
            foreach(string s in source) {
                if(s.Equals(toCheck, comp)) {
                    return true;
                }
            }
            return false;
        }

        public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector) {
            int max = source
                .SelectMany(i => Regex.Matches(selector(i), @"\d+").Cast<Match>().Select(m => (int?)m.Value.Length))
                .Max() ?? 0;

            return source.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
        }
        #endregion

        #region DateTime
        public static string Format(this TimeSpan source) {
            return source.ToString(@"mm\:ss\.fffff");
        }
        #endregion
    }
}
