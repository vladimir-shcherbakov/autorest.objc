using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRest.ObjectiveC.Builder
{
    public interface IServiceClient
    {
        IServiceClient WithBaseUrl(string baseUrl);
        
        IServiceClient WithKey(string key);
        
        IOperation WithOperation(string name);

        // builds request artifacts: headers, body, query, path
        void Build();
    }

    public class ServiceClient : IServiceClient
    {
        private string _name;
        private string _key;
        internal IList<Operation> Operations = new List<Operation>();

        protected ServiceClient(string name)
        {
            _name = name;
        }

        public static IServiceClient Define(string name)
        {
            var instance = new ServiceClient(name);
            return instance;
        }

        public IServiceClient WithBaseUrl(string baseUrl)
        {
            // if "{endpoint}" exsists in the URI - add the param "endpoint" to the factory method
            return this;
        }

        public IServiceClient WithKey(string key)
        {
            // add the param to the facrory method
            _key = key;
            return this;
        }

        public IOperation WithOperation(string name)
        {
            var operation = new Operation(name, this);
            return operation;
        }

        public void Build()
        {
            throw new NotImplementedException();
        }
    }
}
