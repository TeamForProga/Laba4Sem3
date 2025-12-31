using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace RFOnline_CCG
{
    public partial class SaveGameDialog : Window
    {
        // Имя сохранения, введенное пользователем
        public string SaveName { get; private set; }

        public SaveGameDialog()
        {
            InitializeComponent();
            LoadExistingSaves();
            SaveNameTextBox.Focus();
            SaveNameTextBox.SelectAll();
        }

        // Загрузка списка существующих сохранений
        private void LoadExistingSaves()
        {
            try
            {
                if (!Directory.Exists("Saves"))
                    return;

                var saves = Directory.GetFiles("Saves", "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .OrderBy(name => name)
                    .ToList();

                ExistingSavesList.ItemsSource = saves;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки списка сохранений: {ex.Message}");
            }
        }

        // Обработка выбора сохранения из списка
        private void ExistingSavesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ExistingSavesList.SelectedItem is string selectedSave)
            {
                SaveNameTextBox.Text = selectedSave;
            }
        }

        // Сохранение игры с указанным именем
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveName = SaveNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(SaveName))
            {
                MessageBox.Show("Введите имя сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на наличие запрещенных символов в имени файла
            if (SaveName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Имя сохранения содержит недопустимые символы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        // Отмена сохранения
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}