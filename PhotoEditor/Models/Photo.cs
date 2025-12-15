using System;

namespace PhotoEditor
{
    public class Photo
    {
        public string Name { get; set; }

        public Photo(string name)
        {
            Name = name;
        }

        public void Display()
        {
            Console.WriteLine($"Фото: {Name}");
        }
    }
}