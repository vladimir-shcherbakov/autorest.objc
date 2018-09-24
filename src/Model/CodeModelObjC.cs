﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using AutoRest.Core.Model;
using Newtonsoft.Json;

namespace AutoRest.ObjC.Model
{
    public class CodeModelObjC : CodeModel
    {
        public override string BaseUrl
        {
            get
            {
                if (!base.BaseUrl.Contains("://"))
                {
                    return $"https://{base.BaseUrl}";
                }
                return base.BaseUrl;
            }
            set
            {
                base.BaseUrl = value;
            }
        }

        [JsonIgnore]
        public IEnumerable<MethodGroupObjC> AllOperations => Operations.Where(operation => !operation.Name.IsNullOrEmpty()).Cast<MethodGroupObjC>();

        [JsonIgnore]
        public bool IsCustomBaseUri => Extensions.ContainsKey(SwaggerExtensions.ParameterizedHostExtension);

        [JsonIgnore]
        public string ServiceClientServiceType
        {
            get
            {
                return CodeNamerObjC.GetServiceName(Name.ToPascalCase());
            }
        }

        [JsonIgnore]
        public virtual string ImplPackage => "implementation";

        [JsonIgnore]
        public string ModelsPackage => ".models";

        [JsonIgnore]
        public IEnumerable<MethodObjC> RootMethods => Methods.Where(m => m.Group.IsNullOrEmpty()).OfType<MethodObjC>();

        [JsonIgnore]
        public string FullyQualifiedDomainName => Namespace.ToLowerInvariant() + "." + this.Name;

        [JsonIgnore]
        public virtual IEnumerable<string> ImplImports
        {
            get
            {
                var classes = new HashSet<string> {FullyQualifiedDomainName};
                foreach(var methodGroupFullType in this.AllOperations.Select(op => op.MethodGroupFullType).Distinct())
                {
                    classes.Add(methodGroupFullType);
                }
                if (this.Properties.Any(p => p.ModelType.IsPrimaryType(KnownPrimaryType.Credentials)))
                {
                    classes.Add("com.microsoft.rest.credentials.ServiceClientCredentials");
                }
                classes.AddRange(new[]{
                        "com.microsoft.rest.ServiceClient",
                        "com.microsoft.rest.RestClient",
                        "okhttp3.OkHttpClient",
                        "retrofit2.Retrofit"
                    });

                classes.AddRange(RootMethods
                    .SelectMany(m => m.ImplImports)
                    .OrderBy(i => i));

                return classes.AsEnumerable();
            }
        }

        [JsonIgnore]
        public virtual List<string> InterfaceImports
        {
            get
            {
                HashSet<string> classes = new HashSet<string>();
                
                classes.AddRange(RootMethods
                    .SelectMany(m => m.InterfaceImports)
                    .OrderBy(i => i).Distinct());

                classes.Add("com.microsoft.rest.RestClient");

                return classes.ToList();
            }
        }
    }
}