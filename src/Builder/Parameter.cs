using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRest.ObjectiveC.Builder
{
    public interface IParameter
    {
        IOperation Attach();

        IParameter OfType(string val);

        IParameter IsRequired(bool val);

        IParameter WithDefaultValue(string val);

        IParameter WithEnumSet(IEnumerable<string> val);
    }

    internal class Parameter : IParameter
    {
        private readonly Operation _parent;
        private readonly string _name;
        private string _type;
        private bool _isRequired;
        private string _defaultValue;
        private IEnumerable<string> _enumSet;

        public Parameter(string name, Operation parent)
        {
            _parent = parent;
            _name = name;
        }

        public IOperation Attach()
        {
            _parent.Parameters.Add(this);
            return _parent;
        }

        public IParameter OfType(string val)
        {
            _type = val;
            return this;
        }

        public IParameter IsRequired(bool val)
        {
           _isRequired = val;
            return this;
        }

        public IParameter WithDefaultValue(string val)
        {
            _defaultValue = val;
            return this;
        }

        public IParameter WithEnumSet(IEnumerable<string> val)
        {
            _enumSet = val;
            return this;
        }
    }
}
