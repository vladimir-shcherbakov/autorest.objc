// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// 

using AutoRest.Core;
using AutoRest.Core.Extensibility;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.ObjectiveC.Model;
using static AutoRest.Core.Utilities.DependencyInjection;

namespace AutoRest.ObjectiveC
{
    public sealed class PluginOc : Plugin<IGeneratorSettings, TransformerOc, CodeGeneratorOc, CodeNamerOc, CodeModelOc>
    {
        public PluginOc()
        {
            Context = new Context
            {
                // inherit base settings
                Context,

                // set code model implementations our own implementations 
                new Factory<CodeModel, CodeModelOc>(),
                new Factory<Method, MethodOc>(),
                new Factory<CompositeType, CompositeTypeOc>(),
                new Factory<Property, PropertyOc>(),
                new Factory<Parameter, ParameterOc>(),
                new Factory<DictionaryType, DictionaryTypeOc>(),
                new Factory<SequenceType, SequenceTypeOc>(),
                new Factory<MethodGroup, MethodGroupOc>(),
                new Factory<EnumType, EnumTypeOc>(),
                new Factory<PrimaryType, PrimaryTypeOc>(),
                new Factory<Response, ResponseOc>()
            };
        }
    }
}