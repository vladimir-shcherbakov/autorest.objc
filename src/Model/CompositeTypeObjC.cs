using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using AutoRest.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoRest.ObjC.Model
{
    public class CompositeTypeObjC : CompositeType, IModelTypeObjC
    {
        public const string ExternalExtension = "x-ms-external";
        protected string _runtimePackage = "com.microsoft.rest";

        public CompositeTypeObjC()
        {
        }

        public CompositeTypeObjC(string name) : base(name)
        {
        }

        [JsonIgnore]
        public virtual string ModelsPackage => (this.CodeModel as CodeModelObjC)?.ModelsPackage;

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
                    return string.Join(
                        ".",
                        CodeModel?.Namespace.ToLowerInvariant(),
                        "models");
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
        public virtual string ExceptionTypeDefinitionName
        {
            get
            {
                if (this.Extensions.ContainsKey(SwaggerExtensions.NameOverrideExtension))
                {
                    if (this.Extensions[SwaggerExtensions.NameOverrideExtension] is JContainer ext && ext["name"] != null)
                    {
                        return ext["name"].ToString();
                    }
                }
                return this.Name + "Exception";
            }
        }

        [JsonIgnore]
        public bool NeedsFlatten
        {
            get
            {
                return Properties.OfType<PropertyObjC>().Any(p => p.WasFlattened());
            }
        }

        [JsonIgnore]
        public bool SkipParentValidation { get; set; }

        [JsonIgnore]
        public virtual IEnumerable<string> Imports
        {
            get
            {
                var imports = new List<string>();
                if (Name.Contains('<'))
                {
                    imports.AddRange(ParseGenericType().SelectMany(t => t.Imports));
                }
                else
                {
                    imports.Add(string.Join(".", Package, Name));
                }
                return imports;
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> ImportList
        {
            get
            {
                var classes = new HashSet<string>();
                classes.AddRange(Properties.SelectMany(pm => (pm as PropertyObjC)?.Imports));
                if (this.Properties.Any(p => !p.GetJsonProperty().IsNullOrEmpty()))
                {
                    classes.Add("com.fasterxml.jackson.annotation.JsonProperty");
                }
                // For polymorphism
                if (BaseIsPolymorphic)
                {
                    classes.Add("com.fasterxml.jackson.annotation.JsonTypeInfo");
                    classes.Add("com.fasterxml.jackson.annotation.JsonTypeName");
                    if (SubTypes.Any())
                    {
                        classes.Add("com.fasterxml.jackson.annotation.JsonSubTypes");
                    }
                }
                // For flattening
                if (NeedsFlatten)
                {
                    classes.Add("com.microsoft.rest.serializer.JsonFlatten");
                }
                if (SkipParentValidation)
                {
                    classes.Add("com.microsoft.rest.SkipParentValidation");
                }
                return classes.AsEnumerable();
            }
        }

        [JsonIgnore]
        public IModelTypeObjC ResponseVariant => this;

        [JsonIgnore]
        public IModelTypeObjC ParameterVariant => this;

        [JsonIgnore]
        public IModelTypeObjC NonNullableVariant => this;

        [JsonIgnore]
        public string RequiredPropertiesConstructorDeclaration
        {
            get
            {
                if (!GenerateConstructorForRequiredProperties)
                {
                    return "";
                }
                var requiredProps = Properties.Where(p => p.IsRequired && !p.IsConstant);
                var declare = requiredProps.Select(p => $"{p.Name}: ({p.ModelTypeName}) {p.Name}");

//                var declare = requiredProps.Select(p =>
//                {
//                    var type = (p.ModelType is CompositeTypeObjC)
//                        ? $"{p.ModelTypeName} *"
//                        : $"{p.ModelTypeName}";
//                    return $"{p.Name}: ({type}) {p.Name}";
//                });

                var res = string.Join(" ", declare);
                return char.ToUpper(res[0]) + res.Substring(1);
            }
        }

        [JsonIgnore] public bool GenerateConstructorForRequiredProperties => Properties.Any(p => p.IsRequired); //&& true == AutoRest.Core.Settings.Instance.Host?.GetValue<bool?>("generate-constructor").Result;

        protected IEnumerable<IModelTypeObjC> ParseGenericType()
        {
            string name = Name;
            string[] types = Name.ToString().Split(new string[] { "<", ">", ",", ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var innerType in types.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                if (!CodeNamerObjC.PrimaryTypes.Contains(innerType.Trim()))
                {
                    yield return new CompositeTypeObjC { Name = innerType.Trim(), CodeModel = CodeModel };
                }
            }
        }
    }
}
