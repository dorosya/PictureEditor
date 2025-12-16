using System;
using System.Drawing;
using System.IO;

namespace PhotoEditor
{
	/// <summary>
	/// Сервисные операции для работы с Photo
	/// </summary>
	public class Services
	{
		/// <summary>
		/// Загрузка изображения из файла в объект Photo
		/// </summary>
		public void LoadImage(Photo photo)
		{
			if (!File.Exists(photo.FilePath))
				throw new FileNotFoundException("Файл изображения не найден");

			photo.Image = new Bitmap(photo.FilePath);
		}

		/// <summary>
		/// Проверка, что изображение загружено
		/// </summary>
		public void EnsureImageLoaded(Photo photo)
		{
			if (photo.Image == null)
				throw new InvalidOperationException("Изображение не загружено");
		}

		/// <summary>
		/// Сохранение изображения в файл
		/// </summary>
		public void SaveImage(Photo photo, string path)
		{
			EnsureImageLoaded(photo);
			photo.Image!.Save(path);
		}
	}
}
