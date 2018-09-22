// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using AutoRest.ObjC.Writer;
using System;
using System.Collections.Generic;
using System.Linq;
using static AutoRest.Core.Utilities.DependencyInjection;

namespace AutoRest.ObjC.Model
{
    /// <summary>
    /// Defines a synthesized composite type that wraps a primary type, array, or dictionary method response.
    /// </summary>
    public class CompositeTypeObjC : CompositeType, IVariableType
    {
        public static string TestNamespace { get; set; }

        private bool _wrapper;

        // True if the type is returned by a method
        public bool IsResponseType;

        // Name of the field containing the URL used to retrieve the next result set
        // (null or empty if the model is not paged).
        public string NextLink;

        public bool PreparerNeeded = false;

        public bool HasCodingKeys { get; set; }

        internal CompositeFieldWriter FieldWriter { get; private set; }

        public IEnumerable<CompositeType> DerivedTypes => CodeModel.ModelTypes.Where(t => t.DerivesFrom(this));

        public IEnumerable<CompositeType> SiblingTypes
        {
            get
            {
                var st = (BaseModelType as CompositeTypeObjC).DerivedTypes;
                if (BaseModelType.BaseModelType != null && BaseModelType.BaseModelType.IsPolymorphic)
                {
                    st = st.Union((BaseModelType as CompositeTypeObjC).SiblingTypes);
                }
                return st;
            }
        }

        public string InterfaceOutput
        {
            get
            {
                CompositeTypeObjC baseType = (CompositeTypeObjC)this.BaseModelType;
                if(baseType != null)
                {
                    return $"{this.Name}Protocol, {baseType.InterfaceOutput}";
                }

                return $"{this.Name}Protocol";
            }
        }

        public string BaseTypeInterfaceForProtocol
        {
            get
            {
                CompositeTypeObjC baseType = (CompositeTypeObjC)this.BaseModelType;
                if(baseType != null)
                {
                    return $"{baseType.Name}Protocol";
                }

                return $"Codable";
            }
        }

        public bool HasPolymorphicFields
        {
            get
            {
                return AllProperties.Any(p => 
                        
                        (p.ModelType is CompositeType       // polymorphic composite
                            && ((CompositeTypeObjC) p.ModelType).IsPolymorphic) 
                         || (p.ModelType is SequenceType    // polymorphic array
                             && (p.ModelType as ArrayTypeObjC).ElementType is CompositeType 
                             && ((p.ModelType as ArrayTypeObjC).ElementType as CompositeType).IsPolymorphic));
            }
        }

        public EnumType DiscriminatorEnum;

        public string DiscriminatorEnumValue => (DiscriminatorEnum as EnumTypeObjC).Constants.FirstOrDefault(c => c.Value.Equals(SerializedName)).Key;

        public CompositeTypeObjC()
        {
            this.FieldWriter = new CompositeFieldWriter(this);
        }

        public CompositeTypeObjC(string name) : base(name)
        {
            this.FieldWriter = new CompositeFieldWriter(this);
        }

        /// <summary>
        /// If PolymorphicDiscriminator is set, makes sure we have a PolymorphicDiscriminator property.
        /// </summary>
        public void AddPolymorphicPropertyIfNecessary()
        {
            if (!string.IsNullOrEmpty(PolymorphicDiscriminator) && Properties.All(p => p.SerializedName != PolymorphicDiscriminator))
            {
                base.Add(New<Property>(new
                {
                    Name = CodeNamerObjC.Instance.GetPropertyName(PolymorphicDiscriminator),
                    SerializedName = PolymorphicDiscriminator,
                    ModelType = DiscriminatorEnum,
                }));
            }            
        }

        public string PolymorphicProperty
        {
            get
            {
                if (!string.IsNullOrEmpty(PolymorphicDiscriminator))
                {
                    return CodeNamerObjC.Instance.GetPropertyName(PolymorphicDiscriminator);
                }
                if (BaseModelType != null)
                {
                    return (BaseModelType as CompositeTypeObjC).PolymorphicProperty;
                }
                return null;
            }
        }

        public IEnumerable<PropertyObjC> AllProperties
        {
            get
            {
                if (BaseModelType != null)
                {
                    return Properties.Cast<PropertyObjC>().Concat((BaseModelType as CompositeTypeObjC).AllProperties);
                }
                return Properties.Cast<PropertyObjC>();
            }
        }

