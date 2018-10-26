// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.ObjectiveC.Model;

namespace AutoRest.ObjectiveC
{
    public static class ClientModelExtensions
    {
        public const string ExternalExtension = "x-ms-external";

        public static string Period(this string documentation)
        {
            if (string.IsNullOrEmpty(documentation))
            {
                return documentation;
            }
            documentation = documentation.Trim();
            if (!documentation.EndsWith(".", StringComparison.Ordinal))
            {
                documentation += ".";
            }
            return documentation;
        }

        public static string Documentation(this Property property)
        {
            if (string.IsNullOrEmpty(property.Documentation))
            {
                return "the " + property.Name + " value.";
            }
            else
            {
                var doc = property.Documentation.EscapeXmlComment().Period();
                return doc.Substring(0, 1).ToLower() + doc.Substring(1);
            }
        }

        public static string TrimMultilineHeader(this string header)
        {
            if (string.IsNullOrEmpty(header))
            {
                return header;
            }
            var builder = new StringBuilder();
            foreach (var headerLine in header.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                builder.Append(headerLine.TrimEnd()).Append(Environment.NewLine);
            }
            return builder.ToString();
        }

        public static string GetJsonProperty(this Property property)
        {
            if (property == null)
            {
                return null;
            }

            var settings = new List<string>
            {
                string.Format(CultureInfo.InvariantCulture, "\"{0}\"", property.SerializedName)
            };
            if (property.IsRequired)
            {
                settings.Add("required = true");
            }
            if (property.IsReadOnly)
            {
                settings.Add("access = JsonProperty.Access.WRITE_ONLY");
            }
            return string.Join(", ", settings);
        }

        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
        {
            if( hashSet == null || range == null)
            {
                return;
            }

            foreach(var item in range)
            {
                hashSet.Add(item);
            }
        }

        /// <summary>
        /// A null friendly wrapper around type imports.
        /// </summary>
        /// <param name="type">an instance of IModelType</param>
        /// <returns>a list of imports to append</returns>
        public static IEnumerable<string> ImportSafe(this IModelType type)
        {
            return type == null ? new List<string>() : ((IModelTypeOc) type).Imports;
        }

        /// <summary>
        /// Generate code to perform required validation on a type
        /// </summary>
        /// <param name="type">The type to validate</param>
        /// <param name="scope">A scope provider for generating variable names as necessary</param>
        /// <param name="valueReference">A reference to the value being validated</param>
        /// <param name="constraints">Constraints</param>
        /// <returns>The code to validate the reference of the given type</returns>
        public static string ValidateType(this IModelType type, IChild scope, string valueReference, 
            Dictionary<Constraint, string> constraints)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var model = type as CompositeTypeOc;
            var sequence = type as SequenceTypeOc;
            var dictionary = type as DictionaryTypeOc;

            var sb = new IndentedStringBuilder();

            if (model != null && model.ShouldValidateChain())
            {
                sb.AppendLine("{0}.Validate();", valueReference);
            }

            if (constraints != null && constraints.Any())
            {
                AppendConstraintValidations(valueReference, constraints, sb, type);
            }

            if (sequence != null && sequence.ShouldValidateChain())
            {
                var elementVar = scope.GetUniqueName("element");
                var innerValidation = sequence.ElementType.ValidateType(scope, elementVar, null);
                if (!string.IsNullOrEmpty(innerValidation))
                {
                    sb.AppendLine("foreach (var {0} in {1})", elementVar, valueReference)
                       .AppendLine("{").Indent()
                           .AppendLine(innerValidation).Outdent()
                       .AppendLine("}");
                }
            }
            else if (dictionary != null && dictionary.ShouldValidateChain())
            {
                var valueVar = scope.GetUniqueName("valueElement");
                var innerValidation = dictionary.ValueType.ValidateType(scope, valueVar, null);
                if (!string.IsNullOrEmpty(innerValidation))
                {
                    sb.AppendLine("foreach (var {0} in {1}.Values)", valueVar, valueReference)
                      .AppendLine("{").Indent()
                          .AppendLine(innerValidation).Outdent()
                      .AppendLine("}").Outdent();
                }
            }

