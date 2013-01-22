using Callisto.Controls;
using SearchulatorGrid.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using SearchulatorGrid.Pods;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace SearchulatorGrid
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class DetailPopup : SearchulatorGrid.Common.LayoutAwarePage
    {
        public DetailPopup()
        {
            InitializeComponent();
        }

        public Pod SelectedPod
        {
            get { return GetValue(PodProperty) as Pod; }
            set { SetValue(PodProperty, value); }
        }

        public static readonly DependencyProperty PodProperty =
            DependencyProperty.Register("SelectedPod", typeof(Pod),
                                        typeof(LayoutAwarePage), null);

        private const string COPY_IMAGE = "Copy as image";
        private const string COPY_PLAIN = "Copy as plaintext";
        private const string COPY_URL = "Copy image URL";
        private const string OPEN_URL = "Open image in browser";

        private Menu CopyMenu
        {
            get
            {
                var copyMenu = new Menu();

                var mi = new MenuItem();
                mi.Tag = "Plaintext";
                mi.Tapped += CopyCommands;
                mi.Text = COPY_PLAIN;
                mi.MenuTextMargin = new Thickness(28, 10, 28, 12);

                var mi2 = new MenuItem();
                mi2.Text = COPY_IMAGE;
                mi2.Tag = "Image";
                mi2.Tapped += CopyCommands;
                mi2.MenuTextMargin = new Thickness(28, 10, 28, 12);

                var mi3 = new MenuItem();
                mi3.Text = COPY_URL;
                mi3.Tag = "URL";
                mi3.Tapped += CopyCommands;
                mi3.MenuTextMargin = new Thickness(28, 10, 28, 12);



                copyMenu.Items.Add(mi);
                copyMenu.Items.Add(mi2);
                copyMenu.Items.Add(mi3);
                return copyMenu;
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
            SelectedPod = navigationParameter as Pod;

            if (SelectedPod != null && SelectedPod.NumSubpods == 1)
            {
                ImageOptionsAppBar.IsSticky = true;
                ImageOptionsAppBar.IsOpen = true;
            }
        }

        private static FrameworkElement FindByName(string name, FrameworkElement root)
        {
            var tree = new Stack<FrameworkElement>();
            tree.Push(root);

            while (tree.Count > 0)
            {
                FrameworkElement current = tree.Pop();
                if (current.Name == name)
                    return current;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(current, i);
                    if (child is FrameworkElement)
                        tree.Push((FrameworkElement)child);
                }
            }

            return null;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void CopyMenu_Click(object sender, RoutedEventArgs e)
        {
            Flyout f = new Flyout();
            f.PlacementTarget = sender as UIElement;
            f.Placement = PlacementMode.Top;
            
            f.HostMargin = new Thickness(0); // on menu flyouts set HostMargin to 0
            f.Content = CopyMenu;
            f.IsOpen = true;
        }

        private async void CopyCommands(object sender, TappedRoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;

            var image = SelectedPod.CurrentImage;

            var uri = await Q42.WinRT.Data.WebDataCache.GetLocalUriAsync(new Uri(image.URL));
            
            var m = sender as MenuItem;
            switch(m.Text)
            {
                case COPY_IMAGE:
                    dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromUri(uri));
                    break;
                case COPY_PLAIN:
                    dataPackage.SetText(image.Alt);
                    break;
                case COPY_URL:
                    dataPackage.SetText(image.URL);
                    break;
            }

            Clipboard.SetContent(dataPackage);
        }

        private async void OpenInBrowser(object sender, RoutedEventArgs e)
        {
            ImageResult img = SelectedPod.CurrentImage;
            await OpenImageInBrowser(img);
        }

        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (resultsListView.SelectedItem == null)
            {
                ImageOptionsAppBar.IsSticky = false;
                ImageOptionsAppBar.IsOpen = false;
            }
            else
            {
                ImageOptionsAppBar.IsSticky = true;
                ImageOptionsAppBar.IsOpen = true;
            }
        }

        private void ShowImageMenu_RightTap(object sender, RightTappedRoutedEventArgs e)
        {

            for (int i = 0; i < SelectedPod.Images.Count(); i++)
            {
                ListViewItem lvi =  resultsListView.ItemContainerGenerator.ContainerFromItem(SelectedPod.Images.ElementAt(i)) as ListViewItem ;
                if (sender == FindByName("img", lvi))
                {
                    resultsListView.SelectionMode = ListViewSelectionMode.Single;
                    resultsListView.SelectedIndex = i;
                    break;
                }
            }

            var f = new Flyout();
            f.PlacementTarget = e.OriginalSource as UIElement;
            f.Placement = PlacementMode.Top;


            f.HostMargin = new Thickness(0); // on menu flyouts set HostMargin to 0
            Menu m = CopyMenu;
            m.Items.Add(new MenuItemSeparator());

            var mi4 = new MenuItem();
            mi4.Text = OPEN_URL;
            mi4.Tag = "URL";
            mi4.Tapped += OpenURL;
            mi4.MenuTextMargin = new Thickness(28, 10, 28, 12);

            m.Items.Add(mi4);

            f.Content = m;
            f.IsOpen = true;

            e.Handled = true;
        }

        private async void OpenURL(object sender, TappedRoutedEventArgs e)
        {
            var img = SelectedPod.CurrentImage;
            bool success = await OpenImageInBrowser(img);
        }

        private static async System.Threading.Tasks.Task<bool> OpenImageInBrowser(ImageResult img)
        {
            var uri = new Uri(img.URL);
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);
            return success;
        }

        private async void ApplyState(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            State state = b.DataContext as State;
            await SelectedPod.Result.ApplyState(state);
            SelectedPod = (from pod in SelectedPod.Result.Pods where pod.Id == SelectedPod.Id select pod).First();

        }
    }
}
