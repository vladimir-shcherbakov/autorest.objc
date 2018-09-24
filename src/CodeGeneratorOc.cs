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
            if (!(cm is CodeModelObjC codeModel))
            {
                throw new InvalidCastException("CodeModel is not a Java CodeModel");
            }

            _baseFolderName = cm.Name;

            var sc = ServiceClient.Define(cm.Name)
                .WithBaseUrl(cm.BaseUrl)
                .WithKey("df");


            var modelList = new List<string>();
            var modelsFolderPath = Path.Combine(_baseFolderName, ModelsFolderName);

            //Models
            foreach (var compositeType in cm.ModelTypes.Union(codeModel.HeaderTypes))
            {
                var modelType = (CompositeTypeObjC) compositeType;
                var modelTemplate = new InterfaceModelTemplate { Model = modelType };
                await Write(modelTemplate, Path.Combine(modelsFolderPath, $"{modelType.Name.ToPascalCase()}.{HeaderFileExtenson}"));
            }

//
//            foreach (var compositeType in cm.ModelTypes.Union(cm.HeaderTypes))
//            {
//                var modelType = (CompositeTypeObjC) compositeType;
//                modelList.Add(modelType.Name);
//                var modelTemplate = new InterfaceModelTemplate { Model = modelType };
////                var headerContent = "";
////                var moduleContent = "";
////
//                await Write(modelTemplate, Path.Combine(modelsFolderPath, $"{modelType.Name}.{HeaderFileExtenson}"));
////                await Write(moduleContent, Path.Combine(modelsFolderPath, $"{modelType.Name}.{ModuleFileExtenson}"));
//
//
//
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
