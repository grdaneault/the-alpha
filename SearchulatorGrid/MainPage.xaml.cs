using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace SearchulatorGrid
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MainPage : SearchulatorGrid.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();

            /*
            var characterList = new List<Letter>();
            foreach (char c in "π°∞√∫∮∑∂∏∀∃⋃⋂∇∆αβγδεζηθκλμνξρστφχψωΓΘΛΞΥΦΨΩ℧Åℏℵ⇋→⊕⊙†".ToCharArray())
            {
                characterList.Add(new Letter(c));
            }

            DefaultViewModel["Characters"] = characterList;*/
            SearchPane.GetForCurrentView().SuggestionsRequested +=
                new TypedEventHandler<SearchPane, SearchPaneSuggestionsRequestedEventArgs>(Results_SuggestionsRequested);
            //Q42.WinRT.Data.WebDataCache.ClearAll();
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false; // Turn off searching by typing from main window
        }

        private void Results_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            var queryText = args.QueryText.ToLower();
            var request = args.Request;

            List<string> suggestionList = new List<string>()
                {
                    "Pi",
                    "Pi2",
                    "Pumpkin Pie",
                    "Pineapples",
                    "Apple Pie",
                    "Brown Garden Snail",
                    "Doppler Effect",
                    "5",
                    "5 Factorial",
                    "y = 3x^2",
                    "y = 1/(3x)"
                };
            if (string.IsNullOrEmpty(queryText))
            {
                //MainPage.Current.NotifyUser("Use the search pane to submit a query", NotifyType.StatusMessage);
            }
            else
            {
                foreach (string suggestion in suggestionList)
                {
                    if (suggestion.ToLower().Contains(queryText))
                    {
                        // Add suggestion to Search Pane
                        request.SearchSuggestionCollection.AppendQuerySuggestion(suggestion);

                        // Break since the Search Pane can show at most 5 suggestions
                        if (request.SearchSuggestionCollection.Size >= 5)
                        {
                            break;
                        }
                    }
                }
            }

            if (request.SearchSuggestionCollection.Size > 0)
            {
                //MainPage.Current.NotifyUser("Suggestions provided for query: " + queryText, NotifyType.StatusMessage);
            }
            else
            {
                //MainPage.Current.NotifyUser("No suggestions provided for query: " + queryText, NotifyType.StatusMessage);
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a bindable collection of items to this.DefaultViewModel["Items"]
            /*string query = "china";
            HttpResponseMessage response = await httpClient.GetAsync("http://api.wolframalpha.com/v2/query?input=" + Uri.EscapeDataString(query) + "&appid=A83TWA-A46U8PAEWY&mag=1.6");

            response.EnsureSuccessStatusCode();
            XDocument xml = XDocument.Parse(await response.Content.ReadAsStringAsync());
            IEnumerable<Pod> podsRaw = from pod in xml.Descendants("pod") select new Pod(pod);
            pods = new PodCollection(podsRaw);
            this.DefaultViewModel["Items"] = pods;*/
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var text = e.Parameter as string;
            if (text != null)
            {
                QueryBox.Text = text;
            }

            base.OnNavigatedTo(e);
        }

        private void CycleImages(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (Results), "pi");
        }

        private bool focusedQueryBox = false;
        private void AddCharacter(object sender, RoutedEventArgs e)
        {
            var character = sender as Button;
            
            QueryBox.Text += character.Content;

            
            if (focusedQueryBox)
            {
                QueryBox.Focus(FocusState.Pointer);
                QueryBox.Select(QueryBox.Text.Length, 0);
            }
        }

        private void AddCharacter(object sender, ItemClickEventArgs e)
        {
            
        }

        private void AppendCharacter(object sender, ItemClickEventArgs e)
        {
            var character = sender as TextBlock;
            QueryBox.Text += character.Text;
            QueryBox.Focus(FocusState.Pointer);
            QueryBox.Select(QueryBox.Text.Length, 0);
        }

        private void RunSearch(object sender, RoutedEventArgs e)
        {
            Q42.WinRT.Data.WebDataCache.ClearAll();
            RunSearch(QueryBox.Text);
        }

        private void SearchKeyDownHandler(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                RunSearch(QueryBox.Text);
            }
        }

        private void RunSearch(string query)
        {
            if (!String.IsNullOrEmpty(query))
            {
                Frame.Navigate(typeof (Results), QueryBox.Text);
            }
        }
    }
}