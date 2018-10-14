// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoRest.Core;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using AutoRest.Core.Model;
using System;
using AutoRest.ObjectiveC.Model;
using AutoRest.ObjectiveC.Templates;

namespace AutoRest.ObjectiveC
{
    public class CodeGeneratorOc : CodeGenerator
    {
//        private const string ClientRuntimePackage = "com.microsoft.rest:client-runtime:1.0.0-beta6-SNAPSHOT from snapshot repo https://oss.sonatype.org/content/repositories/snapshots/";
//        private const string _packageInfoFileName = "package-info.java";

        public CodeNamerOc Namer { get; private set; }

        public override string UsageInstructions => $"Thedependency is required to execute the generated code.";

        public const string InterfaceFileExtension = ".h";
        public override string ImplementationFileExtension => ".m";

        /// <summary>
        /// Generate Java client code for given ServiceClient.
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        public override async Task Generate(CodeModel cm)
        {
            var packagePath = $"./{cm.Namespace.ToLower().Replace('.', '/')}";

            // get ObjectiveC specific codeModel
            if (!(cm is CodeModelOc codeModel))
            {
                throw new InvalidCastException("CodeModel is not a ObjectiveC CodeModel");
            }

            // Service client
            var serviceClientTemplate = new ServiceClientTemplate { Model = codeModel };
            await Write(serviceClientTemplate, $"{packagePath}/{codeModel.Name.ToPascalCase()}{ImplementationFileExtension}");

            // Service client interface
            var serviceClientInterfaceTemplate = new ServiceClientInterfaceTemplate { Model = codeModel };
            await Write(serviceClientInterfaceTemplate, $"{packagePath}/{cm.Name.ToPascalCase()}{InterfaceFileExtension}");

            // UnitTests
            var unitTestsTemplate = new UnitTestsTemplate { Model = codeModel };
            await Write(unitTestsTemplate, $"{packagePath}/{cm.Name.ToPascalCase()}Tests{ImplementationFileExtension}");

            // root methods - to generate executors - backlog
//            foreach (var method in codeModel.RootMethods)
//            {
//                // method interface
//                var operationsInterfaceTemplate = new RootMethodInterfaceTemplate { Model = method };
//                await Write(operationsInterfaceTemplate, $"{packagePath}/methods/{method.Name.ToPascalCase()}{InterfaceFileExtension}");
//
//            }

            // operations
            foreach (var methodGroup in codeModel.AllOperations)
            {
                // Operation
                var operationsTemplate = new MethodGroupTemplate { Model = methodGroup };
                await Write(operationsTemplate, $"{packagePath}/Operations/{methodGroup.TypeName.ToPascalCase()}{ImplementationFileExtension}");

                // Operation interface
                var operationsInterfaceTemplate = new MethodGroupInterfaceTemplate { Model = methodGroup };
                await Write(operationsInterfaceTemplate, $"{packagePath}/Operations/{methodGroup.TypeName.ToPascalCase()}{InterfaceFileExtension}");
            }

            //Models
            foreach (CompositeTypeOc modelType in cm.ModelTypes.Union(codeModel.HeaderTypes))
            {
                //var cp = modelType.ComposedProperties; // combined whith base type props if any
                var modelTemplate = new ModelTemplate { Model = modelType };
                await Write(modelTemplate, $"{packagePath}/Models/{modelType.Name.ToPascalCase()}{InterfaceFileExtension}");
                var modelTemplateImpl = new ModelTemplateImpl { Model = modelType };
                await Write(modelTemplateImpl, $"{packagePath}/Models/{modelType.Name.ToPascalCase()}{ImplementationFileExtension}");
            }

            // Enums
            foreach (EnumTypeOc enumType in cm.EnumTypes)
            {
                var enumTemplate = new EnumTemplate { Model = enumType };
                await Write(enumTemplate, $"{packagePath}/Models/{enumTemplate.Model.Name.ToPascalCase()}{InterfaceFileExtension}");
                var enumTemplateImpl = new EnumTemplateImpl { Model = enumType };
                await Write(enumTemplateImpl, $"{packagePath}/Models/{enumTemplate.Model.Name.ToPascalCase()}{ImplementationFileExtension}");
            }

            // Exceptions
//            foreach (CompositeTypeOc exceptionType in codeModel.ErrorTypes)
//            {
//                var exceptionTemplate = new ExceptionTemplate { Model = exceptionType };
//                await Write(exceptionTemplate, $"{packagePath}/models/{exceptionTemplate.Model.ExceptionTypeDefinitionName}{enumTemplateImpl}");
//            }

            // package-info
//            await Write(new PackageInfoTemplate
//            {
//                Model = new PackageInfoTemplateModel(cm)
//            }, $"{packagePath}/{_packageInfoFileName}");
//            await Write(new PackageInfoTemplate
//            {
//                Model = new PackageInfoTemplateModel(cm, "implementation")
//            }, $"{packagePath}/implementation/{_packageInfoFileName}");
//            await Write(new PackageInfoTemplate
//            {
//                Model = new PackageInfoTemplateModel(cm, "models")
//            }, $"{packagePath}/models/{_packageInfoFileName}");
        }
    }
}
