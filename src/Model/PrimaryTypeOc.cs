using System;
using System.Collections.Generic;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using Newtonsoft.Json;

namespace AutoRest.ObjectiveC.Model
{
    public class PrimaryTypeOc : PrimaryType, IModelTypeOc
    {
        public PrimaryTypeOc()
        {
            Name.OnGet += v => ImplementationName;
        }

        public PrimaryTypeOc(KnownPrimaryType type, string implName = null)
            : base(type)
        {
            Name.OnGet += v => implName ?? ImplementationName;
        }

        public bool WantNullable { get; private set; } = true;

        [JsonIgnore]
        public bool Nullable => true;

        [JsonIgnore]
        public override string DefaultValue
        {
            get
            {
                switch (Name)
                {
                    case "AZInteger":
                        return "@0";
                    case "AZBoolean":
                        return "@NO";
                    case "AZFloat":
                        return "@0.0F";
                    case "AZDouble":
                        return "@0.0";
                    case "AZLong":
                        return "@0L";
                    default:
                        return "nil";
                }

//                if (Nullable)
//                {
//                    return "nil";
//                }
//
//                throw new NotSupportedException(this.Name + " does not have default value!");
            }
        }

        [JsonIgnore]
        public IModelTypeOc ParameterVariant => this;

        [JsonIgnore]
        public IModelTypeOc ResponseVariant
        {
            get
            {
                return this;
            }
        }

        [JsonIgnore]
        public IEnumerable<string> Imports
        {
            get
            {
                switch (KnownPrimaryType)
                {
//                    case KnownPrimaryType.Base64Url:
//                        yield return "com.microsoft.rest.Base64Url";
//                        break;
//                    case KnownPrimaryType.Date:
//                        yield return "org.joda.time.LocalDate";
//                        break;
//                    case KnownPrimaryType.DateTime:
//                        yield return "org.joda.time.DateTime";
//                        break;
//                    case KnownPrimaryType.DateTimeRfc1123:
//                        yield return "com.microsoft.rest.DateTimeRfc1123";
//                        break;
//                    case KnownPrimaryType.Decimal:
//                        yield return "java.math.BigDecimal";
//                        break;
//                    case KnownPrimaryType.Stream:
//                        yield return "java.io.InputStream";
//                        break;
//                    case KnownPrimaryType.TimeSpan:
//                        yield return "org.joda.time.Period";
//                        break;
//                    case KnownPrimaryType.UnixTime:
//                        yield return "org.joda.time.DateTime";
//                        yield return "org.joda.time.DateTimeZone";
//                        break;
//                    case KnownPrimaryType.Uuid:
//                        yield return "java.util.UUID";
//                        break;
                    case KnownPrimaryType.Credentials:
                        yield return "com.microsoft.rest.ServiceClientCredentials";
                        break;
                }
            }
        }

        [JsonIgnore] public IModelTypeOc NonNullableVariant => this;
            

        public string NameForMethod  => $"{Name}*";

        [JsonIgnore]
        public virtual string ImplementationName
        {
            get
            {
                switch (KnownPrimaryType)
                {
                    case KnownPrimaryType.None:
                        return "void";
                    case KnownPrimaryType.Base64Url:
                        return "AZBase64Url";
                    case KnownPrimaryType.Boolean:
                        return "AZBoolean";
                    case KnownPrimaryType.ByteArray:
                        return "AZByteArray";
                    case KnownPrimaryType.Date:
                        return "Date";
                    case KnownPrimaryType.DateTime:
                        return "DateTime";
                    case KnownPrimaryType.DateTimeRfc1123:
                        return "DateTimeRfc1123";
                    case KnownPrimaryType.Double:
                        return "AZDouble";
                    case KnownPrimaryType.Decimal:
                        return "AZDecimal";
                    case KnownPrimaryType.Int:
                        return "AZInteger";
                    case KnownPrimaryType.Long:
                        return "AZLong";
                    case KnownPrimaryType.Stream:
                        return "AZStream";
                    case KnownPrimaryType.String:
                        return "NSString";
                    case KnownPrimaryType.TimeSpan:
                        return "AZTimeSpan";
                    case KnownPrimaryType.UnixTime:
                        return "AZUnixTime";
                    case KnownPrimaryType.Uuid:
                        return "NSUUID";
                    case KnownPrimaryType.Object:
                        return "NSObject";
                    case KnownPrimaryType.Credentials:
                        return "ServiceClientCredentials";
                }
                throw new NotImplementedException($"Primary type {KnownPrimaryType} is not implemented in {GetType().Name}");
            }
        }
    }
}
