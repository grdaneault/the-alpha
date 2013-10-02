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
using Q42.WinRT.Data;
using Windows.UI.Xaml.Media.Imaging;

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
                        tree.Push((FrameworkElement) child);
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
            switch (m.Text)
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
            /*
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
             * */
        }

        private void ShowImageMenu_RightTap(object sender, RightTappedRoutedEventArgs e)
        {
            ShowImageMenu(sender, e.OriginalSource as UIElement);
            e.Handled = true;
        }
        
        private void ShowImageMenu(object sender, UIElement placementTarget)
        {
            for (int i = 0; i < SelectedPod.Images.Count(); i++)
            {
                ListViewItem lvi = resultsListView.ItemContainerGenerator.ContainerFromItem(SelectedPod.Images.ElementAt(i)) as ListViewItem;
                if (sender == FindByName("img", lvi))
                {
                    resultsListView.SelectionMode = ListViewSelectionMode.Single;
                    resultsListView.SelectedIndex = i;
                    break;
                }
            }

            var f = new Flyout();
            f.PlacementTarget = placementTarget;
            f.Placement = PlacementMode.Right;


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
        }

        private async void OpenURL(object sender, TappedRoutedEventArgs e)
        {
            var img = SelectedPod.CurrentImage;
            bool success = await OpenImageInBrowser(img);
        }

        private static async System.Threading.Tasks.Task<bool> OpenImageInBrowser(ImageResult img)
        {
            return await OpenUriInBrowser(new Uri(img.URL));
        }

        private static async System.Threading.Tasks.Task<bool> OpenUriInBrowser(Uri uri)
        {
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);
            return success;
        }

        private async void ApplyState(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            State state = b.DataContext as State;
            await SelectedPod.Result.ApplyState(state);
            SelectedPod = (from pod in SelectedPod.Result.Pods where pod.Id == SelectedPod.Id select pod).First();
            
            _infoFlyout = null; // Force a refresh of the info popup in case there are any new ones
        }

        private Flyout _infoFlyout;
        private async void ShowInfo(object sender, RoutedEventArgs e)
        {
            if (_infoFlyout == null)
            {
                Flyout flyout = new Flyout();
                // creating some content
                // this could be just an instance of a UserControl of course
                Border border = new Border();
                ScrollViewer scroll = new ScrollViewer();
                scroll.MaxHeight = 600;
                scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                border.BorderThickness = new Thickness(5);
                border.Margin = new Thickness(-5);
                StackPanel infoPanel = new StackPanel();
                infoPanel.Orientation = Orientation.Vertical;

                Menu infoMenu = new Menu();
                foreach (var info in SelectedPod.Infos)
                {
                    if (info.HasImage)
                    {
                        ImageMenuItem imi = new ImageMenuItem();
                        imi.ImgSource = info.Image.URL;
                        imi.ImgWidth  = info.Image.Width;
                        imi.ImgHeight = info.Image.Height;
                        imi.Tag = info;
                        imi.Tapped += showInfoMenu;
                        imi.UpdateLayout();
                        infoMenu.Items.Add(imi);
                    }
                    else
                    {
                        if (infoMenu.Items.Count > 0)
                        {
                            infoMenu.Items.Add(new MenuItemSeparator());
                        }

                        MenuItem more = new MenuItem();
                        more.Text = info.Links[0].Text;
                        more.Tag = info.Links[0].Uri;
                        more.Tapped += infoMI_tapped;
                        infoMenu.Items.Add(more);
                    }
                }

                border.Child = scroll;
                scroll.Content = infoMenu;

                // set the Flyout content
                flyout.Content = border;
                //flyout.

                //b.Child = container;
                //filters.Content = b;
                flyout.PlacementTarget = ShowInfoBtn;
                flyout.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Top;
                flyout.IsOpen = true;
                //f.Background = new SolidColorBrush(Colors.Black);
                _infoFlyout = flyout;
            }
            else
            {
                _infoFlyout.IsOpen = true;
            }
        }

        private void showInfoMenu(object sender, TappedRoutedEventArgs e)
        {
            var imi = sender as ImageMenuItem;
            Info info = imi.Tag as Info;
            Menu menu = createInfoMenu(info);
            var f = new Flyout();
            f.PlacementTarget = e.OriginalSource as UIElement;
            f.Placement = PlacementMode.Right;
            f.Content = menu;
            f.HostMargin = new Thickness(0); // on menu flyout
            f.IsOpen = true;
        }

        private Menu createInfoMenu(Info info)
        {
            var menu = new Menu();

            foreach (var link in info.Links)
            {
                var mi = new MenuItem();
                mi.Tag = link.Uri;
                mi.Tapped += infoMI_tapped;
                mi.Text = link.Text;
                mi.MenuTextMargin = new Thickness(28, 10, 28, 12);
                menu.Items.Add(mi);
            }

            return menu;

        }

        private void infoMI_tapped(object sender, TappedRoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            OpenUriInBrowser(mi.Tag as Uri);
        }

        private void ShowImageMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowImageMenu(sender, e.OriginalSource as UIElement);
            e.Handled = true;
        }

        private void OnScrollViewerManipulated(object sender, ManipulationDeltaRoutedEventArgs e)
        {/*
            if (e.Delta.Scale != 0)
            {
                //e.Handled = true;
                var im = sender as Image;
                var imResult = im.DataContext as ImageResult;
                im.Height = imResult.Height * e.Delta.Scale + 10;
            }*/
        }

        private void OnScrollViewerManipulated(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            sv.Height = sv.ExtentHeight;
            Grid gv = sv.Parent as Grid;
            gv.Height = sv.ExtentHeight;
        }
    }
}