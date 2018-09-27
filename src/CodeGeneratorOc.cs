// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoRest.Core;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.Core.Utilities.Collections;
using AutoRest.ObjC.Model;
using AutoRest.ObjectiveC.Builder;
using static AutoRest.Core.Utilities.DependencyInjection;
using AutoRest.ObjC.Templates;

namespace AutoRest.ObjC
{
    public class CodeGeneratorOc : CodeGenerator
    {
        public CodeGeneratorOc()
        {
        }

        private const string HeaderFileExtenson = ".h";
        private const string ModuleFileExtenson = ".m";
        private const string ModelsFolderName = "Models";
        private const string ProtocolsFolderName = "Protocols";
        private const string OperationsFolderName = "Operations";
        private string _baseFolderName = "UNDEFINED";


        public override string ImplementationFileExtension => ".json";

        public override string UsageInstructions => $"Your Azure Resource Schema(s) can be found in the specified `output-folder`.";

        public override async Task Generate(CodeModel cm)
        {
            var codeModel = cm as CodeModelObjC;
            if (codeModel == null)
            {
                throw new InvalidCastException("CodeModel is not a Java CodeModel");
            }

            _baseFolderName = cm.Name;

            var sc = ServiceClient.Define(cm.Name)
                .WithBaseUrl(cm.BaseUrl)
                .WithKey("df");


            var modelList = new List<string>();
            var modelsFolderPath = Path.Combine(_baseFolderName, ModelsFolderName);
/*
            //Models
            foreach (var compositeType in cm.ModelTypes.Union(codeModel.HeaderTypes))
            {
                var modelType = (CompositeTypeObjC) compositeType;
                
                var interfaceModelTemplate = new ModelTemplateInterface { Model = modelType };
                await Write(interfaceModelTemplate, Path.Combine(modelsFolderPath, $"{modelType.Name.ToPascalCase()}{HeaderFileExtenson}"));
                
                var implModelTemplate = new ModelTemplateImpl { Model = modelType };
                await Write(implModelTemplate, Path.Combine(modelsFolderPath, $"{modelType.Name.ToPascalCase()}{ModuleFileExtenson}"));
            }

            // Enums
            foreach (var enumType in cm.EnumTypes)
            {
                var enumTypeObjC = (EnumTypeObjC) enumType;
                
                var interfaceEnumTemplate = new EnumTemplateInterface { Model = enumTypeObjC };
                await Write(interfaceEnumTemplate, Path.Combine(modelsFolderPath, $"{interfaceEnumTemplate.Model.Name.ToPascalCase()}{HeaderFileExtenson}"));
                
                var implEnumTemplate = new EnumTemplateImpl { Model = enumTypeObjC };
                await Write(implEnumTemplate, Path.Combine(modelsFolderPath, $"{implEnumTemplate.Model.Name.ToPascalCase()}{ModuleFileExtenson}"));
            }
*/

            var protocolsFolderPath = Path.Combine(_baseFolderName, ProtocolsFolderName);

            // Service client
            var serviceClientTemplate = new ServiceClientTemplate { Model = codeModel };
            await Write(serviceClientTemplate,  Path.Combine(protocolsFolderPath, $"{codeModel.Name.ToPascalCase()}Impl{ModuleFileExtenson}"));

            // Service client interface
            var serviceClientInterfaceTemplate = new ServiceClientInterfaceTemplate { Model = codeModel };
            await Write(serviceClientInterfaceTemplate,  Path.Combine(protocolsFolderPath, $"{cm.Name.ToPascalCase()}{HeaderFileExtenson}"));


            // operations
//            var op = cm.Operations;
//            foreach (MethodGroupObjC methodGroup in codeModel.AllOperations)
//            {
//                // Operation
//                var operationsTemplate = new MethodGroupTemplate { Model = methodGroup };
//                await Write(operationsTemplate, Path.Combine(protocolsFolderPath, $"{methodGroup.TypeName.ToPascalCase()}Impl{ModuleFileExtenson}"));
//
//                // Operation protocol
//                var operationsInterfaceTemplate = new MethodGroupInterfaceTemplate { Model = methodGroup };
//                await Write(operationsInterfaceTemplate, Path.Combine(protocolsFolderPath, $"{methodGroup.TypeName.ToPascalCase()}{HeaderFileExtenson}"));
//            }


            var enumlList = new List<string>();
            foreach (var enumType in cm.EnumTypes)
            {
                enumlList.Add(enumType.Name);
            }


//                    .WithOperation("op1")
//                        .WithParameter("op1-param1")
//                            .IsRequired(true)
//                            .OfType("string")
//                            .WithDefaultValue("some default value")
//                            .Attach()
//                        .WithParameter("op1-param2")
//                            .WithDefaultValue("foo")
//                            .Attach()
//                        .Attach();



            await this.Write("Hello World ANd shch", "ShchFile.m");
        }
    }
}