            if (sb.ToString().Trim().Length > 0)
            {
                if (type.IsValueType())
                {
                    return sb.ToString();
                }
                else
                {
                    return CheckNull(valueReference, sb.ToString());
                }
            }

            return null;
        }

        /// <summary>
        /// Determines if the given IModelType is a value type in C#
        /// </summary>
        /// <param name="modelType">The type to check</param>
        /// <returns>True if the type maps to a C# value type, otherwise false</returns>
        public static bool IsValueType(this IModelType modelType) => false;
        
        public static string CheckNull(string valueReference, string executionBlock)
        {
            var sb = new IndentedStringBuilder();
            sb.AppendLine("if ({0} != nil) {{", valueReference)
                .Indent()
                    .AppendLine(executionBlock)
                .Outdent()
                .AppendLine("}");
            return sb.ToString();
        }


        public static bool ShouldValidateChain(this IModelType model)
        {
            if (model == null)
            {
                return false;
            }

            var typesToValidate = new Stack<IModelType>();
            typesToValidate.Push(model);
            var validatedTypes = new HashSet<IModelType>();
            while (typesToValidate.Count > 0)
            {
                IModelType modelToValidate = typesToValidate.Pop();
                if (validatedTypes.Contains(modelToValidate))
                {
                    continue;
                }
                validatedTypes.Add(modelToValidate);

                var sequence = modelToValidate as SequenceType;
                var dictionary = modelToValidate as DictionaryType;
                var composite = modelToValidate as CompositeType;
                if (sequence != null)
                {
                    typesToValidate.Push(sequence.ElementType);
                }
                else if (dictionary != null)
                {
                    typesToValidate.Push(dictionary.ValueType);
                } 
                else if (composite != null)
                {
                    if (composite.ShouldValidate())
                    {
                        return true;
                    }
                    typesToValidate.Push(composite.BaseModelType);
                }
            }

            return false;
        }

