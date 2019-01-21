using Newtonsoft.Json;
using SProcWrapper.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SProcWrapper.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Глубокое копирование объекта с помощью сериализации/десериализации в Json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">объект для копирования.</param>
        /// <returns></returns>
        public static T Copy<T>(this T source)
        {
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new WritablePropertiesOnlyResolver()
            };

            var deserializeSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ContractResolver = new IncludePrivateStateContractResolver()
            };


            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializerSettings),
                deserializeSettings);
        }

        /// <summary>
        /// Копирование публичных свойств объекта
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">целевой объект</param>
        /// <param name="source">объект для копирования.</param>
        /// <returns></returns>
        public static void CopyProperties<T>(this T self, T source)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite);
            foreach (var property in properties)
                property.SetValue(self, property.GetValue(source));
        }

        /// <summary>
        /// Метод выбрасывает исключение, если переданный объект равен null.
        /// </summary>
        /// <typeparam name="T">Тип переданного объекта</typeparam>
        /// <param name="item">Передаваемый объект для проверки</param>
        /// <param name="message">Сообщение ошибки, если возникло исключение</param>
        /// <param name="paramName">Имя параметра</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Если объект null</exception>
        public static T ThrowIfNull<T>(this T item, string message = "", string paramName = "") where T : class
        {
            if (item != null) return item;

            var msg = string.IsNullOrWhiteSpace(message)
                ? $"Аргумент '{paramName}' не может быть равен null."
                : message;

            throw new ArgumentNullException(paramName, msg);
        }
    }
}