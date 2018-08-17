// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// 

namespace AutoRest.ObjectiveC {
    using Core;
    using Core.Extensibility;
    using Core.Model;

    public sealed class PluginOc : Plugin<IGeneratorSettings, CodeModelTransformer<CodeModel>, CodeGeneratorOc, CodeNamer, CodeModel> {
    }
}