using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PictureEditor.Models;
using PictureEditor.Service;

namespace PictureEditor.WPF.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private readonly Filters _filters = new Filters();
		private readonly ImageEditor _editor = new ImageEditor();
		private readonly CollageMaker _collageMaker = new CollageMaker();

		public ObservableCollection<Photo> Photos { get; } = new ObservableCollection<Photo>();

		private Photo? _selectedPhoto;
		public Photo? SelectedPhoto
		{
			get => _selectedPhoto;
			set
			{
				_selectedPhoto = value;
				OnPropertyChanged(nameof(SelectedPhoto));
				OnPropertyChanged(nameof(IsPhotoSelected));
			}
		}

		public bool IsPhotoSelected => SelectedPhoto != null;

		// Команды
		public ICommand OpenCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand ApplyBrightnessCommand { get; }
		public ICommand ApplyColorFilterCommand { get; }

		public MainViewModel()
		{
			// Загружаем состояние при старте
			var savedPhotos = Utils.StateManager.LoadState();
			foreach (var photo in savedPhotos)
			{
				try
				{
					photo.LoadImage();
					Photos.Add(photo);
				}
				catch { /* игнорируем битые фото */ }
			}

			// Инициализируем команды
			OpenCommand = new RelayCommand(OpenPhotos);
			SaveCommand = new RelayCommand(SavePhoto, () => IsPhotoSelected);
			ApplyBrightnessCommand = new RelayCommand(ApplyBrightness, () => IsPhotoSelected);
			ApplyColorFilterCommand = new RelayCommand(ApplyColorFilter, () => IsPhotoSelected);
		}

		private void OpenPhotos()
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
						Photos.Add(photo);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Ошибка загрузки {filePath}: {ex.Message}");
					}
				}

				// Сохраняем состояние
				Utils.StateManager.SaveState(Photos.ToList());
			}
		}

		private void SavePhoto()
		{
			if (SelectedPhoto == null) return;

			SaveFileDialog dialog = new SaveFileDialog
			{
				Filter = "JPEG|*.jpg|PNG|*.png|BMP|*.bmp",
				FileName = SelectedPhoto.Name
			};

			if (dialog.ShowDialog() == true)
			{
				SelectedPhoto.SaveImage(dialog.FileName);
				MessageBox.Show("Сохранено!");
			}
		}

		private void ApplyBrightness()
		{
			if (SelectedPhoto?.BitmapImage == null) return;

			// Простой диалог для яркости
			string input = Microsoft.VisualBasic.Interaction.InputBox(
				"Введите коэффициент яркости (0.5 - темнее, 2.0 - ярче):",
				"Яркость",
				"1.0");

			if (float.TryParse(input, out float factor) && factor > 0)
			{
				var result = _filters.ApplyBrightness(SelectedPhoto.BitmapImage, factor);
				SelectedPhoto.SetImage(result);
				OnPropertyChanged(nameof(SelectedPhoto));
			}
		}

		private void ApplyColorFilter()
		{
			if (SelectedPhoto?.BitmapImage == null) return;

			// Простой диалог для цветового фильтра
			string input = Microsoft.VisualBasic.Interaction.InputBox(
				"Введите R G B через пробел (например: 10 -5 20):",
				"Цветовой фильтр",
				"0 0 0");

			var parts = input.Split(' ');
			if (parts.Length == 3 &&
				int.TryParse(parts[0], out int r) &&
				int.TryParse(parts[1], out int g) &&
				int.TryParse(parts[2], out int b))
			{
				var result = _filters.ApplyColorFilter(SelectedPhoto.BitmapImage, r, g, b);
				SelectedPhoto.SetImage(result);
				OnPropertyChanged(nameof(SelectedPhoto));
			}
		}

		// Событие для обновления интерфейса
		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	// Простая реализация команды
	public class RelayCommand : ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool>? _canExecute;

		public RelayCommand(Action execute, Func<bool>? canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
		public void Execute(object? parameter) => _execute();

		public event EventHandler? CanExecuteChanged
		{
			add { if (value != null) CommandManager.RequerySuggested += value; }
			remove { if (value != null) CommandManager.RequerySuggested -= value; }
		}
	}
}