// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using AutoRest.Core.Model;
using Newtonsoft.Json;

namespace AutoRest.ObjectiveC.Model
{
    public class CodeModelOc : CodeModel
    {
        public override string BaseUrl
        {
            get => !base.BaseUrl.Contains("://") ? $"https://{base.BaseUrl}" : base.BaseUrl;
            set => base.BaseUrl = value;
        }

        [JsonIgnore]
        public IEnumerable<MethodGroupOc> AllOperations => Operations.Where(operation => !operation.Name.IsNullOrEmpty()).Cast<MethodGroupOc>();

        [JsonIgnore]
        public bool IsCustomBaseUri => Extensions.ContainsKey(SwaggerExtensions.ParameterizedHostExtension);

        [JsonIgnore]
        public string ServiceClientServiceType => CodeNamerOc.GetServiceName(Name.ToPascalCase());

        [JsonIgnore]
        public virtual string ImplPackage => "implementation";

        [JsonIgnore]
        public string ModelsPackage => "Models/";

        [JsonIgnore]
        public IEnumerable<MethodOc> RootMethods => Methods.Where(m => m.Group.IsNullOrEmpty()).OfType<MethodOc>();

        [JsonIgnore]
        public string FullyQualifiedDomainName => Namespace.ToLowerInvariant() + "." + this.Name;

        [JsonIgnore]
        public virtual IEnumerable<string> ImplImports
        {
            get
            {
                var imports = new HashSet<string> {FullyQualifiedDomainName};
                foreach(var methodGroupFullType in this.AllOperations.Select(op => op.MethodGroupFullType).Distinct())
                {
                    imports.Add(methodGroupFullType);
                }
                if (this.Properties.Any(p => p.ModelType.IsPrimaryType(KnownPrimaryType.Credentials)))
                {
                    //imports.Add("com.microsoft.rest.credentials.ServiceClientCredentials");
                }
//                classes.AddRange(new[]{
//                        "com.microsoft.rest.ServiceClient",
//                        "com.microsoft.rest.RestClient",
//                        "okhttp3.OkHttpClient",
//                        "retrofit2.Retrofit"
//                    });

                imports.AddRange(RootMethods
                    .SelectMany(m => m.ImplImports)
                    .OrderBy(i => i));

                return imports.AsEnumerable();
            }
        }

        [JsonIgnore]
        public virtual List<string> InterfaceImports
        {
            get
            {
                var imports = new HashSet<string>();
                
                imports.AddRange(RootMethods
                    .SelectMany(m => m.InterfaceImports)
                    .OrderBy(i => i).Distinct());

//                classes.Add("com.microsoft.rest.RestClient");

                return imports.ToList();
            }
        }
    }
}