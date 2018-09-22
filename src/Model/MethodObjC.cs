// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

//using AutoRest.ObjC.Properties;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using AutoRest.Extensions;
using AutoRest.Extensions.Azure;
using AutoRest.Extensions.Azure.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace AutoRest.ObjC.Model
{
    public class MethodObjC : Method
    {
        public string Owner { get; private set; }

        public string PackageName { get; private set; }

        public string APIVersion { get; private set; }

        private readonly string lroDescription = " This method may poll for completion. Polling can be canceled by passing the cancel channel argument. " +
                                                 "The channel will be used to cancel polling and any outstanding HTTP requests.";

        public bool NextAlreadyDefined { get; private set; }

        public bool IsCustomBaseUri
            => CodeModel.Extensions.ContainsKey(SwaggerExtensions.ParameterizedHostExtension);

        public MethodObjC()
        {
            NextAlreadyDefined = true;
        }

        public string CommandModelName
        {
            get { return  this.Name + "Command";  }
        }

        public string CommandProtocolName
        {
            get { return  this.MethodGroup.Name + this.Name;  }
        }

        public string GroupName 
        {
            get { return this.MethodGroup.Name; }
        }

        internal void Transform(CodeModelObjC cmg)
        {
            
            Owner = (MethodGroup as MethodGroupObjC).ClientName;
            PackageName = cmg.Namespace;
            NextAlreadyDefined = NextMethodExists(cmg.Methods.Cast<MethodObjC>());

            var apiVersionParam =
              from p in Parameters
              let name = p.SerializedName
              where name != null && name.IsApiVersion()
              select p.DefaultValue.Value?.Trim(new[] { '"' });

            // When APIVersion is blank, it means that it was unavailable at the method level
            // and we should default back to whatever is present at the client level. However,
            // we will continue embedding that in each method to have broader support.
            APIVersion = apiVersionParam.SingleOrDefault();
            if (APIVersion == default(string))
            {
                APIVersion = cmg.ApiVersion;
            }

            var parameter = Parameters.ToList().Find(p => p.ModelType.PrimaryType(KnownPrimaryType.Stream)
                                                && !(p.Location == ParameterLocation.Body || p.Location == ParameterLocation.FormData));

            if (parameter != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "Go accepts streaming parameters in the Body. The parameter '{0}' streams and is not in the the body.", parameter.Name));
            }
            if (string.IsNullOrEmpty(Description))
            {
                Description = string.Format("sends the {0} request.", Name.ToString().ToPhrase());
            }

            if (IsLongRunningOperation())
            {
                Description += lroDescription;
            }
        }

        public string MethodSignature => $"{Name}({MethodParametersSignature})";
        
        public string ParametersDocumentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                return sb.ToString();
            }
        }

        public PropertyObjC ListElement
        {
            get
            {
                var body = ReturnType.Body as CompositeTypeObjC;
                return body.Properties.Where(p => p.ModelType is ArrayTypeObjC).FirstOrDefault() as PropertyObjC;
            }
        }

        /// <summary>
        /// Generate the method parameter declaration.
        /// </summary>
        public string MethodParametersSignature
        {
            get
            {
                List<string> declarations = new List<string>();
                return string.Join(", ", declarations);
            }
        }

        /// <summary>
        /// Returns true if this method should return its results via channels.
        /// </summary>
        public bool ReturnViaChannel
        {
            get
            {
                // pageable operations will be handled separately
                return IsLongRunningOperation() && !IsPageable;
            }
        }

        /// <summary>
        /// Gets the return type name for this method.
        /// </summary>
        public string MethodReturnType
        {
            get
            {
                return HasReturnValue() ?
                    ((ReturnValue().Body is IVariableType) ? 
                        ((IVariableType)ReturnValue().Body).VariableTypeDeclaration(false)
                            : ObjCNameHelper.GetTypeName(ReturnValue().Body.Name.ToString(), false))
                        : "Void";
            }
        }

        /// <summary>
        /// Gets the return type name for this method.
        /// </summary>
        public string MethodReturnTypeDecodable
        {
            get
            {
                return HasReturnValue() ?
                    ((ReturnValue().Body is IVariableType) ? 
                        ((IVariableType)ReturnValue().Body).DecodeTypeDeclaration(false) 
                            : ObjCNameHelper.GetTypeName(ReturnValue().Body.Name.ToString(), false))
                        : "Void";
            }
        }

        /// <summary>
        /// Returns the method return signature for this method (e.g. "foo, bar").
        /// </summary>
        /// <param name="helper">Indicates if this method is a helper method (i.e. preparer/sender/responder).</param>
        /// <returns>The method signature for this method.</returns>
        public string MethodReturnSignature(bool helper)
        {
            var retValType = MethodReturnType;
            return $"{retValType}";
        }

        public IReadOnlyList<ParameterObjC> URLParameters
        {
            get
            {
                return Parameters.Where(x => { return x.Location == ParameterLocation.Path; })
                    .Select(x => { return (ParameterObjC)x; }).ToList();
            }
        }

        public IReadOnlyList<ParameterObjC> QueryParameters
        {
            get
            {
                return Parameters.Where(x => { return x.Location == ParameterLocation.Query; })
                    .Select(x => { return (ParameterObjC)x; }).ToList();
            }
        }

        public IReadOnlyList<ParameterObjC> HeaderParameters
        {
            get
            {
                return Parameters.Where(x => { return x.Location == ParameterLocation.Header; })
                    .Select(x => { return (ParameterObjC)x; }).ToList();
            }
        }

        public ParameterObjC BodyParameter
        {
            get
            {
                return Parameters.Where(x => { return x.Location == ParameterLocation.Body; })
                    .Select(x => { return (ParameterObjC)x; }).FirstOrDefault();
            }
        }

        public IReadOnlyList<ParameterObjC> AllParameters
        {
            get
            {
                List<ParameterObjC> allParameters = new List<ParameterObjC>();
                allParameters.AddRange(this.URLParameters);
                allParameters.AddRange(this.QueryParameters);
                allParameters.AddRange(this.HeaderParameters);
                if(this.BodyParameter != null) {
                    allParameters.Add(this.BodyParameter);
                }

                return allParameters;
            }
        }

        public string ServiceModelName
        {
            get
            {
                return ((CodeModelObjC)this.CodeModel).ServiceName;
            }
        }

        /// <summary>
        /// Check if method has a return response.
        /// </summary>
        /// <returns></returns>
        public bool HasReturnValue()
        {
            return ReturnValue()?.Body != null;
        }

        /// <summary>
        /// Return response object for the method.
        /// </summary>
        /// <returns></returns>
        public Response ReturnValue()
        {
            return ReturnType ?? DefaultResponse;
        }

        public bool IsBodyParameterTypeAnEnum() {
            if(this.BodyParameter == null) {
                return false;
            }

            return this.BodyParameter.ModelType is EnumType;
        }

        public bool IsBodyParameterTypeNSData() {
            if(this.BodyParameter == null) {
                return false;
            }

            return this.BodyParameter.ModelType is PrimaryTypeObjC &&
                (this.BodyParameter.ModelType as PrimaryTypeObjC).KnownPrimaryType == 
                KnownPrimaryType.Stream;
        }

        public bool IsReturnTypeAnEnum() {
            if(!this.HasReturnValue()) {
                return false;
            }

            return this.ReturnType.Body is EnumType;
        }

        public bool IsReturnTypeData() {
            if(!this.HasReturnValue()) {
                return false;
            }

            return this.ReturnType.Body is PrimaryTypeObjC &&
                (this.ReturnType.Body as PrimaryTypeObjC).KnownPrimaryType == 
                KnownPrimaryType.Stream;
        }

        /// <summary>
        /// Return response object for the method.
        /// </summary>
        /// <returns></returns>
        public string ReturnTypeDeclaration()
        {
            if (this.HasReturnValue())
            {
                if(this.ReturnType.Body is IVariableType)
                {
                    return ((IVariableType)this.ReturnType.Body).VariableTypeDeclaration(false);
                }else
                {
                    return this.ReturnType.Body.Name;
                }
            }
            else
            {
                return "Void";
            }
        }

        /// <summary>
        /// Return initial response type.
        /// </summary>
        /// <returns></returns>
        public string ResponseContentType()
        {
            if (this.ResponseContentTypes.Length == 0)
            {
                return this.RequestContentType;
            }
            else
            {
                return this.ResponseContentTypes[0];
            }
        }

        /// <summary>
        /// Checks if method has pageable extension (x-ms-pageable) enabled.  
        /// </summary>
        /// <returns></returns>

        public bool IsPageable => HasNextLink;

        public bool IsNextMethod => Name.Value.EqualsIgnoreCase(NextOperationName);

        /// <summary>
        /// Checks if method for next page of results on paged methods is already present in the method list.
        /// </summary>
        /// <param name="methods"></param>
        /// <returns></returns>
        public bool NextMethodExists(IEnumerable<MethodObjC> methods)
        {
            string next = NextOperationName;
            if (string.IsNullOrEmpty(next))
            {
                return false; 
            }
            return methods.Any(m => m.Name.Value.EqualsIgnoreCase(next));
        }

        public MethodObjC NextMethod
        {
            get
            {
                if (Extensions.ContainsKey(AzureExtensions.PageableExtension))
                {
                    var pageableExtension = JsonConvert.DeserializeObject<PageableExtension>(Extensions[AzureExtensions.PageableExtension].ToString());
                    if (pageableExtension != null && !string.IsNullOrWhiteSpace(pageableExtension.OperationName))
                    {
                        return (CodeModel.Methods.First(m => m.SerializedName.EqualsIgnoreCase(pageableExtension.OperationName)) as MethodObjC);
                    }
                }
                return null;
            }
        }

        public string NextOperationName
        {
            get
            {
                return NextMethod?.Name.Value;
            }
        }

        public Method NextOperation
        {
            get
            {
                if (Extensions.ContainsKey(AzureExtensions.PageableExtension))
                {
                    var pageableExtension = JsonConvert.DeserializeObject<PageableExtension>(Extensions[AzureExtensions.PageableExtension].ToString());
                    if (pageableExtension != null && !string.IsNullOrWhiteSpace(pageableExtension.OperationName))
                    {
                        return CodeModel.Methods.First(m => m.SerializedName.EqualsIgnoreCase(pageableExtension.OperationName));
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Check if method has long running extension (x-ms-long-running-operation) enabled. 
        /// </summary>
        /// <returns></returns>
        public bool IsLongRunningOperation()
        {
            try
            {
                return Extensions.ContainsKey(AzureExtensions.LongRunningExtension) && (bool)Extensions[AzureExtensions.LongRunningExtension];
            }
            catch (InvalidCastException e)
            {
                var message = $@"{
                    e.Message
                    } The value \'{
                    Extensions[AzureExtensions.LongRunningExtension]
                    }\' for extension {
                    AzureExtensions.LongRunningExtension
                    } for method {
                    Group
                    }. {
                    Name
                    } is invalid in Swagger. It should be boolean.";

                throw new InvalidOperationException(message);
            }
        }

        public bool HasNextLink
        {
            get
            {
                // Note:
                // Methods can be paged, even if "nextLinkName" is null
                // Paged method just means a method returns an array
                if (Extensions.ContainsKey(AzureExtensions.PageableExtension))
                {
                    var pageableExtension = Extensions[AzureExtensions.PageableExtension] as Newtonsoft.Json.Linq.JContainer;
                    if (pageableExtension != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Add NextLink attribute for pageable extension for the method.
        /// </summary>
        /// <returns></returns>
        public string NextLink
        {
            get
            {
                // Note:
                // Methods can be paged, even if "nextLinkName" is null
                // Paged method just means a method returns an array
                if (Extensions.ContainsKey(AzureExtensions.PageableExtension))
                {
                    var pageableExtension = Extensions[AzureExtensions.PageableExtension] as Newtonsoft.Json.Linq.JContainer;
                    if (pageableExtension != null)
                    {
                        var nextLink = (string)pageableExtension["nextLinkName"];
                        if (!string.IsNullOrEmpty(nextLink))
                        {
                            return CodeNamerObjC.Instance.GetPropertyName(nextLink);
                        }
                    }
                }
                return null;
            }
        }

        public string ApiVersion 
        {
            get 
            {
                return this.CodeModel.ApiVersion ?? "";
            }
        }

        /// <summary>
        /// Gets the execute command name.
        /// </summary>
        /// <returns>The execute command name</returns>
        public string GetExecuteCommandName()
        {
            return this.IsLongRunningOperation() ? "executeAsyncLRO" : "executeAsync";
        }

        public string EncodeMimeType 
        {
            get 
            {
                if(this.BodyParameter != null &&
                    (this.MethodReturnTypeDecodable == "Data" ||
                    this.MethodReturnTypeDecodable == "Data?")) {
                        return "application/octet-stream";
                }
                var mimeType = this.RequestContentType;   
                return mimeType.IndexOf(';') > 0 ? 
                    mimeType.Substring(0, mimeType.IndexOf(';')) : mimeType;
            }
        }

        public string DecodeMimeType 
        {
            get 
            {
                if( HasReturnValue() &&
                    (this.MethodReturnTypeDecodable == "Data" ||
                    this.MethodReturnTypeDecodable == "Data?")) {
                    return "application/octet-stream";
                }

                var mimeType = this.ResponseContentType();   
                return mimeType.IndexOf(';') > 0 ? 
                    mimeType.Substring(0, mimeType.IndexOf(';')) : mimeType;
            }
        }

        public bool IsReturnTypeOptional {
            get {
                return this.MethodReturnType.EndsWith("?");
            }
        }

        public string RequiredPropertiesForInitParameters(bool forMethodCall = false)
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = this.URLParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>().ToList();
            properties.AddRange(this.QueryParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>());
            properties.AddRange(this.HeaderParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>());
            if(this.BodyParameter != null) {
                properties.Add(this.BodyParameter);
            }
            
            var seperator = "";

            // Emit each property, except for named Enumerated types, as a pointer to the type
            foreach (var property in properties)
            {
                var modelType = property.ModelType;
                var modelDeclaration = modelType.Name;
                if (modelType is IVariableType)
                {
                    modelDeclaration = ((IVariableType)modelType).VariableTypeDeclaration(property.IsRequired);
                }


                var output = string.Empty;
                var propName = property.VariableName;

                if (property.IsRequired &&
                    (!propName.Equals("apiVersion") ||
                    property.Location != ParameterLocation.Query))
                {
                    if(forMethodCall) {
                        indented.Append($"{seperator}{propName}: {propName}");
                    }else {
                        indented.Append($"{seperator}{propName}: {modelDeclaration}");
                    }

                    seperator = ", ";
                }
            }

            return indented.ToString();
        }

        public bool HasRequiredPropertiesForInitParameters()
        {
            var properties = this.URLParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>().ToList();
            properties.AddRange(this.QueryParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>());
            properties.AddRange(this.HeaderParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>());
            if(this.BodyParameter != null) {
                properties.Add(this.BodyParameter);
            }
            
            foreach (var property in properties)
            {
                var propName = property.VariableName;
                if (property.IsRequired &&
                    (!propName.Equals("apiVersion") ||
                    property.Location != ParameterLocation.Query))
                {
                    return true;
                }
            }

            return false;
        }

        public string RequiredPropertiesSettersForInitParameters()
        {
            var indented = new IndentedStringBuilder("    ");
            var properties = this.URLParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>().ToList();
            properties.AddRange(this.QueryParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>());
            properties.AddRange(this.HeaderParameters.Cast<AutoRest.ObjC.Model.ParameterObjC>());
            if(this.BodyParameter != null) {
                properties.Add(this.BodyParameter);
            }

            foreach (var property in properties)
            {
                var propName = property.VariableName;
                var modelType = property.ModelType;
                if (property.IsRequired &&
                    (!propName.Equals("apiVersion") ||
                    property.Location != ParameterLocation.Query))
                {
                    indented.Append($"self.{propName} = {propName}\r\n");
                }
            }

            return indented.ToString();
        }
    }
}
