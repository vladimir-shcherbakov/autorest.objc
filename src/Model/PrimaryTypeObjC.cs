// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Model;
using System;
using System.Text;

namespace AutoRest.ObjC.Model
{
    public class PrimaryTypeObjC : PrimaryType
    {
        public bool IsRequired { get; set; }

        public PrimaryTypeObjC() : base()
        {
            Name.OnGet += v =>
            {
                return ImplementationName;
            };
        }

        public PrimaryTypeObjC(KnownPrimaryType primaryType) : base(primaryType)
        {
            Name.OnGet += v =>
            {
                return ImplementationName;
            };
        }

        public virtual string ImplementationName
        {
            get
            {
                switch (KnownPrimaryType)
                {
                    case KnownPrimaryType.Base64Url:
                        // TODO: add support
                        return "NSString*";

                    case KnownPrimaryType.Boolean:
                        return "BOOL";

                    case KnownPrimaryType.Date:
                        return "NSDate*";

                    case KnownPrimaryType.DateTime:
                        return "NSDate*";

                    case KnownPrimaryType.DateTimeRfc1123:
                        return "Date*";

                    case KnownPrimaryType.Double:
                        return "float";

                    case KnownPrimaryType.Decimal:
                        return "double";

                    case KnownPrimaryType.Int:
                        return "int";

                    case KnownPrimaryType.Long:
                        return "long int";

                    case KnownPrimaryType.String:
                        return "NSString*";

                    case KnownPrimaryType.TimeSpan:
                        return "NSTimeInterval*";

                    case KnownPrimaryType.Object:
                        // TODO: is this the correct way to support object types?
                        return "NSObject";

                    case KnownPrimaryType.UnixTime:
                        return "NSDate";

                    case KnownPrimaryType.Uuid:
                        return "NSUUID";

                    case KnownPrimaryType.Stream:
                        return "NSData*";

                    case KnownPrimaryType.ByteArray:
                        return "NSData*";
                }

                throw new NotImplementedException($"Primary type {KnownPrimaryType} is not implemented in {GetType().Name}");
            }
        }

        public static string GetImportLine(string package, string alias = default(string)) {
            var builder = new StringBuilder();
            if(!string.IsNullOrEmpty(alias)){
                builder.Append(alias);
                builder.Append(' ');
            }

            builder.Append('"');
            builder.Append(package);
            builder.Append('"');
            return builder.ToString();
        }
    }
}
