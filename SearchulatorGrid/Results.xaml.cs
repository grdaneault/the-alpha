using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using SearchulatorGrid.Common;
using SearchulatorGrid.Pods;
using Windows.ApplicationModel.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Callisto.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace SearchulatorGrid
{
    /// <summary>
    ///     This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class Results : SearchulatorGrid.Common.LayoutAwarePage
    {
        public SearchResult Result
        {
            get { return GetValue(ResultProperty) as SearchResult; }
            set { SetValue(ResultProperty, value); }
        }

        private bool _firstLoad = true;
        private Stack<SearchResult> _history; 
        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof (SearchResult),
                                        typeof (LayoutAwarePage), null);

        public Results()
        {
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true; // Allow user to start searching by typing from main window
            _history = new Stack<SearchResult>();
            InitializeComponent();
        }


        protected override void SaveState(Dictionary<string, object> pageState)
        {
            pageState["result"] = Result;
            pageState["history"] = _history;

            base.SaveState(pageState);
        }

        /// <summary>
        ///     Populates the page with content passed during navigation.  Any saved state is also
        ///     provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">
        ///     The parameter value passed to
        ///     <see cref="Frame.Navigate(Type, Object)" /> when this page was initially requested.
        /// </param>
        /// <param name="pageState">
        ///     A dictionary of state preserved by this page during an earlier
        ///     session.  This will be null the first time a page is visited.
        /// </param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {

            var r = navigationParameter as SearchResult;
            if (r != null)
            {
                r.ResetAssumptionSelections();
                Result = r;
                //ShowResults();
                return;
            }

            var query = navigationParameter as string;
            if (pageState != null && pageState.ContainsKey("result"))
            {
                Result = pageState["result"] as SearchResult;
                Result.ResetAssumptionSelections();
                _history = pageState["history"] as Stack<SearchResult>;
            }
            else
            {
                Result = new SearchResult(query) {NumRows = (int) Math.Floor((Window.Current.Bounds.Height - 186) / 150)};
                await Result.RunSearch();
                _history.Push(new SearchResult(Result));
            }

            ShowResults();
        }


        /// <summary>
        ///     Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ShowResults()
        {
            VisualStateManager.GoToState(this, Result.Pods.Any() ? "ResultsFound" : "NoResults", true);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Result == null)
            {
                var r = e.Parameter as SearchResult;
                if (r != null)
                {
                    Result = r;
                }
                Result = new SearchResult(e.Parameter as string);
            }

            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Refresh)
            {
                //Q42.WinRT.Data.WebDataCache.ClearAll();
            }

            base.OnNavigatedTo(e);
        }

        private void Chose_Item(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Chose_Item(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AdPod)
                return;

            Windows.UI.ViewManagement.ApplicationView.TryUnsnap(); // Unsnap the window
            Frame.Navigate(typeof (DetailPopup), e.ClickedItem as Pod);
        }

        private void FlipViewTapHandler(object sender, TappedRoutedEventArgs e)
        {

        }

        private void FlipViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void MasterButton(object sender, RoutedEventArgs e)
        {
            var pb = sender as PodButton;
            pb.PodOwner.Result = Result;

            Frame.Navigate(typeof (DetailPopup), pb.PodOwner);
        }

        private void FlipViewDoubleTapHandler(object sender, DoubleTappedRoutedEventArgs e)
        {
        }

        private void WindowSizedChanged(object sender, SizeChangedEventArgs e)
        {
            if (Result != null)
            {
                Result.NumRows = (int) Math.Floor((e.NewSize.Height - 186) / 150);
            }
        }

        private async void AssumptionSet(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as ComboBox;
            // wow.
            if (Result.Assumptions != null)
            {
                var changed =
                    Result.Assumptions.FirstOrDefault(
                        a => s != null && a.Values.SequenceEqual(s.ItemsSource as List<string>));


                if (changed != null && !_firstLoad && e.RemovedItems.Count > 0)
                {
                    var r = new SearchResult(Result);
                    await r.ApplyAssumption(changed);
                    _history.Push(r);
                    Result = r;
                }
                _firstLoad = false;
            }
        }

        private void CustomGoBack(object sender, RoutedEventArgs e)
        {
            if (_history.Count > 1)
            {
                _history.Pop();
                Result = _history.Peek();
                Result.ResetAssumptionSelections();
                _firstLoad = true;
            }
            else
            {
                Frame.GoBack();
            }
        }

        private Flyout _assumptionsFlyout;
        private void GetAssumptionsFlyout()
        {
            if (_assumptionsFlyout == null)
            {

                Flyout flyout = new Flyout();
                // creating some content
                // this could be just an instance of a UserControl of course
                Border border = new Border();
                ScrollViewer scroll = new ScrollViewer();
                scroll.MaxHeight = 600;
                scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                //scroll.MaxWidth = 400;
                //b.BorderBrush = new SolidColorBrush(Colors.White);
                //b.Width = 300;
                //b.Height = 600;
                border.BorderThickness = new Thickness(5);
                border.Margin = new Thickness(-5);
                Grid assumptionLayoutGrid = new Grid();
                assumptionLayoutGrid.Margin = new Thickness(0);
                int i = 0;
                ColumnDefinition col = new ColumnDefinition();
                assumptionLayoutGrid.ColumnDefinitions.Add(col);
                foreach (var assumption in Result.Assumptions)
                {
                    RowDefinition headerRow = new RowDefinition();
                    RowDefinition body = new RowDefinition();

                    assumptionLayoutGrid.RowDefinitions.Add(headerRow);
                    assumptionLayoutGrid.RowDefinitions.Add(body);


                    TextBlock assumptionHeaderText = new TextBlock();
                    assumptionHeaderText.Text = assumption.DescriptionBefore;
                    assumptionHeaderText.FontSize = 26;
                    assumptionHeaderText.Margin = new Thickness(4, 4, 24, 4);
                    Border assumptionHeader = new Border();
                    assumptionHeader.Child = assumptionHeaderText;
                    assumptionHeader.Background = new SolidColorBrush(Colors.LightGray);

                    assumptionLayoutGrid.Children.Add(assumptionHeader);
                    Grid.SetColumn(assumptionHeader, 0);
                    Grid.SetRow(assumptionHeader, i);

                    StackPanel choicesPanel = new StackPanel();
                    choicesPanel.Orientation = Orientation.Vertical;
                    choicesPanel.Margin = new Thickness(8, 4, 24, 4);
                    assumptionLayoutGrid.Children.Add(choicesPanel);
                    Grid.SetRow(choicesPanel, i + 1);
                    Grid.SetColumn(choicesPanel, 0);

                    foreach (var choice in assumption.Values)
                    {
                        RadioButton c = new RadioButton();
                        c.Content = choice;
                        c.GroupName = assumption.GetQueryString();
                        choicesPanel.Children.Add(c);
                        c.IsChecked = (assumption.SelectedWord == choice);
                        c.Checked += c_Checked;
                        c.DataContext = assumption;
                    }
                    i += 2;

                }


                border.Child = scroll;
                scroll.Content = assumptionLayoutGrid;

                // set the Flyout content
                flyout.Content = border;
                //flyout.

                //b.Child = container;
                //filters.Content = b;
                flyout.PlacementTarget = QueryText;
                flyout.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Bottom;
                flyout.IsOpen = true;
                //f.Background = new SolidColorBrush(Colors.Black);
                _assumptionsFlyout = flyout;
            }
            else
            {
                _assumptionsFlyout.IsOpen = true;
            }
        }

        private async void c_Checked(object sender, RoutedEventArgs e)
        {
            var s = sender as RadioButton;
            var assumption = s.DataContext as Assumption;
            _assumptionsFlyout.IsOpen = false;
            _assumptionsFlyout.Dispose();
            _assumptionsFlyout = null;
            Result.Working = true;
            assumption.SelectedWord = s.Content as string;
            var r = new SearchResult(Result);
            await r.ApplyAssumption(assumption);
            _history.Push(r);
            Result = r;
            Result.Working = false;
        }

        private void ShowFilters(object sender, TappedRoutedEventArgs e)
        {
            if (Result.HasAssumptions)
            {
                GetAssumptionsFlyout();
            }
        }

        private void LogoTextTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), Result.QueryRaw);
        }

        private void HomeButton_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void CycleZoomLevels(object sender, DoubleTappedRoutedEventArgs e)
        {
            var sV = sender as ScrollViewer;
            if (sV != null)
            {
                if (sV.ZoomFactor == 0.5f)
                {
                    sV.ZoomToFactor(1f);
                }
                else if (sV.ZoomFactor == 1f)
                {
                    sV.ZoomToFactor(2f);
                }
                else if (sV.ZoomFactor == 2f)
                {
                    sV.ZoomToFactor(0.5f);
                }
                else
                {
                    sV.ZoomToFactor(1f);
                }

            }
        }
    }
}