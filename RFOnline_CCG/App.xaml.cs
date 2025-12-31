using System;
using System.Windows;

namespace RFOnline_CCG
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаем папку для сохранений если ее нет
            if (!System.IO.Directory.Exists("Saves"))
            {
                System.IO.Directory.CreateDirectory("Saves");
            }

            // Проверяем аргументы командной строки для автозагрузки
            if (e.Args.Length > 0 && System.IO.File.Exists(e.Args[0]))
            {
                // Можно добавить автозагрузку по двойному клику на файл сохранения
            }
        }
    }
}