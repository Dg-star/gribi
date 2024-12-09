using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using WpfApp1.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace WpfApp1.ViewModel
{
    // Класс команды, реализующий интерфейс ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        // Конструктор команды
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Проверка, можно ли выполнить команду
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Событие изменения возможности выполнения команды
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Выполнение команды
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    // Основной ViewModel для работы с грибами
    public class MainViewModel : INotifyPropertyChanged
    {
        // Событие уведомления об изменении свойства
        public event PropertyChangedEventHandler PropertyChanged;

        // Метод вызова события изменения свойства
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Команды для операций с грибами
        public ICommand AddNewMushroomCommand { get; set; }
        public ICommand DeleteMushroomCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        // Коллекция грибов
        private ObservableCollection<mushroom> _mushrooms;
        public ObservableCollection<mushroom> Mushrooms
        {
            get => _mushrooms;
            set
            {
                _mushrooms = value;
                OnPropertyChanged();
            }
        }

        // Выбранный гриб
        private mushroom _selectedMushroom;
        public mushroom SelectedMushroom
        {
            get => _selectedMushroom;
            set
            {
                _selectedMushroom = value;
                OnPropertyChanged();
            }
        }

        // Конструктор для инициализации ViewModel
        public MainViewModel()
        {
            Mushrooms = new ObservableCollection<mushroom>();
            SelectedMushroom = null;

            // Загрузка грибов из файла
            LoadMushroomsFromJson();

            // Инициализация команд
            SaveCommand = new RelayCommand(Save, CanSave);
            AddNewMushroomCommand = new RelayCommand(AddNewMushroom);
            DeleteMushroomCommand = new RelayCommand(DeleteMushroom, CanDelete);
        }

        // Условия для сохранения: выбран ли гриб
        private bool CanSave(object obj)
        {
            return SelectedMushroom != null;
        }

        // Условия для удаления: выбран ли гриб
        private bool CanDelete(object obj)
        {
            return SelectedMushroom != null;
        }

        // Загрузка списка грибов из JSON файла
        private void LoadMushroomsFromJson()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mushrooms.json");

                // Проверка существования файла
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Файл '{filePath}' не найден.");
                    return;
                }

                string jsonContent = File.ReadAllText(filePath);
                Console.WriteLine("Загружен JSON:\n" + jsonContent);

                // Десериализация JSON в список грибов
                var mushrooms = JsonConvert.DeserializeObject<List<mushroom>>(jsonContent);

                if (mushrooms != null)
                {
                    // Добавление каждого гриба в коллекцию
                    foreach (var mushroom in mushrooms)
                    {
                        // Проверка данных гриба на корректность
                        if (mushroom != null && mushroom.Id > 0 && !string.IsNullOrEmpty(mushroom.Name))
                        {
                            Mushrooms.Add(mushroom);
                            Console.WriteLine($"Загружен гриб: {mushroom.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"Пропущена запись из-за ошибки в данных: {mushroom}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Не удалось десериализовать грибы.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке грибов: {ex.Message}");
            }
        }

        // Добавление нового гриба в коллекцию
        private void AddNewMushroom(object parameter)
        {
            int newId = GetNextId();
            mushroom newMushroom = new mushroom
            {
                Id = newId,
                Name = "",
                Color = "",
                Edible = false,
                Weight = 0,
                Height = 0,
                CapRadius = 0,
            };

            Mushrooms.Add(newMushroom);
            SelectedMushroom = newMushroom;
            OnPropertyChanged(nameof(Mushrooms));  // Обновляем привязку
        }

        // Получение следующего ID для нового гриба
        private int GetNextId()
        {
            return Mushrooms.Any() ? Mushrooms.Max(m => m.Id) + 1 : 1;
        }

        // Удаление гриба из коллекции и файла
        private void DeleteMushroom(object parameter)
        {
            // Проверка, выбран ли гриб для удаления
            if (SelectedMushroom == null)
            {
                MessageBox.Show("Не выбран гриб для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Если гриб не выбран, прекращаем выполнение метода
            }

            try
            {
                // Сохраняем выбранный гриб для дальнейшей работы
                var mushroomToDelete = SelectedMushroom;

                // Удаляем гриб из коллекции
                Mushrooms.Remove(mushroomToDelete);

                // Обновляем путь к файлу
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mushrooms.json");

                // Загружаем существующие данные из файла
                List<mushroom> existingMushrooms = new List<mushroom>();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    existingMushrooms = JsonConvert.DeserializeObject<List<mushroom>>(json) ?? new List<mushroom>();
                }

                // Удаляем гриб из списка по ID
                var mushroomToDeleteFromFile = existingMushrooms.FirstOrDefault(m => m.Id == mushroomToDelete.Id);
                if (mushroomToDeleteFromFile != null)
                {
                    existingMushrooms.Remove(mushroomToDeleteFromFile);  // Удаляем гриб из списка
                }

                // Сериализуем обновленный список грибов и сохраняем в файл
                string updatedJson = JsonConvert.SerializeObject(existingMushrooms, Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);  // Перезаписываем файл

                // Выводим сообщение о успешном удалении
                MessageBox.Show($"Гриб '{mushroomToDelete.Name}' удален из списка и файла.", "Удаление успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                // Сбрасываем выбор гриба (это нужно делать после всех операций)
                SelectedMushroom = null;  // Сбрасываем SelectedMushroom только после всех операций
            }
            catch (Exception ex)
            {
                // Обрабатываем исключение при удалении
                MessageBox.Show($"Ошибка при удалении гриба: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Сохранение выбранного гриба в файл
        private void Save(object obj)
        {
            // Проверка, выбран ли гриб для сохранения
            if (SelectedMushroom == null)
            {
                MessageBox.Show("No mushroom selected to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string filePath = "mushrooms.json"; // Путь к файлу
            try
            {
                // Загрузка существующих грибов (если они есть)
                List<mushroom> existingMushrooms = null;
                string json; // Объявляем json переменную

                if (File.Exists(filePath))
                {
                    json = File.ReadAllText(filePath);
                    existingMushrooms = JsonConvert.DeserializeObject<List<mushroom>>(json);
                }

                // Добавляем или обновляем выбранный гриб в списке
                if (existingMushrooms != null)
                {
                    var existingMushroom = existingMushrooms.FirstOrDefault(m => m.Id == SelectedMushroom.Id);
                    if (existingMushroom != null)
                    {
                        // Если гриб уже существует, удаляем его перед добавлением нового
                        existingMushrooms.Remove(existingMushroom);
                    }

                    existingMushrooms.Add(SelectedMushroom);
                }
                else
                {
                    // Создаем новый список, если грибов еще нет
                    existingMushrooms = new List<mushroom>() { SelectedMushroom };
                }

                // Сериализуем и сохраняем обновленный список грибов
                json = JsonConvert.SerializeObject(existingMushrooms, Formatting.Indented);
                File.WriteAllText(filePath, json);

                // Уведомляем пользователя об успешном сохранении
                MessageBox.Show($"Mushroom '{SelectedMushroom.Name}' saved to {filePath}.");
            }
            catch (Exception ex)
            {
                // Обработка ошибок при сохранении
                MessageBox.Show($"Error saving mushroom: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
