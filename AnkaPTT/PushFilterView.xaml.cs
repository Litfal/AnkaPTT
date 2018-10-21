using AnkaPTT.ViewModels;
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

namespace AnkaPTT
{
    /// <summary>
    /// PushFilterView.xaml 的互動邏輯
    /// </summary>
    public partial class PushFilterView : UserControl
    {
        public PushFilterView()
        {
            InitializeComponent();
        }

        private FilterViewModel GetFilterViewModel()
        {
            return DataContext as FilterViewModel;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var clickedItem = ((FrameworkElement)e.OriginalSource).DataContext as PushViewModel;
                if (clickedItem != null)
                {
                    GetFilterViewModel()?.OnPushDoubleClicked(clickedItem);
                }
            }
        }

        private void ContainsText_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression be = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();
            }
        }
    }
}
