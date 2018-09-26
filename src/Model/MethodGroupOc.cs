// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using Newtonsoft.Json;

namespace AutoRest.ObjectiveC.Model
{
    public class MethodGroupOc : MethodGroup
    {
        public MethodGroupOc()
        {
            Name.OnGet += Core.Utilities.Extensions.ToCamelCase;
        }
        public MethodGroupOc(string name) : base(name)
        {
            Name.OnGet += Core.Utilities.Extensions.ToCamelCase;
        }

        [JsonIgnore]
        public string MethodGroupFullType
        {
            get
            {
                return (CodeModel.Namespace.ToLowerInvariant()) + "." + TypeName;
            }
        }

        [JsonIgnore]
        public virtual string MethodGroupDeclarationType
        {
            get
            {
                return TypeName;
            }
        }

        [JsonIgnore]
        public string MethodGroupImplType
        {
            get
            {
                return TypeName + ImplClassSuffix;
            }
        }

        [JsonIgnore]
        public virtual string ImplClassSuffix
        {
            get
            {
                return "Impl";
            }
        }

        [JsonIgnore]
        public virtual string ParentDeclaration
        {
            get
            {
                return " implements " + MethodGroupTypeString;
            }
        }

        [JsonIgnore]
        public virtual string ImplPackage
        {
            get
            {
                return "implementation";
            }
        }

        [JsonIgnore]
        public string MethodGroupTypeString
        {
            get
            {
                if (this.Methods
                    .OfType<MethodOc>()
                    .SelectMany(m => m.ImplImports)
                    .Any(i => i.Split('.').LastOrDefault() == TypeName))
                {
                    return MethodGroupFullType;
                }
                return TypeName;
            }
        }

        [JsonIgnore]
        public string MethodGroupServiceType
        {
            get
            {
                return CodeNamerOc.GetServiceName(Name.ToPascalCase());
            }
        }

        [JsonIgnore]
        public virtual string ServiceClientType
        {
            get
            {
                return CodeModel.Name + "Impl";
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> ImplImports
        {
            get
            {
                var imports = new List<string>();
                imports.Add("retrofit2.Retrofit");
                if (MethodGroupTypeString == TypeName)
                {
                    imports.Add(MethodGroupFullType);
                }
                imports.AddRange(this.Methods
                    .OfType<MethodOc>()
                    .SelectMany(m => m.ImplImports)
                    .OrderBy(i => i).Distinct());
                return imports;
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> InterfaceImports
        {
            get
            {
                return this.Methods
                    .OfType<MethodOc>()
                    .SelectMany(m => m.InterfaceImports)
                    .OrderBy(i => i).Distinct();
            }
        }
    }
}