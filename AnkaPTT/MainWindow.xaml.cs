using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AnkaPTT.ViewModels;
using CefSharp;

namespace AnkaPTT
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel = new MainViewModel();

        PushFetcher pushFetcher = new PushFetcher();

        System.Timers.Timer _autoRefreshTimer = new System.Timers.Timer(Properties.Settings.Default.AutoRefreshMs);

        LoadPageModes _loadPageMode;

        public MainWindow()
        {
            InitializeComponent();
            viewModel.Dispatcher = Dispatcher;
            viewModel.WebBrowser = wb_main;
            DataContext = viewModel;
            pushFetcher.PushFetched += PushFetcher_PushFetched;
            Title = $"AnkaPTT {GetType().Assembly.GetName().Version}";

            _loadPageMode = (LoadPageModes)Properties.Settings.Default.LoadPageMode;

            _autoRefreshTimer.Elapsed += (sender, e) => Reload();
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            _autoRefreshTimer.Dispose();
            base.OnClosing(e);
        }


        private void PushFetcher_PushFetched(object sender, PushFetchedEventArgs e)
        {
            if (e.FatalError == false)
            {
                viewModel.UpdatePushes(e.Pushes);
            }
            else if (e.FatalError == true)
            {
                wb_main.GetBrowser().Reload();
            }
        }

        private async void txt_url_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                await LoadPage();
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPage();
        }

        private void wb_main_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain) pushFetcher.Start(e.Browser);
        }

        private void wb_main_FrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                pushFetcher.Stop();
                UiThreadRunSync(viewModel.AllPushCollection.Clear);
            }

        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                var clickedItem = ((FrameworkElement)e.OriginalSource).DataContext as ViewModels.PushViewModel;
                if (clickedItem != null)
                {
                    wb_main.GetMainFrame().EvaluateScriptAsync($"selectPush({clickedItem.Index})");
                }
            }
        }

        // it's OK on non-UI-thread
        private string GetCurrentUrl() => wb_main.GetMainFrame().Url;

        private async void Reload()
        {
            var url = GetCurrentUrl();
            if (url != "")
                await LoadPage(url);
        }

        private async Task LoadPage()
        {
            await LoadPage(txt_url.Text);
        }

        private async Task LoadPage(string url)
        {
            if (_loadPageMode == LoadPageModes.Browser)
                wb_main.Load(url);
            else
            {
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    string html;
                    try
                    {
                        html = await httpClient.GetStringAsync(url);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    compareAndLoadHtml(html, url);
                }
            }
        }

        string _checkData;  // it may be whole html or pollurl
        static System.Text.RegularExpressions.Regex _pollurlRegex = 
            new System.Text.RegularExpressions.Regex("data-pollurl=\"(?<pollurl>[^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled);

        private void compareAndLoadHtml(string html, string url)
        {
            string newCheckData = string.Empty;
            bool alwaysLoad = (GetCurrentUrl() != url);
            if (_loadPageMode.HasFlag(LoadPageModes.ComparePollurl))
            {
                var m = _pollurlRegex.Match(html);
                if (m.Success)
                {
                    newCheckData = m.Groups["pollurl"].Value;
                }
                else
                {
                    alwaysLoad = true;
                }

            }
            else if (_loadPageMode.HasFlag(LoadPageModes.CompareWholePage))
            {
                newCheckData = html;
            }
            else
            {
                alwaysLoad = true;
            }

            if(alwaysLoad || newCheckData != _checkData)
            {
                UiThreadRunAsync(() => wb_main.LoadHtml(html, url));
                _checkData = newCheckData;
            }
        }

        /// <summary>
        /// Runs the specific Action on the Dispatcher in an async fashion
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority.</param>
        private void UiThreadRunAsync(Action action, DispatcherPriority priority = DispatcherPriority.DataBind)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else if (!Dispatcher.HasShutdownStarted)
            {
                Dispatcher.BeginInvoke(action, priority);
            }
        }

        /// <summary>
        /// Runs the specific Action on the Dispatcher in an sync fashion
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority.</param>
        private void UiThreadRunSync(Action action, DispatcherPriority priority = DispatcherPriority.DataBind)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else if (!Dispatcher.HasShutdownStarted)
            {
                Dispatcher.Invoke(action, priority);
            }
        }



        private void AutoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            _autoRefreshTimer.Start();
        }

        private void AutoRefresh_Unchecked(object sender, RoutedEventArgs e)
        {
            _autoRefreshTimer.Stop();
        }
    }
}
