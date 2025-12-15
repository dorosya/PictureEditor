using System;
using System.Collections.Generic;

namespace PhotoEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Photo Editor ===");
            List<Photo> photos = new List<Photo>();

            photos.Add(new Photo("example1.jpg"));
            photos.Add(new Photo("example2.jpg"));

            foreach (var photo in photos)
            {
                photo.Display();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}