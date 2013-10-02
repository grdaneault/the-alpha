using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SearchulatorGrid.Common;

namespace SearchulatorGrid.Pods
{
    public class Pod : BindableBase
    {
        public Pod()
        {
            _title = "DEFAULT";
            _images = new List<ImageResult>();
            _states = new List<State>();
            // fake
        }
        // Constants
        public static int MaxRows = 4;
        public static int MaxCols = 2;
        public static int GridSizeRow = 150;
        public static int GridSizeCol = 310;

        private SearchResult _result;
        public SearchResult Result
        {
            get { return _result; }
            set
            {
                if (SetProperty(ref _result, value))
                {
                    OnPropertyChanged();
                }
            }
        }


        //Images

        private List<ImageResult> _images;
        private int _currentIndex;
        private int _numSubpods;


        public List<ImageResult> Images
        {
            get { return _images; }
            set
            {
                if (SetProperty(ref _images, value))
                {
                    OnPropertyChanged();
                    _numSubpods = _images.Count;
                }
            }
        }

        public int NumSubpods
        {
            get { return _numSubpods; }
        }


        public ImageResult CurrentImage
        {
            get { return _images.Count == 0 ? null : _images[CurrentIndex]; }
        }

        public bool IsGallery
        {
            get { return _numSubpods > 1; }
        }


        // States
        private List<State> _states;
        private int _numStates;

        public List<State> States
        {
            get { return _states; }
            set
            {
                if (SetProperty(ref _states, value))
                {
                    _numStates = _states.Count;
                    OnPropertyChanged();
                }
            }
        }

        public int NumStates
        {
            get { return _numStates; }
        }

        public bool HasStates
        {
            get { return _numStates > 0; }
        }


        // Infos
        private int _numInfos;
        private List<Info> _infos;

        public List<Info> Infos
        {
            get { return _infos; }
            private set { _infos = value; }
        }

        public int NumInfos
        {
            get { return _numInfos; }
        }

        public bool HasInfos
        {
            get { return _numInfos > 0; }
        }

        public bool HasAppBar
        {
            get { return HasInfos || HasStates; }
        }


        // Header Block
        private string _title;

        public string Title
        {
            get { return _title/* + " (" + CurrentImage.SnappedWidth + "x" + CurrentImage.SnappedHeight + ")" */; }
            set { if (SetProperty(ref _title, value)) OnPropertyChanged(); }
        }

        public string Subtitle
        {
            get
            {
                if (_numSubpods == 1)
                {
                    return "";
                }
                return (CurrentIndex + 1) + " of " + _numSubpods;
            }
        }

        public Windows.UI.Xaml.Visibility HasSubtitle
        {
            get { return _numSubpods == 1 ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible; }
        }

        public Windows.UI.Xaml.Thickness TitleMargin
        {
            get
            {
                if (_numSubpods == 1)
                {
                    return new Windows.UI.Xaml.Thickness(20, 10, 20, 20);
                }
                return new Windows.UI.Xaml.Thickness(20, 10, 20, 0);
            }
        }


        // Other fields
        public string Scanner { get; private set; }
        public string Id { get; private set; }

        public int Position { get; private set; }

        public bool Error { get; private set; }

        // UI Sizing, etc.
        public int RowSpan { get; set; }
        public int ColSpan { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }

        public int TileHeight
        {
            get { return GridSizeRow * RowSpan - 10; }
        }

        public int SnappedHeight
        {
            get { return 40 + CurrentImage.SnappedHeight + 10; }
        }

        public int TileWidth
        {
            get { return GridSizeCol * ColSpan - 10; }
        }

        public int ImageAreaHeight
        {
            get { return TileHeight - ((_numSubpods == 1) ? 50 : 60) + 6; }
        }

        // 50, 70 without extra space
        public int ImageAreaWidth
        {
            get { return TileWidth + 6; }
        }

        //10 For the space between tiles
        public bool Squashed
        {
            get { return ImageAreaHeight < _images.ElementAt(CurrentIndex).Height; }
        }

        public int CurrentIndex
        {
            get
            {
                return _currentIndex;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (SetProperty(ref _currentIndex, value)) OnPropertyChanged();
            }
        }

