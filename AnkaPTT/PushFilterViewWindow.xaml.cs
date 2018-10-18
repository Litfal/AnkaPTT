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
using System.Windows.Shapes;

namespace AnkaPTT
{
    /// <summary>
    /// PushFilterViewWindow.xaml 的互動邏輯
    /// </summary>
    public partial class PushFilterViewWindow : Window
    {
        public PushFilterViewWindow()
        {
            InitializeComponent();

            DataContextChanged += PushFilterViewWindow_DataContextChanged;
        }

        private void PushFilterViewWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            filterView.DataContext = e.NewValue;
            
        }

        protected override void OnClosed(EventArgs e)
        {
            (filterView.DataContext as ViewModels.FilterViewModel).Dispose();
            base.OnClosed(e);
        }
    }
}
