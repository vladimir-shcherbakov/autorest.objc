using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static AutoRest.Core.Utilities.DependencyInjection;
using AutoRest.ObjC.Model;

namespace AutoRest.ObjC.Writer
{
    internal class CompositeFieldWriter
    {
        private readonly CompositeTypeObjC _compositeType;

        public CompositeFieldWriter(CompositeTypeObjC compositeType)
        {
            this._compositeType = compositeType;
        }

        public IEnumerableWithIndex<Property> Properties
        {
            get
            {
                this._compositeType.AddPolymorphicPropertyIfNecessary();
                return this._compositeType.Properties;
            }
        }

        public CompositeType BaseModelType => this._compositeType.BaseModelType;

        public string FieldsAsString(bool forInterface = false)
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = Properties.Cast<PropertyObjC>().ToList();
            if (BaseModelType != null && !forInterface)
            {
                indented.Append(((CompositeTypeObjC)BaseModelType).FieldWriter.FieldsAsString(forInterface));
            }

            // Emit each property, except for named Enumerated types, as a pointer to the type
            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                var modelType = property.ModelType;
                if (property.IsPolymorphicDiscriminator)
                {
                    continue;
                }

                if (modelType is PrimaryTypeObjC c)
                {
                    c.IsRequired = property.IsRequired;
                }

                var modelDeclaration = modelType.Name;
                switch (modelType)
                {
                    case PrimaryTypeObjC _:
                        modelDeclaration = ObjCNameHelper.GetTypeName(modelType.Name, property.IsRequired);
                        break;
                    case IVariableType _:
                        modelDeclaration = ((IVariableType)modelType).VariableTypeDeclaration(property.IsRequired);
                        break;
                }

                var output = string.Empty;
                var propName = property.VariableName;
                var modifier = forInterface ? "" : "public";
                //TODO: need to handle flatten property case.
                output = string.Format("{2} var {0}: {1}",
                    propName,
                    modelDeclaration,
                    modifier);

                if (forInterface)
                {
                    output += " { get set }\n";
                }
                else
                {
                    output += "\n";
                }

                indented.Append(output);
            }

