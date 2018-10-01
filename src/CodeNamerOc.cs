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
using AutoRest.ObjectiveC.Model;

namespace AutoRest.ObjectiveC
{
    public class CodeNamerOc : CodeNamer
    {
        private Dictionary<IModelType, IModelType> _visited = new Dictionary<IModelType, IModelType>();

        public static HashSet<string> PrimaryTypes { get; private set; }

        #region constructor

        /// <summary>
        /// Initializes a new instance of CSharpCodeNamingFramework.
        /// </summary>
        public CodeNamerOc()
        {
            // List retrieved from
            // http://docs.oracle.com/javase/tutorial/java/nutsandbolts/_keywords.html
            ReservedWords.AddRange(new []
            {
                "if", "else", "switch", "case", "default", "break", "int", "float", "char", "double", "long", "for", "while", "do",
                "void", "goto", "auto", "signed", "const", "extern", "register", "unsigned", "return", "continue", "enum", "sizeof",
                "struct", "typedef", "union", "volatile",
                "description"
            });

            PrimaryTypes = new HashSet<string>
            {
                "int",
                "long",
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
            return string.IsNullOrWhiteSpace(name) 
                ? name
                : CamelCase(RemoveInvalidCharacters(GetEscapedReservedName(name, "Property")));
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
            for (var i = 1; i < result.Length - 1; i++)
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
//            switch (exception) {
//                case "IOException":
//                    return "java.io.IOException";
//                case "CloudException":
//                    return "com.microsoft.azure.CloudException";
//                case "RestException":
//                    return "com.microsoft.rest.RestException";
//                case "IllegalArgumentException":
//                    return null;
//                case "InterruptedException":
//                    return null;
//                default:
//                    return (cm.Namespace.ToLowerInvariant())
//                        + ".models." + exception;
//            }
            return "";
        }

        #endregion

        public override string EscapeDefaultValue(string defaultValue, IModelType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (defaultValue != null && type is PrimaryType primaryType)
            {
                if (primaryType.KnownPrimaryType == KnownPrimaryType.Double)
                {
                    return double.Parse(defaultValue).ToString(CultureInfo.InvariantCulture);
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