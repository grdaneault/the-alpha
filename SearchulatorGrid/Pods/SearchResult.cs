using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SearchulatorGrid.Pods
{
    public class SearchResult : Common.BindableBase
    {
        private PodCollection _pods;
        private List<Assumption> _assumptions;
        private string _query;
        private string _url;
        private string _states;
        private Uri _uri;

        private int _numRows;
        private bool _working;

        public SearchResult(SearchResult other)
        {
            _assumptions = new List<Assumption>();
            foreach (var assumption in other._assumptions)
            {
                _assumptions.Add(new Assumption(assumption));
            }
            

            _query = other._query;
            _url = other._url;
            _uri = other._uri;
            _states = other._states;
            _numRows = other._numRows;
            _working = other._working;
            _pods = new PodCollection(other._pods);
        }

        public bool Working
        {
            get { return _working; }
            set
            {
                if (SetProperty(ref _working, value))
                {
                    OnPropertyChanged("Working");
                }
            }
        }

        public int NumRows
        {
            get { return _numRows; }
            set
            {
                if (SetProperty(ref _numRows, value))
                {
                    if (_pods != null)
                    {
                        _pods.Resize(_numRows);
                    }
                    OnPropertyChanged("Pods");
                }
            }
        }

        public bool HasAssumptions
        {
            get { return _assumptions != null && _assumptions.Any(); }
        }

        public string QueryRaw
        {
            get { return _query; }
            set { Query = value; }
        }

        public string Query
        {
            get { return '\u201c' + _query + '\u201d'; }
            set
            {
                if (SetProperty(ref _query, value))
                {
                    OnPropertyChanged("Query");
                }
            }
        }

        public PodCollection Pods
        {
            get { return _pods; }
            set
            {
                if (SetProperty(ref _pods, value))
                {
                    OnPropertyChanged("Pods");
                }
            }
        }

        public List<Assumption> Assumptions
        {
            get { return _assumptions; }
            set
            {
                if (SetProperty(ref _assumptions, value))
                {
                    OnPropertyChanged("Assumptions");
                }
            }
        }

        private const string ApiUrl = "http://api.wolframalpha.com/v2/query?appid=A83TWA-A46U8PAEWY&reinterpret=true&";

        public SearchResult(string query)
        {
            _query = query;
        }

        private void BuildUrl()
        {
            _url = ApiUrl + "input=" + Uri.EscapeDataString(_query) + "&mag=1"; //1.6

            if (_assumptions != null)
            {
                foreach (var assumption in _assumptions)
                {
                    _url += assumption.GetQueryString();
                }
            }

            _url += _states;

            _uri = new Uri(_url);
        }


        public async Task<SearchResult> RunSearch()
        {
            Working = true;

            BuildUrl();

            var cache = await Q42.WinRT.Data.WebDataCache.GetAsync(_uri, forceGet: false);
            IRandomAccessStream readStream = await cache.OpenAsync(FileAccessMode.Read);
            XDocument xml = XDocument.Load(readStream.AsStreamForRead());


            try
            {
                Pods = new PodCollection(from pod in xml.Descendants("pod") select new Pod(pod, this), NumRows);
                Assumptions =
                    (from 
                         assumption in xml.Descendants("assumptions").Descendants("assumption")
                     select new Assumption(assumption)).ToList();

                OnPropertyChanged("HasAssumptions");
                Working = false;
                return this;
            }
            catch (Exception e)
            {
                if (_pods == null)
                {
                    Pods = new PodCollection(new List<Pod>(), NumRows);
                }
                if (_assumptions == null)
                {
                    Assumptions = new List<Assumption>();
                }
                Working = false;
                return this;
            }
        }

        public async Task<SearchResult> ApplyAssumption(Assumption changed)
        {
            if (_url.Contains(changed.GetQueryString()))
            {
                return this;
            }

            if (changed.SelectedIndex == 0)
            {
                return this;
            }

            _states = ""; // Reset the states when assumptions change
            return await RunSearch();
        }

        public async Task<SearchResult> ApplyState(State s)
        {
            _states += s.GetQueryString();
            return await RunSearch();
        }

        public void ResetAssumptionSelections()
        {
            foreach (Assumption assumption in Assumptions)
            {
                assumption.ResetSelection();
            }
        }

        public void FillSample()
        {
            var podsRaw = new List<Pod>();

            var ad = new ImageResult("http://smallgroupproductions.com/wp-content/uploads/AdPlaceholder.png",
                                     292, 90);
            //ImageResult ad250 = new ImageResult("http://smallgroupproductions.com/wp-content/uploads/250x250_ad_placeholder.png", 250, 250);
            var input = new ImageResult("ms-appx:///Assets/5/input.gif", 8, 18);
            var numName = new ImageResult("ms-appx:///Assets/5/number name.gif", 23, 18);
            var visualRep = new ImageResult("ms-appx:///Assets/5/visual representation.gif", 62, 17);
            var numLine = new ImageResult("ms-appx:///Assets/5/number line.gif", 300, 55);
            var romanNumerals = new ImageResult("ms-appx:///Assets/5/roman numerals.gif", 10, 18);
            var binaryForm = new ImageResult("ms-appx:///Assets/5/binary form.gif", 31, 18);
            var primeFact = new ImageResult("ms-appx:///Assets/5/prime fact.gif", 137, 18);
            var residues = new ImageResult("ms-appx:///Assets/5/residues.gif", 314, 68);
            var prop1 = new ImageResult("ms-appx:///Assets/5/prop-1.gif", 131, 18);
            var prop2 = new ImageResult("ms-appx:///Assets/5/prop-2.gif", 296, 43);
            var prop3 = new ImageResult("ms-appx:///Assets/5/prop-3.gif", 271, 37);
            var prop4 = new ImageResult("ms-appx:///Assets/5/prop-4.gif", 412, 18);
            var quadResidue = new ImageResult("ms-appx:///Assets/5/quad-residue.gif", 500, 18);
            var primitiveRoots = new ImageResult("ms-appx:///Assets/5/primitive roots.gif", 500, 18);
            var charCodes = new ImageResult("ms-appx:///Assets/5/char codes.gif", 357, 89);
            var propList = new List<ImageResult>() {prop1, prop2, prop3, prop4};
            podsRaw.Add(new Pod("Input", ad));
            podsRaw.Add(new Pod("Number Name", numName));
            podsRaw.Add(new Pod("Visual Representation", visualRep));
            podsRaw.Add(new Pod("Number Line", numLine));
            podsRaw.Add(new Pod("Roman Numerals", romanNumerals));
            podsRaw.Add(new Pod("Binary Form", binaryForm));
            podsRaw.Add(new Pod("Prime Factorization", primeFact));
            podsRaw.Add(new Pod("Residues", residues));
            podsRaw.Add(new Pod("Properties", propList));
            podsRaw.Add(new Pod("Quadratic Residue", quadResidue));
            podsRaw.Add(new Pod("Primitive Roots", primitiveRoots));
            podsRaw.Add(new Pod("Character Codes", charCodes));
            Query = "5";

            /*
            ImageResult one = new ImageResult("ms-appx:///Assets/One.png", 292, 100);
            ImageResult two = new ImageResult("ms-appx:///Assets/Two.png", 292, 100);
            ImageResult three = new ImageResult("ms-appx:///Assets/Three.png", 292, 100);
            ImageResult four = new ImageResult("ms-appx:///Assets/Four.png", 292, 100);
            ImageResult five = new ImageResult("ms-appx:///Assets/Five.png", 200, 30);
            ImageResult six = new ImageResult("ms-appx:///Assets/Six.png", 200, 68);
            ImageResult seven = new ImageResult("ms-appx:///Assets/Seven.png", 400, 800);
            List<ImageResult> all = new List<ImageResult>() { six, one, two, three, four, five, six };
                
            Pod p1 = new Pod("One", all);
            Pod p2 = new Pod("Two", seven); 
            Pod p3 = new Pod("Three", all);
            Pod p4 = new Pod("Four", four);
            Pod p5 = new Pod("Five", all);
            Pod p6 = new Pod("Six", two);
            Pod p7 = new Pod("Seven", one);
            Pod p8 = new Pod("Eight", three);
            Pod p9 = new Pod("Nine", three);
            Pod p10 = new Pod("Ten", two);
            Pod p11 = new Pod("Eleven", four);
            Pod p12 = new Pod("Twelve", three);
            Pod p13 = new Pod("Thirteen", one);

            p1.ColSpan = 2;
            p4.ColSpan = 2;
            p5.ColSpan = 2;
            p5.RowSpan = 2;
            p10.ColSpan = 2;
            p10.RowSpan = 2;
            p11.ColSpan = 2;
            p11.RowSpan = 2;
            p12.ColSpan = 2;
            p13.ColSpan = 2;
            p13.RowSpan = 2;
                    
            podsRaw.Add(p1);
            podsRaw.Add(p2);
            podsRaw.Add(p3);
            podsRaw.Add(p4);
            podsRaw.Add(p5);
            podsRaw.Add(p6);
            podsRaw.Add(p7);
            podsRaw.Add(p8);
            podsRaw.Add(p9);
            podsRaw.Add(p10);
            podsRaw.Add(p11);
            podsRaw.Add(p12);
            podsRaw.Add(p13);
            */

            Pods = new PodCollection(podsRaw, NumRows);
            Assumptions = new List<Assumption>();
        }
    }
}