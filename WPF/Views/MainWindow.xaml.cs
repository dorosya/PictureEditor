using Microsoft.Win32;
using PhotoEditor.Models;
using PhotoEditor.Services;
using PhotoEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoEditor.Wpf.Views
{
    public partial class MainWindow : Window
    {
        private readonly List<Photo> photos = new List<Photo>();
        private readonly Filters filters = new Filters();
        private readonly ImageEditor editor = new ImageEditor();
        private readonly CollageMaker collageMaker = new CollageMaker();
       

        public MainWindow()
        {
            InitializeComponent();
            LoadSavedPhotos();
            UpdatePhotosList();
        }

        private void LoadSavedPhotos()
        {
            var saved = StateManager.LoadState();
            foreach (var photo in saved)
            {
                try
                {
                    photo.LoadImage();
                    photos.Add(photo);
                }
                catch { /* ignore */ }
            }
        }

        private void UpdatePhotosList()
        {
            PhotosListBox.ItemsSource = null;
            PhotosListBox.ItemsSource = photos;

            NoImageText.Visibility = photos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            PreviewImage.Visibility = photos.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // === ОБРАБОТЧИКИ СОБЫТИЙ ===

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            AddPhotoButton_Click(sender, e);
        }

        private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp|Все файлы|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string filePath in dialog.FileNames)
                {
                    try
                    {
                        var photo = new Photo(filePath);
                        photo.LoadImage();
                        photos.Add(photo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка загрузки");
                    }
                }

                UpdatePhotosList();
                StateManager.SaveState(photos);
            }
        }

        private void RemovePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is Photo selected)
            {
                photos.Remove(selected);
                UpdatePhotosList();
                StateManager.SaveState(photos);
            }
        }

        private void PhotosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is Photo selected)
            {
                PreviewImage.Source = selected.BitmapImage;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is Photo selected)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "JPEG|*.jpg|PNG|*.png|BMP|*.bmp",
                    FileName = selected.Name
                };

                if (dialog.ShowDialog() == true)
                {
                    selected.SaveImage(dialog.FileName);
                    MessageBox.Show("Сохранено!", "Успех");
                }
            }
        }

        private void BrightnessButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is Photo selected)
            {
                var dialog = new BrightnessDialog();
                if (dialog.ShowDialog() == true)
                {
                    var result = filters.ApplyBrightness(selected.BitmapImage, dialog.BrightnessFactor);
                    selected.SetImage(result);
                    PreviewImage.Source = selected.BitmapImage;
                }
            }
        }

        private void ColorFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is Photo selected)
            {
                var dialog = new ColorFilterDialog();
                if (dialog.ShowDialog() == true)
                {
                    var result = filters.ApplyColorFilter(selected.BitmapImage,
                        dialog.RedOffset, dialog.GreenOffset, dialog.BlueOffset);
                    selected.SetImage(result);
                    PreviewImage.Source = selected.BitmapImage;
                }
            }
        }

        private void CropButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Выделите область для обрезки (в будущей версии)", "Обрезка");
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is Photo selected)
            {
                
                var dialog = new Window
                {
                    Width = 250,
                    Height = 200,
                    Title = "Поворот изображения",
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize
                };

                var stack = new StackPanel { Margin = new Thickness(20) };

                stack.Children.Add(new TextBlock
                {
                    Text = "Выберите угол:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var buttons = new[]
                {
            new { Angle = 90f, Text = "↻ На 90° вправо" },
            new { Angle = -90f, Text = "↺ На 90° влево" },
            new { Angle = 180f, Text = "🔄 Перевернуть (180°)" }
        };

                foreach (var btn in buttons)
                {
                    var button = new Button
                    {
                        Content = btn.Text,
                        Margin = new Thickness(0, 0, 0, 5),
                        Width = 180
                    };

                    button.Click += (s, args) =>
                    {
                        ApplyWpfRotation(selected, btn.Angle);
                        dialog.Close();
                    };

                    stack.Children.Add(button);
                }

                dialog.Content = stack;
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("Сначала выберите изображение", "Внимание");
            }
        }

        private void ApplyWpfRotation(Photo photo, float angle)
        {
            try
            {
                if (photo.BitmapImage == null)
                {
                    MessageBox.Show("Изображение не загружено", "Ошибка");
                    return;
                }

                // 1. Создаём трансформацию поворота
                var transform = new RotateTransform(angle);

                // 2. Применяем к BitmapImage
                var rotatedBitmap = new TransformedBitmap(photo.BitmapImage, transform);

                // 3. Конвертируем обратно в BitmapImage
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rotatedBitmap));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    // 4. Сохраняем в Photo
                    photo.SetImage(bitmapImage);

                    // 5. Обновляем отображение
                    PreviewImage.Source = photo.BitmapImage;

                    MessageBox.Show($"Изображение повернуто на {angle}°", "Готово");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поворота: {ex.Message}", "Ошибка");
            }
        }

        private void RotateImage(Photo photo, float angle)
        {
            try
            {
                // 1. Поворачиваем через ImageEditor
                editor.Rotate(photo, angle);

                // 2. Обновляем BitmapImage в Photo
                photo.LoadImage();

                // 3. Обновляем отображение
                PreviewImage.Source = photo.BitmapImage;

                MessageBox.Show($"Повернуто на {angle}°", "Готово");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка поворота");
            }
        }

        private void ShowCustomRotateDialog(Photo photo)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите угол поворота (градусы):", "Поворот", "45");

            if (float.TryParse(input, out float angle))
            {
                RotateImage(photo, angle);
            }
        }

        private void PixelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Нажмите на пиксель для изменения (в будущей версии)", "Изменение пикселя");
        }

        private void CollageButton_Click(object sender, RoutedEventArgs e)
        {
            if (photos.Count >= 2)
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PNG|*.png",
                    FileName = "collage.png"
                };

                if (dialog.ShowDialog() == true)
                {
                    var bitmaps = photos.Select(p => p.BitmapImage).ToList();
                    var collage = collageMaker.CreateCollage(bitmaps);

                    // Сохраняем коллаж
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(collage));
                    using (var stream = System.IO.File.Create(dialog.FileName))
                    {
                        encoder.Save(stream);
                    }

                    MessageBox.Show($"Коллаж сохранён: {dialog.FileName}", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Нужно минимум 2 фото для коллажа", "Ошибка");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            StateManager.SaveState(photos);
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            StateManager.SaveState(photos);
            base.OnClosed(e);
        }
    }

    // Простые диалоги (можно вынести в отдельные файлы)
    public class BrightnessDialog : Window
    {
        public float BrightnessFactor { get; private set; } = 1.0f;

        public BrightnessDialog()
        {
            Width = 300;
            Height = 150;
            Title = "Яркость";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var slider = new Slider { Minimum = 0.1, Maximum = 3.0, Value = 1.0, Margin = new Thickness(10) };
            var textBlock = new TextBlock { Text = "Коэффициент: 1.0", Margin = new Thickness(10) };
            var button = new Button { Content = "Применить", Width = 80, Margin = new Thickness(10) };

            slider.ValueChanged += (s, e) =>
            {
                BrightnessFactor = (float)slider.Value;
                textBlock.Text = $"Коэффициент: {BrightnessFactor:F2}";
            };

            button.Click += (s, e) => { DialogResult = true; Close(); };

            var panel = new StackPanel();
            panel.Children.Add(textBlock);
            panel.Children.Add(slider);
            panel.Children.Add(button);

            Content = panel;
        }
    }

    public class ColorFilterDialog : Window
    {
        public int RedOffset { get; private set; }
        public int GreenOffset { get; private set; }
        public int BlueOffset { get; private set; }

        private TextBox redBox, greenBox, blueBox;

        public ColorFilterDialog()
        {
            Width = 300;
            Height = 250;
            Title = "Цветовой фильтр";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Красный
            AddColorRow(grid, 0, "Красный (-255..255):", System.Windows.Media.Brushes.Red);
            // Зелёный
            AddColorRow(grid, 1, "Зелёный (-255..255):", System.Windows.Media.Brushes.Green);
            // Синий
            AddColorRow(grid, 2, "Синий (-255..255):", System.Windows.Media.Brushes.Blue);

            // Кнопка
            var button = new Button
            {
                Content = "Применить",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            button.Click += (s, e) =>
            {
                if (int.TryParse(redBox.Text, out int r) &&
                    int.TryParse(greenBox.Text, out int g) &&
                    int.TryParse(blueBox.Text, out int b))
                {
                    RedOffset = r;
                    GreenOffset = g;
                    BlueOffset = b;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Введите числа от -255 до 255!");
                }
            };

            Grid.SetRow(button, 3);
            grid.Children.Add(button);

            Content = grid;
        }

        private void AddColorRow(Grid grid, int row, string label, System.Windows.Media.Brush color)
        {
            var labelText = new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(labelText, row);
            Grid.SetColumn(labelText, 0);

            var textBox = new TextBox
            {
                Text = "0",
                Width = 60,
                Background = color,
                Opacity = 0.3,
                Foreground = System.Windows.Media.Brushes.Black
            };

            if (label.Contains("Красный")) redBox = textBox;
            else if (label.Contains("Зелёный")) greenBox = textBox;
            else if (label.Contains("Синий")) blueBox = textBox;

            Grid.SetRow(textBox, row);
            Grid.SetColumn(textBox, 1);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.Children.Add(labelText);
            grid.Children.Add(textBox);
        }


        private Slider CreateSlider(string label, int min, int max, int value)
        {
            return new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                Margin = new Thickness(10, 5, 10, 0),
                ToolTip = label
            };
        }
    }
}