        private static bool ShouldValidate(this IModelType model)
        {
            if (model == null)
            {
                return false;
            }

            var typesToValidate = new Stack<IModelType>();
            typesToValidate.Push(model);
            var validatedTypes = new HashSet<IModelType>();
            while (typesToValidate.Count > 0)
            {
                IModelType modelToValidate = typesToValidate.Pop();
                if (validatedTypes.Contains(modelToValidate))
                {
                    continue;
                }
                validatedTypes.Add(modelToValidate);

                var sequence = modelToValidate as SequenceType;
                var dictionary = modelToValidate as DictionaryType;
                var composite = modelToValidate as CompositeType;
                if (sequence != null)
                {
                    typesToValidate.Push(sequence.ElementType);
                } 
                else if (dictionary != null)
                {
                    typesToValidate.Push(dictionary.ValueType);
                } 
                else if (composite != null)
                {
                    composite.Properties
                        .Where(p => p.ModelType is CompositeType)
                        .ForEach(cp => typesToValidate.Push(cp.ModelType));

                    if (composite.Properties.Any(p => (p.IsRequired && !p.IsConstant) || p.Constraints.Any()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Generates a C# string literal for given string, fully escaped and such.
        /// </summary>
        private static string ToLiteral(string input)
        {
            return "\"" + input.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static void AppendConstraintValidations(string valueReference, Dictionary<Constraint, string> constraints, IndentedStringBuilder sb, IModelType type)
        {
            foreach (var constraint in constraints.Keys)
            {

                string constraintCheck;
                var knownFormat = (type as PrimaryType)?.KnownFormat;
                var constraintValue =
                    knownFormat == KnownFormat.@char ? $"'{constraints[constraint]}'" :
                    constraints[constraint];

                var typeName = (type as PrimaryTypeOc)?.ImplementationName;
                var param  = valueReference;
                if (typeName != null)
                    switch (typeName)
                    {
                        case "AZInteger":
                            param = $"[{valueReference} intValue]";
                            break;
                        case "AZBoolean":
                            param = $"[{valueReference} getBool]";
                            break;
                        case "AZFloat":
                            param = $"[{valueReference} floatValue]";
                            break;
                        case "AZDouble":
                            param = $"[{valueReference} doubleValue]";
                            break;
                        case "AZLong":
                            param = $"[{valueReference} longValue]";
                            break;
                        default:
                            break;
                    }

                switch (constraint)
                {
                    case Constraint.ExclusiveMaximum:
                        constraintCheck = $"{param} >= {constraintValue}";
                        break;
                    case Constraint.ExclusiveMinimum:
                        constraintCheck = $"{param} <= {constraintValue}";
                        break;
                    case Constraint.InclusiveMaximum:
                        constraintCheck = $"{param} > {constraintValue}";
                        break;
                    case Constraint.InclusiveMinimum:
                        constraintCheck = $"{param} < {constraintValue}";
                        break;
                    case Constraint.MaxItems:
                        constraintCheck = $"{valueReference}.count > {constraintValue}";
                        break;
                    case Constraint.MaxLength:
                        constraintCheck = $"{valueReference}.length > {constraintValue}";
                        break;
                    case Constraint.MinItems:
                        constraintCheck = $"{valueReference}.count < {constraintValue}";
                        break;
                    case Constraint.MinLength:
                        constraintCheck = $"{valueReference}.length < {constraintValue}";
                        break;
                    case Constraint.MultipleOf:
                        constraintCheck = $"{valueReference} % {constraintValue} != 0";
                        break;
                    case Constraint.Pattern:
                        //constraintValue = ToLiteral(constraintValue);
                        if (type is DictionaryType)
                        {
                            constraintCheck = $"!System.Linq.Enumerable.All({valueReference}.Values, value => System.Text.RegularExpressions.Regex.IsMatch(value, {constraintValue}))";
                        }
                        else
                        {
                            //constraintCheck = $"!System.Text.RegularExpressions.Regex.IsMatch({valueReference}, {constraintValue})";
                            constraintCheck = $"![[NSPredicate predicateWithFormat:@\"SELF MATCHES %@\", @\"{constraintValue}\"]evaluateWithObject:{valueReference}]";
                        }
                        break;
                    case Constraint.UniqueItems:
                        if ("true".EqualsIgnoreCase(constraints[constraint]))
                        {
                            constraintCheck = $"{valueReference}.Count != System.Linq.Enumerable.Count(System.Linq.Enumerable.Distinct({valueReference}))";
                        }
                        else
                        {
                            constraintCheck = null;
                        }
                        break;
                    default:
                        throw new NotSupportedException("Constraint '" + constraint + "' is not supported.");
                }

                if (constraintCheck == null) continue;

                if (constraint != Constraint.UniqueItems)
                {
                    sb.AppendLine("if ({0}) {{", constraintCheck)
                        .Indent()
                            .AppendLine("NSException *e = [NSException")
                            .Indent()
                                .AppendLine("exceptionWithName: @\"IllegalArgumentException\"")
                                .AppendLine($"reason: @\"Parameter '{valueReference}' failed rule validation, rule name: '{constraint}', constrain value: {constraintValue}\"")
                                .AppendLine("userInfo: nil];")
                            .Outdent()
                            .AppendLine("@throw e;")
                        .Outdent()
                        .AppendLine("}");
                }
                else
                {
                    sb.AppendLine("if ({0}) {", constraintCheck)
                        .Indent().AppendLine("throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.{0}, \"{1}\");",
                            constraint, valueReference.Replace("this.", ""))
                        .Outdent().AppendLine("}");
                }
            }
        }

        //        public static string ImportFrom(this HttpMethod httpMethod)
        //        {
        //            var package = "retrofit2.http.";
        //            if (httpMethod == HttpMethod.Delete)
        //            {
        //                return package + "HTTP";
        //            }
        //            else
        //            {
        //                return package + httpMethod.ToString().ToUpperInvariant();
        //            }
        //        }
    }
}
