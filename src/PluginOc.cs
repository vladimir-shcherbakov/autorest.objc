// Copyright (c) Microsoft Corporation. All rights reserved.TransformerSwift
// Licensed under the MIT License. See License.txt in the project root for license information.
// 

using AutoRest.Core.Utilities;
using AutoRest.ObjC.Model;

namespace AutoRest.ObjC {
    using Core;
    using Core.Extensibility;
    using Core.Model;

    //    public sealed class PluginOc : Plugin<IGeneratorSettings, CodeModelTransformer<CodeModel>, CodeGeneratorOc, CodeNamer, CodeModel> {
    //    }
    public sealed class PluginOc : Plugin<IGeneratorSettings, TransformerOc, CodeGeneratorOc, CodeNamerObjC, CodeModelObjC>
    {
        public PluginOc()
        {
            Context = new DependencyInjection.Context
            {
                // inherit base settings
                Context,

                // set code model implementations our own implementations 
                new Factory<CodeModel, CodeModelObjC>(),
                new Factory<Method, MethodObjC>(),
                new Factory<CompositeType, CompositeTypeObjC>(),
                new Factory<Property, PropertyObjC>(),
                new Factory<Parameter, ParameterObjC>(),
                new Factory<DictionaryType, DictionaryTypeObjC>(),
                new Factory<SequenceType, SequenceTypeObjC>(),
                new Factory<MethodGroup, MethodGroupObjC>(),
                new Factory<EnumType, EnumTypeObjC>(),
                new Factory<PrimaryType, PrimaryTypeObjC>(),
                new Factory<Response, ResponseObjC>()
            };
        }
    }
}