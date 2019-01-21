using System;
using System.ComponentModel.DataAnnotations;

namespace SProcWrapper
{
    /// <summary>
    /// Атрибут, уведомляющий, что это сумма, и в БД она в копейках, а в коде в рублях...
    /// При обработке такого поля в ХП происходит умножение/деление на 100 соответсвенно
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MoneyAttribute : DisplayFormatAttribute
    {
        public MoneyAttribute()
        {
            ApplyFormatInEditMode = true;
            DataFormatString = "N2";
        }
    }
}