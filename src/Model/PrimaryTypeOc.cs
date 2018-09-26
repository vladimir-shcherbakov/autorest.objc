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

        public PrimaryTypeOc(KnownPrimaryType type)
            : base(type)
        {
            Name.OnGet += v => ImplementationName;
        }

        public bool WantNullable { get; private set; } = true;

        [JsonIgnore]
        public bool Nullable
        {
            get
            {
                if (WantNullable)
                {
                    return true;
                }
                switch (KnownPrimaryType)
                {
                    case KnownPrimaryType.None:
                    case KnownPrimaryType.Boolean:
                    case KnownPrimaryType.Double:
                    case KnownPrimaryType.Int:
                    case KnownPrimaryType.Long:
                    case KnownPrimaryType.UnixTime:
                        return false;
                }
                return true;
            }
        }

        [JsonIgnore]
        public override string DefaultValue
        {
            get
            {
                if (Name == "byte[]")
                {
                    return "new byte[0]";
                }
                else if (Name == "Byte[]")
                {
                    return "new Byte[0]";
                }
                else if (Nullable)
                {
                    return null;
                }
                else
                {
                    throw new NotSupportedException(this.Name + " does not have default value!");
                }
            }
        }

        [JsonIgnore]
        public IModelTypeOc ParameterVariant
        {
            get
            {
                if (KnownPrimaryType == KnownPrimaryType.DateTimeRfc1123)
                {
                    return new PrimaryTypeOc(KnownPrimaryType.DateTime);
                }
                else if (KnownPrimaryType == KnownPrimaryType.UnixTime)
                {
                    return new PrimaryTypeOc(KnownPrimaryType.DateTime);
                }
                else if (KnownPrimaryType == KnownPrimaryType.Base64Url)
                {
                    return new PrimaryTypeOc(KnownPrimaryType.ByteArray);
                }
                else if (KnownPrimaryType == KnownPrimaryType.Stream)
                {
                    return new PrimaryTypeOc(KnownPrimaryType.ByteArray);
                }
                else
                {
                    return this;
                }
            }
        }

        [JsonIgnore]
        public IModelTypeOc ResponseVariant
        {
            get
            {
                if (KnownPrimaryType == KnownPrimaryType.DateTimeRfc1123)
                {
                    return new PrimaryTypeOc(KnownPrimaryType.DateTime);
                }
                else if (KnownPrimaryType == KnownPrimaryType.UnixTime)
                {
                    return new PrimaryTypeOc(KnownPrimaryType.DateTime);
                }
                else if (KnownPrimaryType == KnownPrimaryType.Base64Url)
                {
                    return new PrimaryTypeOc(KnownPrimaryType.ByteArray);
                }
                else if (KnownPrimaryType == KnownPrimaryType.None)
                {
                    return NonNullableVariant;
                }
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
                    case KnownPrimaryType.Base64Url:
                        yield return "com.microsoft.rest.Base64Url";
                        break;
                    case KnownPrimaryType.Date:
                        yield return "org.joda.time.LocalDate";
                        break;
                    case KnownPrimaryType.DateTime:
                        yield return "org.joda.time.DateTime";
                        break;
                    case KnownPrimaryType.DateTimeRfc1123:
                        yield return "com.microsoft.rest.DateTimeRfc1123";
                        break;
                    case KnownPrimaryType.Decimal:
                        yield return "java.math.BigDecimal";
                        break;
                    case KnownPrimaryType.Stream:
                        yield return "java.io.InputStream";
                        break;
                    case KnownPrimaryType.TimeSpan:
                        yield return "org.joda.time.Period";
                        break;
                    case KnownPrimaryType.UnixTime:
                        yield return "org.joda.time.DateTime";
                        yield return "org.joda.time.DateTimeZone";
                        break;
                    case KnownPrimaryType.Uuid:
                        yield return "java.util.UUID";
                        break;
                    case KnownPrimaryType.Credentials:
                        yield return "com.microsoft.rest.ServiceClientCredentials";
                        break;
                }
            }
        }

        [JsonIgnore]
        public IModelTypeOc NonNullableVariant => 
            new PrimaryTypeOc
            {
                KnownPrimaryType = KnownPrimaryType,
                Format = Format,
                WantNullable = false
            };

        [JsonIgnore]
        public virtual string ImplementationName
        {
            get
            {
                switch (KnownPrimaryType)
                {
                    case KnownPrimaryType.None:
                        return WantNullable ? "Void" : "void";
                    case KnownPrimaryType.Base64Url:
                        return "Base64Url";
                    case KnownPrimaryType.Boolean:
                        return WantNullable ? "Boolean" : "boolean";
                    case KnownPrimaryType.ByteArray:
                        return "byte[]";
                    case KnownPrimaryType.Date:
                        return "LocalDate";
                    case KnownPrimaryType.DateTime:
                        return "DateTime";
                    case KnownPrimaryType.DateTimeRfc1123:
                        return "DateTimeRfc1123";
                    case KnownPrimaryType.Double:
                        return WantNullable ? "Double" : "double";
                    case KnownPrimaryType.Decimal:
                        return "BigDecimal";
                    case KnownPrimaryType.Int:
                        return WantNullable ? "Integer" : "int";
                    case KnownPrimaryType.Long:
                        return WantNullable ? "Long" : "long";
                    case KnownPrimaryType.Stream:
                        return "InputStream";
                    case KnownPrimaryType.String:
                        return "String";
                    case KnownPrimaryType.TimeSpan:
                        return "Period";
                    case KnownPrimaryType.UnixTime:
                        return WantNullable ? "Long" : "long";
                    case KnownPrimaryType.Uuid:
                        return "UUID";
                    case KnownPrimaryType.Object:
                        return "Object";
                    case KnownPrimaryType.Credentials:
                        return "ServiceClientCredentials";
                }
                throw new NotImplementedException($"Primary type {KnownPrimaryType} is not implemented in {GetType().Name}");
            }
        }
    }
}
