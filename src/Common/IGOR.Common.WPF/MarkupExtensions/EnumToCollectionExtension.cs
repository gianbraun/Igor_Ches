using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace IGOR.Common.WPF.MarkupExtensions
{
    public class EnumToCollectionExtension : MarkupExtension
    {
        public Type EnumType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return EnumType == null
                ? throw new ArgumentNullException(nameof(EnumType))
                : Enum.GetValues(EnumType).Cast<Enum>()
                    .Select(EnumToDescriptionOrString);
                    //.Where(s => s != null);
        }

        private string EnumToDescriptionOrString(Enum value)
        {
            return value.GetType().GetField(value.ToString())
                       .GetCustomAttributes(typeof(DescriptionAttribute), false)
                       .Cast<DescriptionAttribute>()
                       .FirstOrDefault()?.Description ?? null;
        }
    }
}
