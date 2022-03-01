using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input; // KeyBinding

namespace Caro.ViewModel
{
    public class MyKeyBinding : KeyBinding
    {
        public static readonly DependencyProperty CommandBindingProperty =
            DependencyProperty.Register("CommandBinding", typeof(ICommand),
            typeof(MyKeyBinding),
            new FrameworkPropertyMetadata(OnCommandBindingChanged));

        public ICommand CommandBinding
        {
            get { return (ICommand)GetValue(CommandBindingProperty); }
            set { SetValue(CommandBindingProperty, value); }
        }

        private static void OnCommandBindingChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var keyBinding = (MyKeyBinding)d;
            keyBinding.Command = (ICommand)e.NewValue;
        }
    }
}
