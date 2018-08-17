// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoRest.Core;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities.Collections;
using static AutoRest.Core.Utilities.DependencyInjection;

namespace AutoRest.ObjectiveC
{
    public class CodeGeneratorOc : CodeGenerator
    {
        public CodeGeneratorOc()
        {
        }

        public override string ImplementationFileExtension => ".json";

        public override string UsageInstructions => $"Your Azure Resource Schema(s) can be found in the specified `output-folder`.";

        public override async Task Generate(CodeModel serviceClient)
        {
            this.Write("Hello World", "ShchFile.m");
        }
    }
}
