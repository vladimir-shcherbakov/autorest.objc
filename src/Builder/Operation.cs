using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRest.ObjectiveC.Builder
{
    public interface IOperation
    {
        // builds request artifacts: headers, body, query, path
        IServiceClient Attach();

        IParameter WithParameter(string name);

        IParameter WithDescription(string name);

        IParameter WithResonse(string name);

        IParameter WithDefaultResonse(string name);

        IParameter WithHttpMethod(string name);

        IParameter RequestContentType(string name);

        IParameter ResponceContentType(string name);

        
    }
    public class Operation : IOperation
    {
        internal ServiceClient Parent;
        internal readonly string Name;
        internal IList<IParameter> Parameters = new List<IParameter>();

        public Operation(string name, ServiceClient parent)
        {
            Name = name;
            Parent = parent;
        }
        public IServiceClient Attach()
        {
            Parent.Operations.Add(this);
            return Parent;
        }

        public IParameter WithParameter(string name)
        {
            var p = new Parameter(name, this);
            return p;
        }

        public IParameter WithDescription(string name)
        {
            throw new NotImplementedException();
        }

        public IParameter WithResonse(string name)
        {
            throw new NotImplementedException();
        }

        public IParameter WithDefaultResonse(string name)
        {
            throw new NotImplementedException();
        }

        public IParameter WithHttpMethod(string name)
        {
            throw new NotImplementedException();
        }

        public IParameter RequestContentType(string name)
        {
            throw new NotImplementedException();
        }

        public IParameter ResponceContentType(string name)
        {
            throw new NotImplementedException();
        }
    }
}