            return indented.ToString();
        }

        public string FieldEnumValuesForCodable(bool isTopLevel = false)
        {
            var indented = new IndentedStringBuilder("    ");
            if (isTopLevel)
            {
                indented.Append("enum CodingKeys: String, CodingKey {");
            }
            var properties = Properties.Cast<PropertyObjC>().ToList();
            if (BaseModelType != null)
            {
                var baseEnumValues = ((CompositeTypeObjC)BaseModelType).FieldWriter.FieldEnumValuesForCodable();
                if (baseEnumValues.Length > 0)
                {
                    this._compositeType.HasCodingKeys = true;
                    indented.Append(baseEnumValues);
                }
            }

            // Emit each property, except for named Enumerated types, as a pointer to the type
            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                var propName = property.VariableName;
                var serializeName = property.SerializedName;
                this._compositeType.HasCodingKeys = true;
                indented.Append($"case {propName} = \"{serializeName}\"\r\n");
            }

            if (!this._compositeType.HasCodingKeys)
            {
                return "";
            }

            if (isTopLevel)
            {
                indented.Append("}");
            }

            return indented.ToString();
        }

        public string FieldEncodingString()
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = Properties.Cast<PropertyObjC>().ToList();
            if (BaseModelType != null)
            {
                indented.Append(((CompositeTypeObjC)BaseModelType).FieldWriter.FieldEncodingString());
            }

            // Emit each property, except for named Enumerated types, as a pointer to the type
            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                if (property.IsPolymorphicDiscriminator)
                {
                    continue;
                }

                var propName = property.VariableName;
                var modelType = property.ModelType;
                if (modelType is PrimaryTypeObjC c)
                {
                    c.IsRequired = property.IsRequired;
                }

                switch (modelType)
                {
                    case IVariableType type when !(type is DictionaryType) && !string.IsNullOrEmpty(type.DecodeTypeDeclaration(property.IsRequired)):
                        indented.Append(property.IsRequired
                            ? $"try container.encode(self.{propName} as! {type.DecodeTypeDeclaration(property.IsRequired)}, forKey: .{propName})\r\n"
                            : $"if self.{propName} != nil {{try container.encode(self.{propName} as! {type.DecodeTypeDeclaration(property.IsRequired)}, forKey: .{propName})}}\r\n");
                        break;
                    case PrimaryTypeObjC _ when "Date".Equals(modelType.Name):
                        this._compositeType.SetEncodeDate(propName, property, indented);
                        break;
                    default:
                        indented.Append(property.IsRequired
                            ? $"try container.encode(self.{propName}, forKey: .{propName})\r\n"
                            : $"if self.{propName} != nil {{try container.encode(self.{propName}, forKey: .{propName})}}\r\n");

                        break;
                }
            }

            return indented.ToString();
        }

        public string FieldDecodingString()
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = Properties.Cast<PropertyObjC>().ToList();
            if (BaseModelType != null)
            {
                indented.Append(((CompositeTypeObjC)BaseModelType).FieldWriter.FieldDecodingString());
            }

            // Emit each property, except for named Enumerated types, as a pointer to the type
            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                if (property.IsPolymorphicDiscriminator)
                {
                    continue;
                }

                var propName = property.VariableName;
                var modelType = property.ModelType;
                if (modelType is PrimaryTypeObjC c)
                {
                    c.IsRequired = property.IsRequired;
                }

                var modelDeclaration = modelType.Name;
                if (modelType is PrimaryTypeObjC)
                {
                    modelDeclaration = ObjCNameHelper.GetTypeName(modelType.Name, property.IsRequired);
                }
                else if (modelType is IVariableType type &&
                    !string.IsNullOrEmpty(type.DecodeTypeDeclaration(property.IsRequired)))
                {
                    modelDeclaration = type.DecodeTypeDeclaration(property.IsRequired);
                }

                if (property.IsRequired)
                {
                    if (modelType is PrimaryTypeObjC 
                        && "Date".Equals(modelType.Name))
                    {
                        this._compositeType.SetDecodeDate(propName, property, indented);
                    }
                    else
                    {
                        indented.Append($"self.{propName} = try container.decode({modelDeclaration}.self, forKey: .{propName})\r\n");
                    }
                }
                else
                {
                    indented.Append($"if container.contains(.{propName}) {{\r\n");
                    if (modelType is PrimaryTypeObjC 
                        && "Date".Equals(modelType.Name))
                    {
                        this._compositeType.SetDecodeDate(propName, property, indented);
                    }
                    else
                    {
                        indented.Append($"    self.{propName} = try container.decode({modelDeclaration}.self, forKey: .{propName})\r\n");
                    }
                    indented.Append($"}}\r\n");
                }
            }

            return indented.ToString();
        }

        public string FieldsForTest()
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = Properties.Cast<PropertyObjC>().ToList();
            if (BaseModelType != null)
            {
                indented.Append(((CompositeTypeObjC)BaseModelType).FieldWriter.FieldsForTest());
            }

            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                if (property.IsPolymorphicDiscriminator)
                {
                    continue;
                }

                var propName = property.VariableName;
                var serializeName = property.SerializedName;
                indented.Append($"model.{propName} = nil\r\n");
            }

            return indented.ToString();
        }

        public string FieldsForValidation()
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = Properties.Cast<PropertyObjC>().ToList();
            if (BaseModelType != null)
            {
                indented.Append(((CompositeTypeObjC)BaseModelType).FieldWriter.FieldsForValidation());
            }

            // Emit each property, except for named Enumerated types, as a pointer to the type
            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                if (property.IsPolymorphicDiscriminator)
                {
                    continue;
                }

                var propName = ObjCNameHelper.ConvertToValidObjCTypeName(property.Name.RawValue);
                var modelType = property.ModelType;
                var modelDeclaration = modelType.Name;
                var serializeName = ObjCNameHelper.ConvertToValidObjCTypeName(property.SerializedName);
                if (modelType is IVariableType type 
                    && !string.IsNullOrEmpty(type.DecodeTypeDeclaration(property.IsRequired)))
                {
                }
                else
                {
                }
            }

            return indented.ToString();
        }

        public string RequiredPropertiesForInitParameters(bool forMethodCall = false)
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = Properties.Cast<PropertyObjC>().ToList();
            var seperator = "";
            if (BaseModelType != null)
            {
                indented.Append(((CompositeTypeObjC)BaseModelType).FieldWriter.RequiredPropertiesForInitParameters(forMethodCall));
                if (indented.ToString().Length > 0)
                    seperator = ", ";
            }

            // Emit each property, except for named Enumerated types, as a pointer to the type
            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                if (property.IsPolymorphicDiscriminator)
                {
                    continue;
                }

                var modelType = property.ModelType;
                if (modelType is PrimaryTypeObjC)
                {
                    ((PrimaryTypeObjC)modelType).IsRequired = property.IsRequired;
                }

                var modelDeclaration = modelType.Name;
                switch (modelType)
                {
                    case PrimaryTypeObjC _:
                        modelDeclaration = ObjCNameHelper.GetTypeName(modelType.Name, property.IsRequired);
                        break;
                    case IVariableType type when !string.IsNullOrEmpty(type.VariableTypeDeclaration(property.IsRequired)):
                        modelDeclaration = type.VariableTypeDeclaration(property.IsRequired);
                        break;
                }

                var output = string.Empty;
                var propName = property.VariableName;

                if (!property.IsRequired) continue;

                indented.Append(forMethodCall
                    ? $"{seperator}{propName}: {propName}"
                    : $"{seperator}{propName}: {modelDeclaration}");

                seperator = ", ";
            }

            return indented.ToString();
        }

        public string RequiredPropertiesSettersForInitParameters()
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = Properties.Cast<PropertyObjC>().ToList();
            if (BaseModelType != null)
            {
                indented.Append(((CompositeTypeObjC)BaseModelType).FieldWriter.RequiredPropertiesSettersForInitParameters());
            }

            foreach (var property in properties)
            {
                if (property.ModelType == null)
                {
                    continue;
                }

                if (property.IsPolymorphicDiscriminator)
                {
                    continue;
                }

                var propName = property.VariableName;
                var modelType = property.ModelType;
                if (modelType is PrimaryTypeObjC c)
                {
                    c.IsRequired = property.IsRequired;
                }
/*
                var modelDeclaration = modelType.Name;
                switch (modelType)
                {
                    case PrimaryTypeObjC _:
                        modelDeclaration = ObjCNameHelper.GetTypeName(modelType.Name, property.IsRequired);
                        break;
                    case IVariableType type when !string.IsNullOrEmpty(type.VariableTypeDeclaration(property.IsRequired)):
                        modelDeclaration = type.VariableTypeDeclaration(property.IsRequired);
                        break;
                }
*/
                if (property.IsRequired)
                {
                    indented.Append($"self.{propName} = {propName}\r\n");
                }
            }

            return indented.ToString();
        }
    }
}
