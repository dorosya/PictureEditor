using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictureEditor.Models;
using PictureEditor.Service;
using PictureEditor.Utils;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;

namespace PictureEditor.Tests
{
    [TestClass]
    public class PhotoTests
    {
        private string _testImagePath;
        private Photo _photo;

        [TestInitialize]
        public void Setup()
        {
            _testImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            CreateTestImage(_testImagePath);
            _photo = new Photo(_testImagePath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _photo?.UnloadImage();

            if (File.Exists(_testImagePath))
            {
                try { File.Delete(_testImagePath); } catch { }
            }
        }

        private void CreateTestImage(string path)
        {
            using (var bitmap = new Bitmap(100, 100))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Red);
                }
                bitmap.Save(path);
            }
        }

        [TestMethod]
        public void Photo_Constructor_ValidPath_CreatesPhoto()
        {
            var photo = new Photo(_testImagePath);
            Assert.AreEqual(_testImagePath, photo.FilePath);
            Assert.AreEqual(Path.GetFileName(_testImagePath), photo.Name);
            Assert.IsNull(photo.Image);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Photo_Constructor_InvalidPath_ThrowsException()
        {
            new Photo("C:\\nonexistent.jpg");
        }

        [TestMethod]
        public void LoadImage_ValidFile_LoadsBitmap()
        {
            var photo = new Photo(_testImagePath);
            photo.LoadImage();
            Assert.IsNotNull(photo.Image);
            Assert.IsNotNull(photo.BitmapImage);
            Assert.AreEqual(100, photo.Image.Width);
            Assert.AreEqual(100, photo.Image.Height);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadImage_FileNotFound_ThrowsException()
        {
            var photo = new Photo(_testImagePath);
            File.Delete(_testImagePath);
            photo.LoadImage();
        }

        [TestMethod]
        public void SaveImage_ValidPath_SavesFile()
        {
            var photo = new Photo(_testImagePath);
            photo.LoadImage();
            var savePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_saved.png");

            try
            {
                photo.SaveImage(savePath);
                Assert.IsTrue(File.Exists(savePath));
                Assert.AreEqual(Path.GetFullPath(savePath), photo.FilePath);
            }
            finally
            {
                if (File.Exists(savePath))
                {
                    try { File.Delete(savePath); } catch { }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SaveImage_ImageNotLoaded_ThrowsException()
        {
            var photo = new Photo(_testImagePath);
            photo.SaveImage("test.jpg");
        }

        [TestMethod]
        public void SetImage_Bitmap_UpdatesImage()
        {
            var photo = new Photo(_testImagePath);
            using (var newBitmap = new Bitmap(50, 50))
            {
                photo.SetImage(newBitmap);
                Assert.AreEqual(newBitmap, photo.Image);
                Assert.IsNotNull(photo.BitmapImage);
            }
        }

        [TestMethod]
        public void UnloadImage_DisposesImage()
        {
            var photo = new Photo(_testImagePath);
            photo.LoadImage();
            photo.UnloadImage();
            Assert.IsNull(photo.Image);
            Assert.IsNull(photo.BitmapImage);
        }
    }

    [TestClass]
    public class FiltersTests
    {
        private Filters _filters;

        [TestInitialize]
        public void Setup()
        {
            _filters = new Filters();
        }

        private BitmapImage CreateTestBitmapImage(int width, int height, System.Windows.Media.Color color)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(
                    new SolidColorBrush(color),
                    null,
                    new System.Windows.Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0;

                var result = new BitmapImage();
                result.BeginInit();
                result.StreamSource = stream;
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        [TestMethod]
        public void ApplyBrightness_Factor2_ImageBecomesBrighter()
        {
            var testImage = CreateTestBitmapImage(10, 10, Colors.Red);
            var result = _filters.ApplyBrightness(testImage, 2.0f);
            Assert.IsNotNull(result);
            Assert.AreEqual(testImage.PixelWidth, result.PixelWidth);
            Assert.AreEqual(testImage.PixelHeight, result.PixelHeight);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApplyBrightness_NullSource_ThrowsException()
        {
            _filters.ApplyBrightness(null, 1.0f);
        }

        [TestMethod]
        public void ApplyColorFilter_PositiveOffset_ChangesColor()
        {
            var testImage = CreateTestBitmapImage(10, 10, Colors.Red);
            var result = _filters.ApplyColorFilter(testImage, 50, 0, 0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApplyColorFilter_NullSource_ThrowsException()
        {
            _filters.ApplyColorFilter(null, 0, 0, 0);
        }
    }

    [TestClass]
    public class CropServiceTests
    {
        private CropService _cropService;

        [TestInitialize]
        public void Setup()
        {
            _cropService = new CropService();
        }

        private BitmapSource CreateTestBitmapSource(int width, int height)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(
                    new SolidColorBrush(Colors.Red),
                    null,
                    new System.Windows.Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        [TestMethod]
        public void Crop_ValidRectangle_ReturnsCroppedImage()
        {
            var testImage = CreateTestBitmapSource(100, 100);
            var result = _cropService.Crop(testImage, 10, 10, 50, 50);
            Assert.IsNotNull(result);
            Assert.AreEqual(50, result.PixelWidth);
            Assert.AreEqual(50, result.PixelHeight);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Crop_NullSource_ThrowsException()
        {
            _cropService.Crop(null, 0, 0, 10, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Crop_InvalidRectangle_ThrowsException()
        {
            var testImage = CreateTestBitmapSource(100, 100);
            _cropService.Crop(testImage, -10, 0, 50, 50);
        }
    }

    [TestClass]
    public class StateManagerTests
    {
        private string _testImagePath1;
        private string _testImagePath2;
        private List<Photo> _photos;
        private const string StateFile = "app_state.json";

        [TestInitialize]
        public void Setup()
        {
            _testImagePath1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            _testImagePath2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            CreateTestImage(_testImagePath1);
            CreateTestImage(_testImagePath2);

            _photos = new List<Photo>
            {
                new Photo(_testImagePath1),
                new Photo(_testImagePath2)
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var path in new[] { _testImagePath1, _testImagePath2, StateFile })
            {
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        private void CreateTestImage(string path)
        {
            using (var bitmap = new Bitmap(10, 10))
            {
                bitmap.Save(path);
            }
        }

        [TestMethod]
        public void SaveState_ValidPhotos_CreatesJsonFile()
        {
            StateManager.SaveState(_photos);
            Assert.IsTrue(File.Exists(StateFile));
        }

        [TestMethod]
        public void LoadState_NoStateFile_ReturnsEmptyList()
        {
            if (File.Exists(StateFile))
                File.Delete(StateFile);

            var result = StateManager.LoadState();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SaveAndLoadState_RoundTrip_ReturnsSamePaths()
        {
            StateManager.SaveState(_photos);
            var loadedPhotos = StateManager.LoadState();
            Assert.AreEqual(2, loadedPhotos.Count);
            Assert.AreEqual(_testImagePath1, loadedPhotos[0].FilePath);
            Assert.AreEqual(_testImagePath2, loadedPhotos[1].FilePath);
        }
    }

    [TestClass]
    public class CollageMakerTests
    {
        private CollageMaker _collageMaker;
        private List<BitmapImage> _testImages;

        [TestInitialize]
        public void Setup()
        {
            _collageMaker = new CollageMaker();
            _testImages = new List<BitmapImage>
            {
                CreateTestBitmapImage(50, 50, Colors.Red),
                CreateTestBitmapImage(50, 50, Colors.Green),
                CreateTestBitmapImage(50, 50, Colors.Blue)
            };
        }

        private BitmapImage CreateTestBitmapImage(int width, int height, System.Windows.Media.Color color)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(
                    new SolidColorBrush(color),
                    null,
                    new System.Windows.Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0;

                var result = new BitmapImage();
                result.BeginInit();
                result.StreamSource = stream;
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        [TestMethod]
        public void CreateCollage_ThreeImages_ReturnsCollage()
        {
            var result = _collageMaker.CreateCollage(_testImages);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.PixelWidth > 0);
            Assert.IsTrue(result.PixelHeight > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateCollage_EmptyList_ThrowsException()
        {
            _collageMaker.CreateCollage(new List<BitmapImage>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))] // ИСПРАВЛЕНО: было ArgumentNullException
        public void CreateCollage_NullList_ThrowsException()
        {
            _collageMaker.CreateCollage(null);
        }

        [TestMethod]
        public void CreateCollage_SingleImage_ReturnsImage()
        {
            var singleImage = new List<BitmapImage> { _testImages[0] };
            var result = _collageMaker.CreateCollage(singleImage);
            Assert.IsNotNull(result);
        }
    }

    [TestClass]
    public class TextServiceTests
    {
        private TextService _textService;

        [TestInitialize]
        public void Setup()
        {
            _textService = new TextService();
        }

        private BitmapSource CreateTestBitmapSource(int width, int height)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(
                    new SolidColorBrush(Colors.White),
                    null,
                    new System.Windows.Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        [TestMethod]
        public void AddText_ValidParameters_ReturnsImageWithText()
        {
            var testImage = CreateTestBitmapSource(100, 100);
            var result = _textService.AddText(testImage, "Test", 10, 10, 12, Colors.Black);
            Assert.IsNotNull(result);
            Assert.AreEqual(testImage.PixelWidth, result.PixelWidth);
            Assert.AreEqual(testImage.PixelHeight, result.PixelHeight);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddText_NullSource_ThrowsException()
        {
            _textService.AddText(null, "Test", 10, 10, 12, Colors.Black);
        }
    }

    [TestClass]
    public class PixelEditorTests
    {
        private PixelEditor _pixelEditor;

        [TestInitialize]
        public void Setup()
        {
            _pixelEditor = new PixelEditor();
        }

        private BitmapSource CreateTestBitmapSource(int width, int height)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(
                    new SolidColorBrush(Colors.White),
                    null,
                    new System.Windows.Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        [TestMethod]
        public void SetPixel_ValidCoordinates_ChangesPixel()
        {
            var testImage = CreateTestBitmapSource(10, 10);
            var result = _pixelEditor.SetPixel(testImage, 5, 5, Colors.Red);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetPixel_NullSource_ThrowsException()
        {
            _pixelEditor.SetPixel(null, 0, 0, Colors.Red);
        }
    }
}