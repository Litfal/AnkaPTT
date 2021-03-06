﻿using System;
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
using HtmlAgilityPack;
using System.Net;
using System.Net.Http;

namespace AnkaPTT
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel = new MainViewModel();

        PushFetcher _pushFetcher = new PushFetcher();

        System.Timers.Timer _autoRefreshTimer = new System.Timers.Timer(Properties.Settings.Default.AutoRefreshMs);

        LoadPageModes _loadPageMode;

        public MainWindow()
        {
            InitializeComponent();
            wb_main.MenuHandler = new CustomMenuHandler();
            viewModel.Dispatcher = Dispatcher;
            viewModel.WebBrowser = wb_main;
            DataContext = viewModel;
            _pushFetcher.PushFetched += PushFetcher_PushFetched;
            Title = $"AnkaPTT {GetType().Assembly.GetName().Version}";

            _loadPageMode = (LoadPageModes)Properties.Settings.Default.LoadPageMode;

            _autoRefreshTimer.Elapsed += (sender, e) => Reload();
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            _autoRefreshTimer.Dispose();
            _pushFetcher.Stop();
            // close others filter window
            foreach (Window win in App.Current.Windows)
            {
                if (win != this) win.Close();
            }
            base.OnClosing(e);
        }

        private void PushFetcher_PushFetched(object sender, PushFetchedEventArgs e)
        {
            if (e.FatalError == false)
            {
                viewModel.UpdatePushes(e.Pushes);
            }
            else if (e.FatalError == true && Properties.Settings.Default.ReloadOnAutoFatalPushError)
            {
                Reload(true);
            }
        }

        private async void txt_url_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                await LoadPage(true);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPage(true);
        }

        private void wb_main_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain) _pushFetcher.Start(e.Browser);
        }

        private void wb_main_FrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                _pushFetcher.Stop();
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

        private async void Reload(bool force = false)
        {
            var url = GetCurrentUrl();
            if (url != "")
                await LoadPage(url, force);
        }

        private async Task LoadPage(bool force = false)
        {
            await LoadPage(txt_url.Text, force);
        }

        private async Task LoadPage(string url, bool force = false)
        {
            if (_loadPageMode == LoadPageModes.Browser)
                wb_main.Load(url);
            else
            {
                string html = null;
                var cookieContainer = new CookieContainer();
                // cookieContainer.Add(new System.Net.Cookie("over18", "1", "/", "www.ptt.cc"));
                // it's work but I want to do a question message
                // MessageBox.Show("", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                var clienthandler = new HttpClientHandler { AllowAutoRedirect = true, UseCookies = true, CookieContainer = cookieContainer };
                using (HttpClient httpClient = new HttpClient(clienthandler))
                {
                    try
                    {
                        var result = await httpClient.GetAsync(url);
                        //result.Content.Headers.ContentType.MediaType
                        if (result.IsSuccessStatusCode &&
                            System.Globalization.CompareInfo.GetCompareInfo("zh-TW")
                            .IndexOf(result.Content.Headers.ContentType.MediaType, "htm", System.Globalization.CompareOptions.IgnoreCase) >= 0)
                        {
                            html = await result.Content.ReadAsStringAsync();
                        }
                        else LoadUrl(url);

                        // html = await httpClient.GetStringAsync(url);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                if (html != null)
                    compareAndLoadHtml(html, url, force);
            }
        }

        string _checkData;  // it may be whole html or pollurl
        static System.Text.RegularExpressions.Regex _pollurlRegex = 
            new System.Text.RegularExpressions.Regex("data-pollurl=\"(?<pollurl>[^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled);

        string cacheKey;
        int offset;

        private void compareAndLoadHtml(string html, string url, bool force = false)
        {
            string newCheckData = string.Empty;
            bool alwaysLoad = force || (GetCurrentUrl() != url);
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
                LoadHtmlAsync(html, url, newCheckData, force);

            }
        }


        private Task LoadHtmlAsync(string html, string url, string newCheckData, bool force)
        {
            return Task.Run(()=> LoadHtml(html, url, newCheckData, force));
        }

        private void LoadHtml(string html, string url, string newCheckData, bool force)
        {
            _checkData = newCheckData;
            if (_loadPageMode.HasFlag(LoadPageModes.ComparePollurl))
            {
                try
                {
                    // parse pollurl 
                    int questionIdx = newCheckData.IndexOf("?");
                    if (questionIdx >= 0 && newCheckData.Length > questionIdx)
                    {
                        string query = newCheckData.Substring(questionIdx + 1);
                        var queries = System.Web.HttpUtility.ParseQueryString(System.Web.HttpUtility.HtmlDecode(query));
                        // set offset & cacheKey
                        int newOffset = int.Parse(queries["offset"]);
                        System.Diagnostics.Debug.WriteLine(newOffset);
                        string newCacheKey = queries["cacheKey"];
                        if (force || cacheKey != newCacheKey) // cacheKey is changed after edited
                        {
                            System.Diagnostics.Debug.WriteLine("LoadHtml full page");
                            LoadWholeHtml(html, url);
                        }
                        else if (offset != newOffset)
                        {
                            // guest new push = (newOffset - offset) / 102
                            System.Diagnostics.Debug.WriteLine("Add Pushes");
                            AddPushes(html, (newOffset - offset) / 102);
                        }
                        offset = newOffset;
                        cacheKey = newCacheKey;
                    }
                    else
                        LoadWholeHtml(html, url);
                }
                catch (Exception ex)
                {
                    LoadWholeHtml(html, url);
                }
            }
            else
                LoadWholeHtml(html, url);
        }

        private void LoadWholeHtml(string html, string url)
            => UiThreadRunAsync(() => wb_main.LoadHtml(html, url));

        private void LoadUrl(string url)
            => UiThreadRunAsync(() => wb_main.Load(url));

        private void AddPushes(string html, int newPushCount)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            DecodeDocument_cfEmail(document);

            var pushNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'push')]");
            if (pushNodes != null)
            {
                StringBuilder prarmeter = new StringBuilder("[");
                for (int i = pushNodes.Count - newPushCount; i < pushNodes.Count; i++)
                {
                    var node = pushNodes[i];
                    prarmeter.Append("'");
                    prarmeter.Append(System.Web.HttpUtility.JavaScriptStringEncode(node.OuterHtml.Trim()));
                    // check NextSibling is div.richcontent 
                    // it's yes when push contains youtube, imgur or etc link
                    // append the richcontent to push
                    var nextNode = node.NextSibling;
                    if (nextNode?.HasClass("richcontent") == true)
                        prarmeter.Append(System.Web.HttpUtility.JavaScriptStringEncode(nextNode.OuterHtml.Trim()));
                    prarmeter.Append("',");
                }
                if (prarmeter.Length > 1) prarmeter[prarmeter.Length - 1] = ']';
                wb_main.GetMainFrame().EvaluateScriptAsync($"addPushes({prarmeter})");
            }

            #region *OLD* update whole page
            //var mainNode = document.DocumentNode.SelectSingleNode("//div[@id='main-container']");
            // covert to base64
            //string encoded = System.Web.HttpUtility.JavaScriptStringEncode(mainNode.InnerHtml.Trim());
            //wb_main.GetMainFrame().EvaluateScriptAsync($"updateMainContent('{encoded}')");
            #endregion

            _pushFetcher.FetchOnce(wb_main.GetBrowser());
        }

        private void DecodeDocument_cfEmail(HtmlDocument document)
        {
            var encodeEmailNodes = document.DocumentNode.SelectNodes("//a[@class='__cf_email__']");
            if (encodeEmailNodes == null) return;
            for (int i = 0; i < encodeEmailNodes.Count; i++)
            {
                var node = encodeEmailNodes[i];
                var cfemail = node.GetAttributeValue("data-cfemail", "");
                var decoded = Decode_cfEmail(cfemail);
                var newNode = HtmlNode.CreateNode(decoded);
                node.ParentNode.ReplaceChild(newNode, node);
            }
        }

        public static string Decode_cfEmail(string encodedString)
        {
            StringBuilder email = new StringBuilder();
            int r = Convert.ToInt32(encodedString.Substring(0, 2), 16), n, i;
            for (n = 2; encodedString.Length - n > 0; n += 2)
            {
                i = Convert.ToInt32(encodedString.Substring(n, 2), 16) ^ r;
                char character = (char)i;
                email.Append(Convert.ToString(character));
            }

            return email.ToString();
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

        private void BtnOpenNewFilterWindow_Click(object sender, RoutedEventArgs e)
        {
            var newFilterViewModel = viewModel.CreateNewFilterViewModel();
            if(newFilterViewModel == null)
            {
                MessageBox.Show("篩選視窗數量已達上限","AnkaPTT");
                return;
            }
            PushFilterViewWindow newWindow = new PushFilterViewWindow();
            newWindow.DataContext = newFilterViewModel;
            newWindow.Show();
        }
    }
}
