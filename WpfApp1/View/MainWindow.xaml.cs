using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private bool isZPressed = false;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, какие клавиши нажаты
            if (e.Key == System.Windows.Input.Key.Z)
            {
                isZPressed = true;
            }

            if (isZPressed)
            {
                OpenConsole();
            }
        }
        private void OpenConsole()
        {
            // Открытие консоли вручную
            AllocConsole();
            Console.WriteLine("Консоль открыта!");

            // Сбрасываем флаги, чтобы не открывалась консоль снова при повторном нажатии этих клавиш
            isZPressed = false;
        }
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
    }
}