        public Pod(string title, ImageResult img)
        {
            img.Owner = this;
            _images = new List<ImageResult>();
            _images.Add(img);

            Title = title;

            RowSpan = CalcRowSpan(img.Height + 60);
            ColSpan = CalcColSpan(img.Width);

            _numSubpods = 1;
            _states = new List<State>();
        }

        public Pod(string title, List<ImageResult> imgs)
        {
            Title = title;
            int w = 0, h = 0;
            foreach (ImageResult img in imgs)
            {
                if (img.Width > w)
                {
                    w = img.Width;
                }
                if (img.Height > h)
                {
                    h = img.Height;
                }

                img.Owner = this;
            }

            RowSpan = CalcRowSpan(h + 80);
            ColSpan = CalcColSpan(w);
            //Title += " (" + RowSpan + " x " + ColSpan + ") ";
            _numSubpods = imgs.Count;
            _states = new List<State>();
            _images = imgs;
        }

        public Pod(XElement source, SearchResult searchResult)
        {
            Result = searchResult;
            _title = source.Attribute("title").Value;
            Scanner = source.Attribute("scanner").Value;
            Id = source.Attribute("id").Value;
            Position = int.Parse(source.Attribute("position").Value);
            Error = bool.Parse(source.Attribute("error").Value);

            _numSubpods = int.Parse(source.Attribute("numsubpods").Value);
            _images = new List<ImageResult>(_numSubpods);
            int maxHeight = 0, maxWidth = 0;
            foreach (XElement img in source.Elements("subpod"))
            {
                ImageResult i = new ImageResult(img);
                if (i.Height > maxHeight)
                {
                    maxHeight = i.Height;
                }
                if (i.Width > maxWidth)
                {
                    maxWidth = i.Width;
                }
                i.Owner = this;
                _images.Add(i);
            }

            XElement infos = source.Element("infos");
            if (infos != null)
            {
                _numInfos = int.Parse(infos.Attribute("count").Value);
                _infos = new List<Info>(_numInfos);
                foreach (XElement info in infos.Elements("info"))
                {
                    Info i = new Info(info);
                    _infos.Add(i);
                }
            }
            else
            {
                _infos = new List<Info>(0);
                _numInfos = 0;
            }

            XElement statesNode = source.Element("states");
            if (statesNode != null)
            {
                _numStates = int.Parse(statesNode.Attribute("count").Value);
                _states = new List<State>(_numStates);
                foreach (XElement state in statesNode.Elements("state"))
                {
                    _states.Add(new State(state));
                }
            }
            else
            {
                _states = new List<State>();
                _numStates = 0;
            }

            RowSpan = CalcRowSpan(maxHeight + 60);
            ColSpan = CalcColSpan(maxWidth);

            //_title += " (" + RowSpan + " x " + ColSpan + ") ";
        }

        private int CalcRowSpan(int height)
        {
            int rows = (int) Math.Ceiling((double) (height + 10) / GridSizeRow);
            return (rows < 1) ? 1 : ((rows > MaxRows) ? MaxRows : rows);
        }

        private int CalcColSpan(int width)
        {
            int cols = (int) Math.Ceiling((double) (width + 20) / GridSizeCol);
            //return (rows < 1) ? 1 : ((rows > MAX_COLS) ? MAX_COLS : rows);
            return (cols > 1) ? 2 : 1;
        }

        public void NextImage()
        {
            CurrentIndex++;
            CurrentIndex %= _numSubpods;
        }

        public void PrevImage()
        {
            CurrentIndex--;
            if (CurrentIndex < 0)
            {
                CurrentIndex = _numSubpods - 1;
            }
        }

        public override string ToString()
        {
            return _title;
        }

        /// <summary>
        ///     Resets the RowSpan and ColSpan to the default for use in resizing the screen
        /// </summary>
        public void ResetRowColSpans()
        {
            int maxHeight = 0, maxWidth = 0;
            foreach (ImageResult i in _images)
            {
                if (i.Height > maxHeight)
                {
                    maxHeight = i.Height;
                }
                if (i.Width > maxWidth)
                {
                    maxWidth = i.Width;
                }
            }
            RowSpan = CalcRowSpan(maxHeight + 60);
            ColSpan = CalcColSpan(maxWidth);
        }

        internal void Update(Pod pod)
        {
            Images = pod.Images;
            States = pod.States;
        }
    }
}