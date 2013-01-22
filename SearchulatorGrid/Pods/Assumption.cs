using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SearchulatorGrid.Pods
{
    public class Assumption : Common.BindableBase
    {
        public List<string> Values { get; private set; }
        private List<AssumptionValue> _assumptionValues;

        public enum AssumptionType
        {
            Clash,
            Unit,
            AngleUnit,
            Function,
            MultiClash,
            SubCategory,
            Attribute,
            TimeAMOrPM,
            DateOrder,
            ListOrNumber,
            CoordinateSystem,
            I,
            NumberBase,
            MixedFraction,
            MortalityYearDOB,
            DNAOrString,
            TideStation
        };

        private int _selectedIndex;
        private readonly int _originalIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { if (SetProperty(ref _selectedIndex, value)) OnPropertyChanged(); }
        }


        public string Type { get; private set; }
        public string Word { get; private set; }
        public string DescriptionBefore { get; private set; }
        public string DescriptionAfter { get; private set; }

        public Assumption(Assumption other)
        {
            _assumptionValues = new List<AssumptionValue>(other._assumptionValues);
            _selectedIndex = other.SelectedIndex;
            _originalIndex = other.SelectedIndex;
            Values = new List<string>(other.Values);
            DescriptionAfter = other.DescriptionAfter;
            DescriptionBefore = other.DescriptionBefore;

        }

        public Assumption(XElement source)
        {
            _assumptionValues = new List<AssumptionValue>();
            foreach (XElement val in source.Elements("value"))
            {
                _assumptionValues.Add(new AssumptionValue(val));
            }

            Values = new List<string>();
            foreach (AssumptionValue val in _assumptionValues)
            {
                Values.Add(val.Description);
            }

            _selectedIndex = 0;
            _originalIndex = 0;

            Type = source.Attribute("type").Value;
            switch (Type)
            {
                case "Clash":
                case "Function":
                case "MultiClash":
                case "ListOrNumber":
                case "NumberBase":
                case "MixedFraction":
                case "DNAOrString":
                    Word = source.Attribute("word").Value;
                    DescriptionBefore = "Assuming '" + Word + "' is";
                    DescriptionAfter = "";
                    break;
                case "Unit":
                    Word = source.Attribute("word").Value;
                    DescriptionBefore = "Assuming";
                    DescriptionAfter = "for '" + Word + "'";
                    break;
                case "AngleUnit":
                    DescriptionBefore = "Assuming trigonometric arguments in ";
                    DescriptionAfter = "";
                    break;
                case "TideStation":
                    DescriptionBefore = "Using";
                    DescriptionAfter = "";
                    break;
                case "SubCategory":
                case "Attribute":
                case "DateOrder":
                case "ListOrTimes":
                case "CoordinateSystem":
                case "I":
                case "MortalityYearDOB":
                default:
                    DescriptionBefore = "Assuming";
                    DescriptionAfter = "";
                    break;
            }
        }



        public string GetInputFromDescription(string desc)
        {
            return (from val in _assumptionValues where val.Description == desc select val.Input).FirstOrDefault();

            /*
            foreach (AssumptionValue val in _assumptionValues)
            {
                if (val.Description == desc)
                {
                    return val.Input;
                }
            }

            return null;
             * */
        }

        public bool Active { get
        {
            return true;  // _selectedIndex != 0;
        } }

        internal string GetQueryString()
        {
            if (!Active) return "";

            return "&assumption=" + _assumptionValues[SelectedIndex].Input;
        }

        public void ResetSelection()
        {
            SelectedIndex = _originalIndex;
        }
    }

    internal class AssumptionValue
    {
        public string Description { get; private set; }
        public string Input { get; private set; }
        public string Name { get; private set; }

        public AssumptionValue(XElement val)
        {
            Description = val.Attribute("desc").Value;
            Input = val.Attribute("input").Value;
            Name = val.Attribute("name").Value;
        }
    }
}