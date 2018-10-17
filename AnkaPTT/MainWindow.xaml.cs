using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            viewModel.Dispatcher = Dispatcher;
            viewModel.WebBrowser = wb_main;
            DataContext = viewModel;
            pushFetcher.PushFetched += PushFetcher_PushFetched;
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

        private void txt_url_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (wb_main.Address == txt_url.Text)
                    wb_main.GetBrowser().Reload();
                else
                    wb_main.Address = txt_url.Text;
                // wb_main.RequestContext

            }
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
                Dispatcher.Invoke(viewModel.AllPushCollection.Clear);
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
    }
}
