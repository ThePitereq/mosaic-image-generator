using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AIProject3
{
    class Program
    {
		// MOSAIC IMAGE GENERATOR
		// AUTORZY
		// Kuształa Kamil
		// Ochowiak Kinga
		// Olejniczak Piotr
		//
		// Generator zdjęć z mniejszych elementów, w tym przypadku tekstury z gry Minecraft. Program wspiera każdą teksturę 16x16. W przypadku innych rozmiarów wymagane byłyby małe zmiany w kodzie.
		// Program początkowo wyszukuje tekstury najbliższe kolorowi danego piksela a następnie wykorzystuje wczesniej poznane przy generowaniu kolejnych obrazów.
		// Wraz z dalszym uczeniem się programu czas generowania zdjęć może zostać skrócony nawet 5-krotnie.
		// Program posiada kilka QoL funkcji takich jak skalowanie, obracanie, generowanie do zdjęcia (w założeniu projektu kluczowym jest generowanie się kodu HTML z wyglądem,
		// gdyż cały kod generowania obrazu jest o wiele bardziej zasobożerny od generowania tekstur odpowiadających kolorom)
		
		
        //Cache nauczonych kolorów i odpowiadających bloków.
        static readonly Dictionary<string, string> rgbBlock = new Dictionary<string, string>();
        //Cache odpowiadających bloków.
        static readonly Dictionary<string, ColorCode> blockAvg = new Dictionary<string, ColorCode>();

        //Klasa definująca kolory odpowiadających bloków w cache.
        private class ColorCode
        {
            public int R;
            public int G;
            public int B;
        }

        static void Main()
        {
            int count = 0;
            Console.WriteLine($"Searching for suitable images in 'block' folder.");
            string currentDirectory = Directory.GetCurrentDirectory();
            //Szukanie odpowiadających bloków w folderze 'block'
            foreach (var file in Directory.GetFiles($"{currentDirectory}\\block"))
            {
                //Sprawdzanie czy końcówka to .png, jak nie to kontynuacja. Ignoruje śmieci i inne niepotrzebne pliki.
                if (file.Split('.').Last() != "png") continue;
                //Tworzy bitmape z pliku.
                Bitmap image = new Bitmap(file);
                bool transparent = false;
                //Sprawdza czy tekstura odpowiada prawidłowym rozmiarom.
                if (image.Width != 16 || image.Height != 16) continue;
                int[] colorSum = { 0, 0, 0 };
                //Loop przez szerokość zdjęcia.
                for (int i = 0; i < image.Width; i++)
                {
                    //Loop przez wysokość zdjęcia.
                    for (int j = 0; j < image.Height; j++)
                    {
                        //Sprawdzanie pixela na pozycji i, j.
                        Color pixel = image.GetPixel(i, j);
                        //Sprawdza czy pixel posiada przezroczystość. Jeżeli tak, to ignoruje ten plik.
                        if (pixel.A != 255)
                        {
                            transparent = true;
                            break;
                        }
                        colorSum[0] += pixel.R;
                        colorSum[1] += pixel.G;
                        colorSum[2] += pixel.B;
                    }
                    if (transparent) break;
                }
                if (transparent) continue;
                //Dodaje blok do cache.
                blockAvg.Add(file, new ColorCode()
                {
                    R = colorSum[0] / 256,
                    G = colorSum[1] / 256,
                    B = colorSum[2] / 256
                });
                count++;
            }
            Console.WriteLine($"Found {count} suitable images.");
            int learnNow = -1;
            Console.Write("Learn colors now? (1 - Yes, 0 - No): ");
            while (learnNow == -1)
                learnNow = Convert.ToInt32(Console.ReadLine());
            //Program do nauki wstępnej. Praktycznie kopia tego co jest niżej, więc nie będe go tu opisywał.
            if (learnNow == 1)
            {
                string path = $"{currentDirectory}\\learn";
                int imageAmount = Directory.GetFiles(path).Count();
                Console.WriteLine($"Starting learning colors from {imageAmount} files in 'learn' folder...");
                int imageCurrentCount = 0;
                int learnedColors = 0;
                foreach (var file in Directory.GetFiles($"{currentDirectory}\\learn"))
                {
                    Bitmap image = new Bitmap(file);
                    int loopCountMax = image.Height * image.Width;
                    int loopCount = 0;
                    for (int i = 0; i < image.Height; i++)
                    {
                        for (int j = 0; j < image.Width; j++)
                        {
                            loopCount++;
                            if (loopCount % 10000 == 0)
                                Console.WriteLine($"Progress: {loopCount}/{loopCountMax} [{imageCurrentCount}/{imageAmount}]");
                            Color pixel = image.GetPixel(j, i);
                            if (!rgbBlock.ContainsKey($"{pixel.R} {pixel.G} {pixel.B}"))
                            {
                                LearnColor(pixel);
                                learnedColors++;
                            }
                        }
                    }
                    imageCurrentCount++;
                }
                Console.WriteLine($"Pre-learned {learnedColors} color values!");
            }
            while (1 == 1)
            {
                Console.WriteLine("");
                Console.WriteLine("");
                string filename = String.Empty;
                int saveToImage = -1;
                int rotation = -1;
                int scale = -1;
                Console.WriteLine($"Known color-codes: {rgbBlock.Count}");
                Console.WriteLine("Type image filename to start or 'exit' to exit.");
                Console.Write("Image Filename: ");
                while (filename == string.Empty)
                    filename = Console.ReadLine();
                if (filename == "exit")
                    break;
                Console.Write("Save to Image? (High performance impact, 1 - Yes, 0 - No): ");
                while (saveToImage == -1)
                    saveToImage = Convert.ToInt32(Console.ReadLine());
                Console.Write("Rotate Image? (Multiplies of 90): ");
                while (rotation == -1)
                    rotation = Convert.ToInt32(Console.ReadLine()) * 90;
                Console.Write("Scale Image? (1 block as X pixels): ");
                while (scale == -1)
                    scale = Convert.ToInt32(Console.ReadLine());
                try
                {
                    //Pobiera czas aktualny, żeby na koniec było wiadomo ile to zajęło.
                    DateTime start = DateTime.Now;
                    int alreadyAddedCount = 0;
                    int newAddedCount = 0;
                    //StringBuilder do pliku HTML. O wiele szybsze rozwiązanie od robienia zdjęć, lecz nie nadaje się na ogromne zdjęcia.
                    StringBuilder htmlFile = new StringBuilder("<html>");
                    Bitmap image = new Bitmap(filename);
                    //Obracamy zdjęcie.
                    if (rotation == 90)
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    else if (rotation == 180)
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    else if (rotation == 270)
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    //Skalujemy zdjęcie.
                    if (scale > 1)
                        image = ResizeImage(image, image.Width / scale, image.Height / scale);
                    int loopCountMax = image.Height * image.Width;
                    int loopCount = 0;
                    int currentX = 0;
                    int currentY = 0;
                    //Tworzymy wyjściowe zdjęcie x16 większe od normalnego.
                    Bitmap outputImage = new Bitmap(image.Width * 16, image.Height * 16);
                    Graphics g = Graphics.FromImage(outputImage);
                    Console.WriteLine($"Starting generating image with {loopCountMax} pixels...");
                    for (int i = 0; i < image.Height; i++)
                    {
                        for (int j = 0; j < image.Width; j++)
                        {
                            loopCount++;
                            //Wyświetlanie progressu co 10000 iteracji.
                            if (loopCount % 10000 == 0)
                                Console.WriteLine($"Progress: {loopCount}/{loopCountMax}");
                            Color pixel = image.GetPixel(j, i);
                            //Sprawdzamy czy w cache kolorów jest już wartość, ta akcja skraca działanie AI ponad 4-krotnie.
                            if (rgbBlock.ContainsKey($"{pixel.R} {pixel.G} {pixel.B}"))
                            {
                                string key = $"{currentDirectory}//blocks//{rgbBlock[$"{pixel.R} {pixel.G} {pixel.B}"]}";
                                //Dodajemy odpowiednik zdjęcia do pliku HTML.
                                htmlFile.Append($"<img src=\"{key}\">");
                                //Jeżeli zapisujemy do obrazu to totaj dodaje nasz blok do finalnego obrazu.
                                if (saveToImage == 1)
                                {
                                    Image iconImage = Image.FromFile(key);
                                    g.DrawImage(iconImage, currentX, currentY);
                                }
                                alreadyAddedCount++;
                            }
                            else
                            {
                                //Uczy się nowego koloru.
                                string newKey = LearnColor(pixel);
                                htmlFile.Append($"<img src=\"{newKey}\">");
                                if (saveToImage == 1)
                                {
                                    Image iconImage = Image.FromFile(newKey);
                                    g.DrawImage(iconImage, currentX, currentY);
                                }
                                newAddedCount++;
                            }
                            currentX += 16;
                        }
                        //Robi kolejną linię w pliku HTML.
                        htmlFile.Append("</br>");
                        //Resetuje offset tworzenia zdjęcia.
                        currentX = 0;
                        currentY += 16;
                    }
                    htmlFile.Append("</html>");
                    if (saveToImage == 1)
                        outputImage.Save($"outputImages/{filename}.png");
                    Console.WriteLine($"Already added pixels: {alreadyAddedCount}");
                    Console.WriteLine($"New learned pixels: {newAddedCount}");
                    Console.WriteLine($"Image generated in: {(DateTime.Now - start).TotalSeconds}s");

                    File.WriteAllText($"outputHTML/{filename}.html", htmlFile.ToString());
                }
                catch
                {
                    Console.WriteLine($"File {filename} is missing!");
                }
            }
        }

        private static string LearnColor(Color pixel)
        {
            int lowestErrorValue = 10000;
            string lowestErrorKey = String.Empty;
            //Sprawdzanie każdych bloków w bibliotece.
            foreach (var block in blockAvg)
            {
                //Sprawdzanie błedu wartości przy aktualnym bloku.
                int value = Math.Abs(block.Value.R - pixel.R) + Math.Abs(block.Value.G - pixel.G) + Math.Abs(block.Value.B - pixel.B);
                if (value < lowestErrorValue)
                {
                    //Jeżeli błąd jest mniejszy to nadpisuje większy.
                    lowestErrorValue = value;
                    lowestErrorKey = block.Key;
                }
            }
            //Finalnie przypisuje blok do kodu koloru.
            string shortPath = lowestErrorKey.Split('\\').Last();
            rgbBlock.Add($"{pixel.R} {pixel.G} {pixel.B}", shortPath);
            return lowestErrorKey;
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            //Tworzy nowy obraz.
            Rectangle destRect = new Rectangle(0, 0, width, height);
            //Tworzy nową bitmapę.
            Bitmap destImage = new Bitmap(width, height);
            //Ustawia rozdzielczośc na bazie zdjęcia.
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                //Ustawienia polepszające wygląd zeskalowanego obrazu.
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    //Robi gradient przy skalowanych pixelach, by nie wyglądało rozpikselowane.
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    //Generuje finalnie obraz.
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}
