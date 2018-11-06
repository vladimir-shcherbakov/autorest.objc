using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRest.ObjectiveC.Builder
{
    public interface IParameterMetadata
    {

    }

    public class ParameterMetadata  
    {
        public string Type { get; set; }

        public bool IsRequired { get; set; }
        
        public string DefaultValue { get; set; }
        
        public IEnumerable<string> EnumSet { get; set; } = new List<string>();

        

    }
}
