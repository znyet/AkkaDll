using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;


namespace TestMessagePack
{

    public class DateTimeResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new DateTimeResolver();

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            // generic's static constructor should be minimized for reduce type generation size!
            // use outer helper method.
            static FormatterCache()
            {
                formatter = (IMessagePackFormatter<T>)SampleCustomResolverGetFormatterHelper.GetFormatter(typeof(T));
            }
        }

    }

    class DateTimeFormat : IMessagePackFormatter<DateTime>
    {
        public int Serialize(ref byte[] bytes, int offset, DateTime value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteString(ref bytes, offset, value.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public DateTime Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return DateTime.MinValue;
            }
            var value = MessagePackBinary.ReadString(bytes, offset, out readSize);
            return Convert.ToDateTime(value);

        }
    }

    internal static class SampleCustomResolverGetFormatterHelper
    {
        // If type is concrete type, use type-formatter map
        static readonly Dictionary<Type, object> formatterMap = new Dictionary<Type, object>()
        {
            {typeof(DateTime), new DateTimeFormat()}
            // add more your own custom serializers.
        };

        internal static object GetFormatter(Type t)
        {
            object formatter;
            if (formatterMap.TryGetValue(t, out formatter))
            {
                return formatter;
            }

            // If target type is generics, use MakeGenericType.
            if (t.IsGenericParameter && t.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                return Activator.CreateInstance(typeof(ValueTupleFormatter<,>).MakeGenericType(t.GenericTypeArguments));
            }

            // If type can not get, must return null for fallback mecanism.
            return null;
        }
    }


}

//CompositeResolver.RegisterAndSetAsDefault(
//    //NativeDateTimeResolver.Instance,
//    DateTimeResolver.Instance,
//    StandardResolver.Instance
//);