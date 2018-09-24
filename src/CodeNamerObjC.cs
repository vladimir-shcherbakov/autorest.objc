// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using AutoRest.Core;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using AutoRest.Core.Model;
using AutoRest.ObjC.Model;

namespace AutoRest.ObjC
{
    public class CodeNamerObjC : CodeNamer
    {
        private Dictionary<IModelType, IModelType> _visited = new Dictionary<IModelType, IModelType>();

        public static HashSet<string> PrimaryTypes { get; private set; }

        #region constructor

        /// <summary>
        /// Initializes a new instance of CSharpCodeNamingFramework.
        /// </summary>
        public CodeNamerObjC()
        {
            ReservedWords.AddRange(new []
            {
                "if", "else", "switch", "case", "default", "break", "int", "float", "char", "double", "long", "for", "while", "do",
                "void", "goto", "auto", "signed", "const", "extern", "register", "unsigned", "return", "continue", "enum", "sizeof",
                "struct", "typedef", "union", "volatile"
            });

            PrimaryTypes = new HashSet<string>
            {
                "int", "Integer",
                "long", "Long",
                "object", "Object",
                "bool", "Boolean",
                "double", "Double",
                "float", "Float",
                "byte", "Byte",
                "byte[]", "Byte[]",
                "String",
                "LocalDate",
                "DateTime",
                "DateTimeRfc1123",
                "Duration",
                "Period",
                "BigDecimal",
                "InputStream",
           
                "int",
                "long",
                "bool",
                "double",
                "float",
                "byte",
                "byte[]"
            };
        }

        #endregion

        #region naming

        public override string GetFieldName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            return '_' + GetVariableName(name);
        }

        public override string GetPropertyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            return CamelCase(RemoveInvalidCharacters(GetEscapedReservedName(name, "Property")));
        }

        public override string GetMethodName(string name)
        {
            name = GetEscapedReservedName(name, "Method");
            return CamelCase(name);
        }

        public override string GetMethodGroupName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            name = PascalCase(name);
            if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                name += "s";
            }
            return name;
        }

        public override string GetEnumMemberName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            string result = RemoveInvalidCharacters(new Regex("[\\ -]+").Replace(name, "_"));
            Func<char, bool> isUpper = new Func<char, bool>(c => c >= 'A' && c <= 'Z');
            Func<char, bool> isLower = new Func<char, bool>(c => c >= 'a' && c <= 'z');
            for (int i = 1; i < result.Length - 1; i++)
            {
                if (isUpper(result[i]))
                {
                    if (result[i - 1] != '_' && isLower(result[i - 1]))
                    {
                        result = result.Insert(i, "_");
                    }
                }
            }
            return result.ToUpperInvariant();
        }

        public override string GetParameterName(string name)
        {
            return base.GetParameterName(GetEscapedReservedName(name, "Parameter"));
        }

        public override string GetVariableName(string name)
        {
            return base.GetVariableName(GetEscapedReservedName(name, "Variable"));
        }

        public static string GetServiceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            return name + "Service";
        }

        #endregion

        #region type handling

        public static string GetJavaException(string exception, CodeModel cm)
        {
            switch (exception) {
                case "IOException":
                    return "java.io.IOException";
                case "CloudException":
                    return "com.microsoft.azure.CloudException";
                case "RestException":
                    return "com.microsoft.rest.RestException";
                case "IllegalArgumentException":
                    return null;
                case "InterruptedException":
                    return null;
                default:
                    return (cm.Namespace.ToLowerInvariant())
                        + ".models." + exception;
            }
        }

        #endregion

        public override string EscapeDefaultValue(string defaultValue, IModelType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var primaryType = type as PrimaryType;
            if (defaultValue != null && primaryType != null)
            {
                if (primaryType.KnownPrimaryType == KnownPrimaryType.Double)
                {
                    return double.Parse(defaultValue).ToString();
                }
                if (primaryType.KnownPrimaryType == KnownPrimaryType.String)
                {
                    return QuoteValue(defaultValue);
                }
                else if (primaryType.KnownPrimaryType == KnownPrimaryType.Boolean)
                {
                    return defaultValue.ToLowerInvariant();
                }
                else if (primaryType.KnownPrimaryType == KnownPrimaryType.Long)
                {
                    return defaultValue + "L";
                }
                else
                {
                    if (primaryType.KnownPrimaryType == KnownPrimaryType.Date)
                    {
                        return "LocalDate.parse(\"" + defaultValue + "\")";
                    }
                    else if (primaryType.KnownPrimaryType == KnownPrimaryType.DateTime ||
                        primaryType.KnownPrimaryType == KnownPrimaryType.DateTimeRfc1123)
                    {
                        return "DateTime.parse(\"" + defaultValue + "\")";
                    }
                    else if (primaryType.KnownPrimaryType == KnownPrimaryType.TimeSpan)
                    {
                        return "Period.parse(\"" + defaultValue + "\")";
                    }
                    else if (primaryType.KnownPrimaryType == KnownPrimaryType.ByteArray)
                    {
                        return "\"" + defaultValue + "\".getBytes()";
                    }
                }
            }
            return defaultValue;
        }
    }
}