        public override Property Add(Property item)
        {
            var property = base.Add(item) as PropertyObjC;
            return property;
        }

        /// <summary>
        /// Add imports for composite types.
        /// </summary>
        /// <param name="imports"></param>
        public void AddImports(HashSet<string> imports)
        {
            Properties.ForEach(p => p.ModelType.AddImports(imports));
        }

        public bool IsPolymorphicResponse() {
            if (BaseIsPolymorphic && BaseModelType != null)
            {
                return (BaseModelType as CompositeTypeObjC).IsPolymorphicResponse();
            }
            return IsPolymorphic && IsResponseType;
        }

        public IModelType BaseType { get; private set; }

        public IModelType GetElementType(IModelType type)
        {
            if (type is ArrayTypeObjC)
            {
                Name += "List";
                return GetElementType((type as SequenceType).ElementType);
            }
            else if (type is DictionaryTypeObjC)
            {
                Name += "Set";
                return GetElementType(((type as DictionaryTypeObjC).ValueType));
            }
            else
            {
                return type;
            }
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public bool IsRequired { get; set; }

        public string VariableTypeDeclaration(bool isRequired)
        {
            return ObjCNameHelper.GetTypeName(this.Name + "Protocol", isRequired);
        }

        public string EncodeTypeDeclaration(bool isRequired)
        {
                return ObjCNameHelper.GetTypeName(this.TypeName, isRequired);
        }

        public string DecodeTypeDeclaration(bool isRequired)
        {
            return ObjCNameHelper.GetTypeName(this.TypeName, isRequired);
        }

        public string VariableName
        {
            get
            {
                return ObjCNameHelper.ConvertToVariableName(this.Name);
            }
        }

        public string TypeName {
            get {
                return this.Name + "Data";
            }
        }

        public bool HasRequiredFields {
            get {
                if (BaseModelType != null)
                {
                    if(((CompositeTypeObjC)BaseModelType).HasRequiredFields) {
                        return true;
                    }
                }

                return this.Properties.Where(x => x.IsRequired).Count() > 0;
            }
        }

        public void SetDecodeDate(String propName, PropertyObjC property, IndentedStringBuilder indented) {
            var modelType = property.ModelType as PrimaryTypeObjC;
            var formatString = String.Empty;
            if(modelType != null) {
                switch (modelType.KnownPrimaryType)
                {
                    case KnownPrimaryType.Date:
                        formatString = "date";
                        break;
                    case KnownPrimaryType.DateTime:
                        formatString = "dateTime";
                        break;
                    case KnownPrimaryType.DateTimeRfc1123:
                        formatString = "dateTimeRfc1123";
                        break;
                    default:
                        throw new Exception("Date format unknown");
                }

                indented.Append($"    self.{propName} = DateConverter.fromString(dateStr: (try container.decode(String?.self, forKey: .{propName})), format: .{formatString})" + 
                    (property.IsRequired ? "!" : "") + "\r\n");
                return;
            }

            throw new Exception("Date format unknown");
        }

        public void SetEncodeDate(string propName, PropertyObjC property, IndentedStringBuilder indented) {
            var modelType = property.ModelType as PrimaryTypeObjC;
            if(modelType != null) {

                var formatString = string.Empty;

                switch (modelType.KnownPrimaryType)
                {
                    case KnownPrimaryType.Date:
                        formatString = "date";
                        break;
                    case KnownPrimaryType.DateTime:
                        formatString = "dateTime";
                        break;
                    case KnownPrimaryType.DateTimeRfc1123:
                        formatString = "dateTimeRfc1123";
                        break;
                    default:
                        throw new Exception("Date format unknown");
                }

                if (property.IsRequired)
                {
                    indented.Append($"try container.encode(DateConverter.toString(date: self.{propName}, format: .{formatString}), forKey: .{propName})\r\n");
                }
                else
                {
                    indented.Append($"if self.{propName} != nil {{\r\n");
                    indented.Append($"    try container.encode(DateConverter.toString(date: self.{propName}!, format: .{formatString}), forKey: .{propName})\r\n");
                    indented.Append($"}}\r\n");
                }

                return;
            }

            throw new Exception("Date format unknown");
        }
    }
}
