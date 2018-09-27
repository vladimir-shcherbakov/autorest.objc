// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using Newtonsoft.Json;

namespace AutoRest.ObjC.Model
{
    public class MethodGroupObjC : MethodGroup
    {
        public MethodGroupObjC()
        {
            Name.OnGet += Core.Utilities.Extensions.ToCamelCase;
        }
        public MethodGroupObjC(string name) : base(name)
        {
            Name.OnGet += Core.Utilities.Extensions.ToCamelCase;
        }

        [JsonIgnore]
        public string MethodGroupFullType => (CodeModel.Namespace.ToLowerInvariant()) + "." + TypeName;

        [JsonIgnore]
        public virtual string MethodGroupDeclarationType => TypeName;

        [JsonIgnore]
        public string MethodGroupImplType => TypeName + ImplClassSuffix;

        [JsonIgnore]
        public virtual string ImplClassSuffix => "Impl";

        [JsonIgnore]
        public virtual string ParentDeclaration => " implements " + MethodGroupTypeString;

        [JsonIgnore]
        public virtual string ImplPackage => "implementation";

        [JsonIgnore]
        public string MethodGroupTypeString
        {
            get
            {
//                if (this.Methods
//                    .OfType<MethodObjC>()
//                    .SelectMany(m => m.ImplImports)
//                    .Any(i => i.Split('.').LastOrDefault() == TypeName))
//                {
//                    return MethodGroupFullType;
//                }
                return TypeName;
            }
        }

        [JsonIgnore]
        public string MethodGroupServiceType => CodeNamerObjC.GetServiceName(Name.ToPascalCase());

        [JsonIgnore]
        public virtual string ServiceClientType => CodeModel.Name + "Impl";

        [JsonIgnore]
        public virtual IEnumerable<string> ImplImports
        {
            get
            {
                var imports = new List<string> {"retrofit2.Retrofit"};
//                if (MethodGroupTypeString == TypeName)
//                {
//                    imports.Add(MethodGroupFullType);
//                }
//                imports.AddRange(this.Methods
//                    .OfType<MethodObjC>()
//                    .SelectMany(m => m.ImplImports)
//                    .OrderBy(i => i).Distinct());
                return imports;
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> InterfaceImports
        {
            get
            {
                return this.Methods
                    .OfType<MethodObjC>()
                    .SelectMany(m => m.InterfaceImports)
                    .OrderBy(i => i).Distinct();
            }
        }
    }
}