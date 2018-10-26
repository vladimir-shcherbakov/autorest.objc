using System.Collections.Generic;
using System.Globalization;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using AutoRest.Extensions.Azure;
using Newtonsoft.Json;

namespace AutoRest.ObjectiveC.Model
{
    public class ParameterOc : Parameter
    {
        public ParameterOc()
            : base()
        {
            _implImports = new List<string>();
            Name.OnGet += name => !IsClientProperty
                ? name
                : $"self.{name}"
//                : string.Format(CultureInfo.InvariantCulture,
//                        "{0}.{1}()",
//                        Method != null && true == Method.Group.IsNullOrEmpty() ? "this" : "this.client",
//                        ClientProperty.Name.ToCamelCase())
                ;

            DefaultValue.OnGet += defaultValue => defaultValue;
        }

        [JsonIgnore]
        public bool WantNullable => IsXNullable ?? !IsRequired;

        /// <summary>
        /// Gets True if parameter can call .Validate method
        /// </summary>
        [JsonIgnore]
        public virtual bool CanBeValidated => !IsODataFilterExpression;

        /// <summary>
        /// Gets True if parameter is OData $filter, $top, $orderby, $expand, $skip expression
        /// </summary>
        [JsonIgnore]
        public virtual bool IsODataFilterExpression => base.Extensions.ContainsKey(AzureExtensions.ODataExtension);

        public override IModelType ModelType
        {
            get
            {
                if (base.ModelType == null)
                {
                    return null;
                }
                return WantNullable
                    ? base.ModelType
                    : (base.ModelType as IModelTypeOc)?.NonNullableVariant;
            }
            set => base.ModelType = value;
        }

        [JsonIgnore]
        public IModelTypeOc ClientType => ((IModelTypeOc)ModelType).ParameterVariant;

        [JsonIgnore]
        public IModelTypeOc WireType
        {
            get
            {
                if (ModelType.IsPrimaryType(KnownPrimaryType.Stream))
                {
                    var res = new PrimaryTypeOc(KnownPrimaryType.Stream)
                    {
                        Name =
                        {
                            FixedValue = "RequestBody"
                        }
                    };
                    return res;
                }
                else if (!ModelType.IsPrimaryType(KnownPrimaryType.Base64Url) &&
                    Location != Core.Model.ParameterLocation.Body &&
                    Location != Core.Model.ParameterLocation.FormData &&
                    NeedsSpecialSerialization(ClientType))
                {
                    return new PrimaryTypeOc(KnownPrimaryType.String);
                }
                else
                {
                    return (IModelTypeOc) ModelType;
                }
            }
        }

        [JsonIgnore]
        public string WireName => NeedsConversion ? this.Name.ToCamelCase() + "Converted" : this.Name.ToString();

        [JsonIgnore]
        public bool NeedsConversion => !ClientType.StructurallyEquals(WireType);

        public string ConvertToWireType(string source, string clientReference)
        {
            if (Location != Core.Model.ParameterLocation.Body &&
                Location != Core.Model.ParameterLocation.FormData &&
                NeedsSpecialSerialization(ModelType))
            {
                var primary = ClientType as PrimaryTypeOc;
                var sequence = ClientType as SequenceTypeOc;
                if (primary != null && primary.IsPrimaryType(KnownPrimaryType.ByteArray))
                {
                    if (WireType.IsPrimaryType(KnownPrimaryType.String))
                    {
                        return string.Format(CultureInfo.InvariantCulture, "{0} {1} = Base64.encodeBase64String({2});", WireType.Name, WireName, source);
                    }
                    else
                    {
                        return string.Format(CultureInfo.InvariantCulture, "{0} {1} = Base64Url.encode({2});", WireType.Name, WireName, source);
                    }
                }
                else if (sequence != null)
                {
                    return string.Format(CultureInfo.InvariantCulture,
                        "{0} {1} = {2}.serializerAdapter().serializeList({3}, CollectionFormat.{4});",
                        WireType.Name,
                        WireName,
                        clientReference,
                        source,
                        CollectionFormat.ToString().ToUpperInvariant());
                }
            }

            return ConvertClientTypeToWireType(WireType, source, WireName, clientReference);
        }

