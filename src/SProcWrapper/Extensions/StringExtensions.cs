using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SProcWrapper.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsWithIgnoreCase(this string s1, string s2, bool ignoreCase = true)
        {
            return ignoreCase ? string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase) : string.Equals(s1, s2);
        }

        public static string Frmt(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", " ").Trim();
        }

        public static string ReplaceAt(this string value, int index, char newchar)
        {
            return value.Length <= index ? value : string.Concat(value.Select((c, i) => i == index ? newchar : c));
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static string[] ToStringArray(this string source)
        {
            source.ThrowIfNullOrWhiteSpace(paramName: nameof(source));
            return source.Select((c, i) => c.ToString(CultureInfo.InvariantCulture)).ToArray();
        }

        /// <summary>
        /// Делаем строку с заглавной буквой
        /// </summary>
        /// <param name="str">исходная строка</param>
        /// <returns></returns>
        public static string FirstCharToUpper(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;

            return char.ToUpper(str.First()) + str.Substring(1);
        }

        /// <summary>
        /// Возвращаем null если пустая строка
        /// </summary>
        /// <param name="str">исходная строка</param>
        /// <returns></returns>
        public static string NullIfEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }

        /// <summary>
        /// Метод выбрасывает исключение, если строка равна null или содержит только пробельные символы.
        /// </summary>
        /// <param name="str">Проверяемая строка</param>
        /// <param name="message">Сообщение ошибки, если возникло исключение</param>
        /// <param name="paramName">Имя параметра</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Если строка null или пустая.</exception>
        public static string ThrowIfNullOrWhiteSpace(this string str, string message = "", string paramName = "")
        {
            if (!string.IsNullOrWhiteSpace(str)) return str;

            var msg = string.IsNullOrWhiteSpace(message)
                ? $"Аргумент '{paramName}' не может быть равен null, пустым или содержать только пробельные символы."
                : message;

            throw new ArgumentException(msg, paramName);
        }
    }
}