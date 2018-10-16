using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnkaPTT.UI
{
    class DateTimeTextBox : TextBox
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(DateTime), typeof(DateTimeTextBox));
        public DateTime Value
        {
            get { return (DateTime)GetValue(ValueProperty); }
            set
            {
                var oldValue = Value;
                if (oldValue == value) return;
                SetValue(ValueProperty, value);
            }
        }

        private void OnValueChanged()
        {
            updateText();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ValueChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ValueProperty)
                OnValueChanged();
        }

        private void updateText()
        {
            if (IsFocused)
                Text = Value.ToString("yyyy/MM/dd HH:mm");
            else
                Text = Value.ToString("HH:mm");
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Value = Value.AddMinutes(1);
            }
            if (e.Key == Key.Down)
            {
                Value = Value.AddMinutes(-1);
            }
            
            base.OnPreviewKeyDown(e);
        }



        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            updateText();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            updateText();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            DateTime newDateTime;
            if (DateTime.TryParseExact(Text, "yyyy/MM/dd HH:mm",
                System.Globalization.CultureInfo.GetCultureInfo("zh-TW"), System.Globalization.DateTimeStyles.AssumeLocal,
                out newDateTime))
            {
                Value = newDateTime;
            }
        }
    }
}
