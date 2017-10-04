using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNumeric(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            return (
                typeCode == TypeCode.Byte ||
                typeCode == TypeCode.Decimal ||
                typeCode == TypeCode.Double ||
                typeCode == TypeCode.Int16 ||
                typeCode == TypeCode.Int32 ||
                typeCode == TypeCode.Int64 ||
                typeCode == TypeCode.SByte ||
                typeCode == TypeCode.Single ||
                typeCode == TypeCode.UInt16 ||
                typeCode == TypeCode.UInt32 ||
                typeCode == TypeCode.UInt64
                );
        }

        public static bool IsDateTime(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            return typeCode == TypeCode.DateTime;
        }

        public static bool IsBoolean(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            return typeCode == TypeCode.Boolean;
        }
    }
}