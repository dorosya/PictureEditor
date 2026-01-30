using Microsoft.Win32;
using PictureEditor.Models;
using PictureEditor.Service;
using PictureEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PictureEditor.WPF.Views
{
    public partial class MainWindow : Window
    {
        private enum EditMode
        {
            None,
            Crop,
            Pixel
        }

        private readonly List<Photo> photos = new List<Photo>();
        private readonly Filters filters = new Filters();
        private readonly ImageEditor editor = new ImageEditor();
        private readonly CollageMaker collageMaker = new CollageMaker();

        private EditMode _mode = EditMode.None;
        private bool _isSelecting = false;
        private Point _dragStartUi;
        private Point _dragEndUi;
       

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
                    if (selected.BitmapImage == null) return;
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
                    if (selected.BitmapImage == null) return;
                    var result = filters.ApplyColorFilter(selected.BitmapImage,
                        dialog.RedOffset, dialog.GreenOffset, dialog.BlueOffset);
                    selected.SetImage(result);
                    PreviewImage.Source = selected.BitmapImage;
                }
            }
        }

        private void CropButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is not Photo selected || selected.BitmapImage == null)
            {
                MessageBox.Show("Сначала выберите изображение", "Обрезка");
                return;
            }

            _mode = EditMode.Crop;
            _isSelecting = false;
            SelectionRect.Visibility = Visibility.Collapsed;
            MessageBox.Show("Режим обрезки: зажмите ЛКМ и выделите прямоугольник на фото.", "Обрезка");
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
            if (PhotosListBox.SelectedItem is not Photo selected || selected.BitmapImage == null)
            {
                MessageBox.Show("Сначала выберите изображение", "Пиксель");
                return;
            }

            _mode = EditMode.Pixel;
            SelectionRect.Visibility = Visibility.Collapsed;
            MessageBox.Show("Режим пикселя: кликните по изображению, чтобы выбрать пиксель и изменить его цвет.", "Пиксель");
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosListBox.SelectedItem is not Photo selected || selected.BitmapImage == null)
            {
                MessageBox.Show("Сначала выберите изображение", "Текст");
                return;
            }

            var dialog = new TextDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                var result = new TextService().AddText(
                    selected.BitmapImage,
                    dialog.TextValue,
                    dialog.X,
                    dialog.Y,
                    dialog.FontSizeValue,
                    dialog.TextColor);

                selected.SetImage(result);
                PreviewImage.Source = selected.BitmapImage;
            }
        }

        private void PreviewImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PhotosListBox.SelectedItem is not Photo selected || selected.BitmapImage == null)
                return;

            var pos = e.GetPosition(PreviewImage);

            if (_mode == EditMode.Crop)
            {
                _isSelecting = true;
                _dragStartUi = pos;
                _dragEndUi = pos;
                UpdateSelectionRectUi(_dragStartUi, _dragEndUi);
                SelectionRect.Visibility = Visibility.Visible;
                PreviewImage.CaptureMouse();
            }
            else if (_mode == EditMode.Pixel)
            {
                var px = TryMapUiPointToPixel(pos, selected.BitmapImage);
                if (px == null)
                {
                    MessageBox.Show("Клик вне изображения", "Пиксель");
                    return;
                }

                var (x, y) = px.Value;
                var current = GetPixelColor(selected.BitmapImage, x, y);
                var dlg = new PixelDialog(current) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    var edited = new PixelEditor().SetPixel(selected.BitmapImage, x, y, dlg.SelectedColor);
                    selected.SetImage(edited);
                    PreviewImage.Source = selected.BitmapImage;
                }

                _mode = EditMode.None;
            }
        }

        private void PreviewImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isSelecting || _mode != EditMode.Crop)
                return;

            _dragEndUi = e.GetPosition(PreviewImage);
            UpdateSelectionRectUi(_dragStartUi, _dragEndUi);
        }

        private void PreviewImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_isSelecting || _mode != EditMode.Crop)
                return;

            if (PhotosListBox.SelectedItem is not Photo selected || selected.BitmapImage == null)
                return;

            _isSelecting = false;
            PreviewImage.ReleaseMouseCapture();

            var startPx = TryMapUiPointToPixel(_dragStartUi, selected.BitmapImage);
            var endPx = TryMapUiPointToPixel(_dragEndUi, selected.BitmapImage);
            if (startPx == null || endPx == null)
            {
                SelectionRect.Visibility = Visibility.Collapsed;
                MessageBox.Show("Выделение вне изображения", "Обрезка");
                return;
            }

            int x1 = Math.Min(startPx.Value.x, endPx.Value.x);
            int y1 = Math.Min(startPx.Value.y, endPx.Value.y);
            int x2 = Math.Max(startPx.Value.x, endPx.Value.x);
            int y2 = Math.Max(startPx.Value.y, endPx.Value.y);

            int w = x2 - x1;
            int h = y2 - y1;

            if (w < 2 || h < 2)
            {
                SelectionRect.Visibility = Visibility.Collapsed;
                MessageBox.Show("Слишком маленькая область", "Обрезка");
                return;
            }

            var answer = MessageBox.Show($"Обрезать область {w}x{h} px?", "Обрезка", MessageBoxButton.YesNo);
            if (answer == MessageBoxResult.Yes)
            {
                var cropped = new CropService().Crop(selected.BitmapImage, x1, y1, w, h);
                selected.SetImage(cropped);
                PreviewImage.Source = selected.BitmapImage;
            }

            SelectionRect.Visibility = Visibility.Collapsed;
            _mode = EditMode.None;
        }

        private void UpdateSelectionRectUi(Point a, Point b)
        {
            double x = Math.Min(a.X, b.X);
            double y = Math.Min(a.Y, b.Y);
            double w = Math.Abs(a.X - b.X);
            double h = Math.Abs(a.Y - b.Y);

            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = w;
            SelectionRect.Height = h;
        }

        private (int x, int y)? TryMapUiPointToPixel(Point uiPoint, BitmapSource bitmap)
        {
            // PreviewImage stretches uniformly, centered.
            double controlW = PreviewImage.ActualWidth;
            double controlH = PreviewImage.ActualHeight;
            if (controlW <= 1 || controlH <= 1) return null;

            double imgW = bitmap.PixelWidth;
            double imgH = bitmap.PixelHeight;

            double scale = Math.Min(controlW / imgW, controlH / imgH);
            double shownW = imgW * scale;
            double shownH = imgH * scale;
            double offsetX = (controlW - shownW) / 2.0;
            double offsetY = (controlH - shownH) / 2.0;

            double xIn = uiPoint.X - offsetX;
            double yIn = uiPoint.Y - offsetY;
            if (xIn < 0 || yIn < 0 || xIn > shownW || yIn > shownH)
                return null;

            int px = (int)Math.Floor(xIn / scale);
            int py = (int)Math.Floor(yIn / scale);

            px = Math.Max(0, Math.Min(bitmap.PixelWidth - 1, px));
            py = Math.Max(0, Math.Min(bitmap.PixelHeight - 1, py));
            return (px, py);
        }

        private Color GetPixelColor(BitmapSource source, int x, int y)
        {
            // Read 1 pixel, assume BGRA32-compatible output.
            var formatted = source.Format == PixelFormats.Bgra32 ? source : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            byte[] px = new byte[4];
            formatted.CopyPixels(new Int32Rect(x, y, 1, 1), px, 4, 0);
            return Color.FromArgb(px[3], px[2], px[1], px[0]);
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
                    var bitmaps = photos.Select(p => p.BitmapImage).Where(b => b != null).Select(b => b!).ToList();
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

        private TextBox? redBox, greenBox, blueBox;

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
                if (redBox == null || greenBox == null || blueBox == null)
                {
                    MessageBox.Show("Не удалось инициализировать поля ввода цвета", "Ошибка");
                    return;
                }

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

    public class TextDialog : Window
    {
        public string TextValue { get; private set; } = "";
        public int X { get; private set; }
        public int Y { get; private set; }
        public int FontSizeValue { get; private set; } = 24;
        public Color TextColor { get; private set; } = Colors.Red;

        public TextDialog()
        {
            Width = 360;
            Height = 330;
            Title = "Добавить текст";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var grid = new Grid { Margin = new Thickness(15) };
            for (int i = 0; i < 7; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var tbText = new TextBox { Margin = new Thickness(0, 5, 0, 10) };
            AddRow(grid, 0, "Текст:", tbText);

            var tbX = new TextBox { Text = "10", Width = 80 };
            AddRow(grid, 1, "X (px):", tbX);

            var tbY = new TextBox { Text = "10", Width = 80 };
            AddRow(grid, 2, "Y (px):", tbY);

            var tbSize = new TextBox { Text = "24", Width = 80 };
            AddRow(grid, 3, "Размер шрифта:", tbSize);

            // Color RGB
            var spColor = new StackPanel { Orientation = Orientation.Horizontal };
            var tbR = new TextBox { Text = "255", Width = 50, Margin = new Thickness(0, 0, 5, 0) };
            var tbG = new TextBox { Text = "0", Width = 50, Margin = new Thickness(0, 0, 5, 0) };
            var tbB = new TextBox { Text = "0", Width = 50 };
            spColor.Children.Add(new TextBlock { Text = "R", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 3, 0) });
            spColor.Children.Add(tbR);
            spColor.Children.Add(new TextBlock { Text = "G", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 3, 0) });
            spColor.Children.Add(tbG);
            spColor.Children.Add(new TextBlock { Text = "B", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 3, 0) });
            spColor.Children.Add(tbB);
            AddRow(grid, 4, "Цвет (RGB):", spColor);

            var btn = new Button
            {
                Content = "Применить",
                Width = 110,
                Margin = new Thickness(0, 15, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(btn, 6);
            Grid.SetColumnSpan(btn, 2);
            grid.Children.Add(btn);

            btn.Click += (_, __) =>
            {
                if (!int.TryParse(tbX.Text, out int x) || !int.TryParse(tbY.Text, out int y))
                {
                    MessageBox.Show("X и Y должны быть числами", "Ошибка");
                    return;
                }

                if (!int.TryParse(tbSize.Text, out int fs) || fs <= 0)
                {
                    MessageBox.Show("Размер шрифта должен быть положительным числом", "Ошибка");
                    return;
                }

                if (!byte.TryParse(tbR.Text, out byte r) || !byte.TryParse(tbG.Text, out byte g) || !byte.TryParse(tbB.Text, out byte b))
                {
                    MessageBox.Show("RGB должны быть в диапазоне 0..255", "Ошибка");
                    return;
                }

                TextValue = tbText.Text ?? "";
                X = x;
                Y = y;
                FontSizeValue = fs;
                TextColor = Color.FromRgb(r, g, b);

                DialogResult = true;
                Close();
            };

            Content = grid;
        }

        private static void AddRow(Grid grid, int row, string label, UIElement element)
        {
            if (grid.ColumnDefinitions.Count == 0)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            var tb = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(tb, row);
            Grid.SetColumn(tb, 0);
            grid.Children.Add(tb);

            Grid.SetRow(element, row);
            Grid.SetColumn(element, 1);
            if (element is FrameworkElement fe) fe.Margin = new Thickness(0, 5, 0, 5);
            grid.Children.Add(element);
        }
    }

    public class PixelDialog : Window
    {
        public Color SelectedColor { get; private set; }

        public PixelDialog(Color current)
        {
            SelectedColor = current;
            Width = 320;
            Height = 260;
            Title = "Изменить пиксель";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var panel = new StackPanel { Margin = new Thickness(15) };

            panel.Children.Add(new TextBlock
            {
                Text = $"Текущий цвет: A={current.A}, R={current.R}, G={current.G}, B={current.B}",
                Margin = new Thickness(0, 0, 0, 10)
            });

            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            var tbR = new TextBox { Text = current.R.ToString(), Width = 60, Margin = new Thickness(0, 0, 8, 0) };
            var tbG = new TextBox { Text = current.G.ToString(), Width = 60, Margin = new Thickness(0, 0, 8, 0) };
            var tbB = new TextBox { Text = current.B.ToString(), Width = 60 };
            sp.Children.Add(new TextBlock { Text = "R", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) });
            sp.Children.Add(tbR);
            sp.Children.Add(new TextBlock { Text = "G", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 4, 0) });
            sp.Children.Add(tbG);
            sp.Children.Add(new TextBlock { Text = "B", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 4, 0) });
            sp.Children.Add(tbB);
            panel.Children.Add(sp);

            var preview = new Border { Height = 30, CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 0, 10) };
            panel.Children.Add(preview);

            void UpdatePreview()
            {
                if (byte.TryParse(tbR.Text, out byte r) && byte.TryParse(tbG.Text, out byte g) && byte.TryParse(tbB.Text, out byte b))
                {
                    SelectedColor = Color.FromRgb(r, g, b);
                    preview.Background = new SolidColorBrush(SelectedColor);
                }
            }

            tbR.TextChanged += (_, __) => UpdatePreview();
            tbG.TextChanged += (_, __) => UpdatePreview();
            tbB.TextChanged += (_, __) => UpdatePreview();
            UpdatePreview();

            var btn = new Button { Content = "Применить", Width = 110, HorizontalAlignment = HorizontalAlignment.Center };
            btn.Click += (_, __) =>
            {
                if (!byte.TryParse(tbR.Text, out byte r) || !byte.TryParse(tbG.Text, out byte g) || !byte.TryParse(tbB.Text, out byte b))
                {
                    MessageBox.Show("RGB должны быть в диапазоне 0..255", "Ошибка");
                    return;
                }
                SelectedColor = Color.FromRgb(r, g, b);
                DialogResult = true;
                Close();
            };
            panel.Children.Add(btn);

            Content = panel;
        }
    }
}