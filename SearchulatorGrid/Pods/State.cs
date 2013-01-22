using System.Xml.Linq;

namespace SearchulatorGrid.Pods
{
    public class State
    {
        public string Name { get; set; }
        public string Input { get; set; }

        public State(XElement state)
        {
            Name = state.Attribute("name").Value;
            Input = state.Attribute("input").Value;
        }

        public string GetQueryString()
        {
            return "&podstate=" + Input;
        }
    }
}
