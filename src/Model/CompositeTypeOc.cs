﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoRest.Core;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using AutoRest.Extensions;
using Newtonsoft.Json;

namespace AutoRest.ObjectiveC.Model
{
    public class CompositeTypeOc : CompositeType, IModelTypeOc
    {
        public const string ExternalExtension = "x-ms-external";
        protected string _runtimePackage = "Models";

        public CompositeTypeOc()
        {
            Name.OnGet += name => Settings.Instance?.Namespace + CodeNamer.Instance.GetTypeName($"{name}");
        }

    //    public CompositeTypeOc(string name1) : base(name1)
    //    {
    //        Name.OnGet += name => Settings.Instance?.Namespace + CodeNamer.Instance.GetTypeName($"{name}");
    //    }

        [JsonIgnore]
        public virtual string ModelsPackage => (this.CodeModel as CodeModelOc).ModelsPackage;

        [JsonIgnore]
        public virtual string Package
        {
            get
            {
                if (Extensions.Get<bool>(ExternalExtension) == true) {
                    return _runtimePackage;
                }
                else
                {
//                    return string.Join(
//                        ".",
//                        CodeModel?.Namespace.ToLowerInvariant(),
//                        "models");
                    return @"Models";
                }
            }
        }

        [JsonIgnore]
        public IEnumerable<CompositeType> SubTypes
        {
            get
            {
                if (BaseIsPolymorphic)
                {
                    foreach (var type in CodeModel.ModelTypes)
                    {
                        if (type.BaseModelType != null &&
                            type.BaseModelType.SerializedName == this.SerializedName)
                        {
                            yield return type;
                        }
                    }
                }
            }
        }

        [JsonIgnore]
        public virtual string ErrorTypeName
        {
            get
            {
                if (this.Extensions.ContainsKey(SwaggerExtensions.NameOverrideExtension))
                {
                    var ext = this.Extensions[SwaggerExtensions.NameOverrideExtension] as Newtonsoft.Json.Linq.JContainer;
                    if (ext?["name"] != null)
                    {
                        return ext["name"].ToString();
                    }
                }
                return this.Name;
            }
        }

        [JsonIgnore]
        public bool NeedsFlatten
        {
            get
            {
                return Properties.OfType<PropertyOc>().Any(p => p.WasFlattened());
            }
        }

        [JsonIgnore]
        public bool SkipParentValidation { get; set; }

        [JsonIgnore]
        public virtual IEnumerable<string> Imports
        {
            get
            {
//                var imports = new List<string>();
//                if (Name.Contains('<'))
//                {
//                    imports.AddRange(ParseGenericType().SelectMany(t => t.Imports));
//                }
//                else
//                {
//                    imports.Add(string.Join("/", Package, Name));
//                }
                yield return Name;
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> ImportList
        {
            get
            {
                var imports = new HashSet<string>();
                imports.AddRange(Properties.SelectMany(pm => (pm as PropertyOc).Imports));
//                if (this.Properties.Any(p => !p.GetJsonProperty().IsNullOrEmpty()))
//                {
//                    imports.Add("com.fasterxml.jackson.annotation.JsonProperty");
//                }
//                // For polymorphism
//                if (BaseIsPolymorphic)
//                {
//                    imports.Add("com.fasterxml.jackson.annotation.JsonTypeInfo");
//                    imports.Add("com.fasterxml.jackson.annotation.JsonTypeName");
//                    if (SubTypes.Any())
//                    {
//                        imports.Add("com.fasterxml.jackson.annotation.JsonSubTypes");
//                    }
//                }
//                // For flattening
//                if (NeedsFlatten)
//                {
//                    imports.Add("com.microsoft.rest.serializer.JsonFlatten");
//                }
//                if (SkipParentValidation)
//                {
//                    imports.Add("com.microsoft.rest.SkipParentValidation");
//                }
                return imports.AsEnumerable();
            }
        }

        [JsonIgnore]
        public IModelTypeOc ResponseVariant => this;

        [JsonIgnore]
        public IModelTypeOc ParameterVariant => this;

        [JsonIgnore]
        public IModelTypeOc NonNullableVariant => this;

        public string NameForMethod => $"{Name}*";

        [JsonIgnore]
        public string RequiredPropertiesConstructorDeclaration
        {
            get
            {
                // if (!GenerateConstructorForRequiredProperties)
                // {
                //     return "";
                // }
                var parametersDelaraton = Properties
                    .Where(p => !p.IsConstant)
                    .Where(p => p.IsRequired || p.IsReadOnly)
                    .Select(p => $"with{p.Name.ToPascalCase()}:({(p.ModelType as IModelTypeOc)?.Name} *){p.Name}");                
                return string.Join(" ", parametersDelaraton).StartWithUppercase();
            }
        }

        [JsonIgnore]
        // //public bool GenerateConstructorForRequiredProperties => Properties.Any(p => p.IsRequired) && true == AutoRest.Core.Settings.Instance.Host?.GetValue<bool?>("generate-constructor").Result;
        public bool GenerateConstructorForRequiredProperties => Properties.Any(p => p.IsRequired || p.IsReadOnly);

        protected IEnumerable<IModelTypeOc> ParseGenericType()
        {
            string name = Name;
            string[] types = Name.ToString().Split(new string[] { "<", ">", ",", ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var innerType in types.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                if (!CodeNamerOc.PrimaryTypes.Contains(innerType.Trim()))
                {
                    yield return new CompositeTypeOc { Name = innerType.Trim(), CodeModel = CodeModel };
                }
            }
        }
    }
}