        private string ConvertClientTypeToWireType(IModelTypeOc wireType, string source, string target, string clientReference, int level = 0)
        {
            IndentedStringBuilder builder = new IndentedStringBuilder();
            if (wireType.IsPrimaryType(KnownPrimaryType.DateTimeRfc1123))
            {
                if (!IsRequired)
                {
                    builder.AppendLine("DateTimeRfc1123 {0} = {1};", target, wireType.GetDefaultValue(Method) ?? "null")
                        .AppendLine("if ({0} != null) {{", source).Indent();
                }
                builder.AppendLine("{0}{1} = new DateTimeRfc1123({2});", IsRequired ? "DateTimeRfc1123 " : "", target, source);
                if (!IsRequired)
                {
                    builder.Outdent().AppendLine("}");
                }
            }
            else if (wireType.IsPrimaryType(KnownPrimaryType.UnixTime))
            {
                if (!IsRequired)
                {
                    builder.AppendLine("Long {0} = {1};", target, wireType.GetDefaultValue(Method) ?? "null")
                        .AppendLine("if ({0} != null) {{", source).Indent();
                }
                builder.AppendLine("{0}{1} = {2}.toDateTime(DateTimeZone.UTC).getMillis() / 1000;", IsRequired ? "Long " : "", target, source);
            }
            else if (wireType.IsPrimaryType(KnownPrimaryType.Base64Url))
            {
                if (!IsRequired)
                {
                    builder.AppendLine("Base64Url {0} = {1};", target, wireType.GetDefaultValue(Method) ?? "null")
                        .AppendLine("if ({0} != null) {{", source).Indent();
                }
                builder.AppendLine("{0}{1} = Base64Url.encode({2});", IsRequired ? "Base64Url " : "", target, source);
                if (!IsRequired)
                {
                    builder.Outdent().AppendLine("}");
                }
            }
            else if (wireType.IsPrimaryType(KnownPrimaryType.Stream))
            {
                if (!IsRequired)
                {
                    builder.AppendLine("RequestBody {0} = {1};", target, wireType.GetDefaultValue(Method) ?? "null")
                        .AppendLine("if ({0} != null) {{", source).Indent();
                }
                builder.AppendLine("{0}{1} = RequestBody.create(MediaType.parse(\"{2}\"), {3});",
                    IsRequired ? "RequestBody " : "", target, Method.RequestContentType, source);
                if (!IsRequired)
                {
                    builder.Outdent().AppendLine("}");
                }
            }
            else if (wireType is SequenceTypeOc)
            {
                if (!IsRequired)
                {
                    builder.AppendLine("{0} {1} = {2};", WireType.Name, target, wireType.GetDefaultValue(Method) ?? "null")
                        .AppendLine("if ({0} != null) {{", source).Indent();
                }
                var sequenceType = wireType as SequenceTypeOc;
                var elementType = sequenceType.ElementType as IModelTypeOc;
                var itemName = string.Format(CultureInfo.InvariantCulture, "item{0}", level == 0 ? "" : level.ToString(CultureInfo.InvariantCulture));
                var itemTarget = string.Format(CultureInfo.InvariantCulture, "value{0}", level == 0 ? "" : level.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("{0}{1} = new ArrayList<{2}>();", IsRequired ? wireType.Name + " " : "", target, elementType.Name)
                    .AppendLine("for ({0} {1} : {2}) {{", elementType.ParameterVariant.Name, itemName, source)
                    .Indent().AppendLine(ConvertClientTypeToWireType(elementType, itemName, itemTarget, clientReference, level + 1))
                        .AppendLine("{0}.add({1});", target, itemTarget)
                    .Outdent().Append("}");
                _implImports.Add("java.util.ArrayList");
                if (!IsRequired)
                {
                    builder.Outdent().AppendLine("}");
                }
            }
            else if (wireType is DictionaryTypeOc)
            {
                if (!IsRequired)
                {
                    builder.AppendLine("{0} {1} = {2};", WireType.Name, target, wireType.GetDefaultValue(Method) ?? "null")
                        .AppendLine("if ({0} != null) {{", source).Indent();
                }
                var dictionaryType = wireType as DictionaryTypeOc;
                var valueType = dictionaryType.ValueType as IModelTypeOc;
                var itemName = string.Format(CultureInfo.InvariantCulture, "entry{0}", level == 0 ? "" : level.ToString(CultureInfo.InvariantCulture));
                var itemTarget = string.Format(CultureInfo.InvariantCulture, "value{0}", level == 0 ? "" : level.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("{0}{1} = new HashMap<String, {2}>();", IsRequired ? wireType.Name + " " : "", target, valueType.Name)
                    .AppendLine("for (Map.Entry<String, {0}> {1} : {2}.entrySet()) {{", valueType.ParameterVariant.Name, itemName, source)
                    .Indent().AppendLine(ConvertClientTypeToWireType(valueType, itemName + ".getValue()", itemTarget, clientReference, level + 1))
                        .AppendLine("{0}.put({1}.getKey(), {2});", target, itemName, itemTarget)
                    .Outdent().Append("}");
                _implImports.Add("java.util.HashMap");
                if (!IsRequired)
                {
                    builder.Outdent().AppendLine("}");
                }
            }
            return builder.ToString();
        }

        [JsonIgnore]
        public IEnumerable<string> InterfaceImports => ClientType.Imports;

        [JsonIgnore]
        public IEnumerable<string> RetrofitImports
        {
            get
            {
                var imports = new List<string>();
                // type imports
                if (this.Location == Core.Model.ParameterLocation.Body || !NeedsSpecialSerialization(ModelType))
                {
                    imports.AddRange(WireType.Imports);
                }
                // parameter location
                imports.Add(LocationImport(this.Location));
                return imports;
            }
        }

        private List<string> _implImports;

        [JsonIgnore]
        public IEnumerable<string> ClientImplImports => ClientType.Imports;

        [JsonIgnore]
        public IEnumerable<string> WireImplImports
        {
            get
            {
                var imports = new List<string>(WireType.Imports);
                if (Location != Core.Model.ParameterLocation.Body)
                {
                    if (this.ModelType.IsPrimaryType(KnownPrimaryType.ByteArray))
                    {
                        imports.Add("org.apache.commons.codec.binary.Base64");
                    }
                    else if (this.ModelType is SequenceType)
                    {
                        imports.Add("com.microsoft.rest.CollectionFormat");
                    }
                }
                if (ModelType.IsPrimaryType(KnownPrimaryType.Stream) && Location == Core.Model.ParameterLocation.Body)
                {
                    imports.Add("okhttp3.RequestBody");
                    imports.Add("okhttp3.MediaType");
                }
                return imports;
            }
        }

        private static string LocationImport(ParameterLocation parameterLocation)
        {
            if (parameterLocation == Core.Model.ParameterLocation.FormData)
            {
                return "retrofit2.http.Part";
            }
            else if (parameterLocation != Core.Model.ParameterLocation.None)
            {
                return "retrofit2.http." + parameterLocation.ToString();
            }
            else
            {
                return null;
            }
        }

        private static bool NeedsSpecialSerialization(IModelType type)
        {
            return type is PrimaryType known &&
                type.IsPrimaryType(KnownPrimaryType.ByteArray) ||
                type is SequenceType;
        }

        public string ActualDefaultValue
        {
            get
            {
                if (!string.IsNullOrEmpty(DefaultValue))
                {
                    if (ModelType is PrimaryTypeOc)
                        return DefaultValue.Value;
                    if (ModelType is EnumTypeOc enumType)
                        return enumType.Name == "NSString" 
                            ? $"@\"{DefaultValue}\"" 
                            : $"[[{Name} values] firstObject]";
                    //return DefaultValue;

                }
                return "nil";
            }
        }
    }
}
