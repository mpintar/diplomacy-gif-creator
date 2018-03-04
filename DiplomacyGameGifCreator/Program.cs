using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SixLabors.ImageSharp;

namespace DiplomacyGameGifCreator
{
    class Program
    {
        private static List<Image<Rgba32>> images = new List<Image<Rgba32>>();

        static void Main(string[] args)
        {
            Console.Write("Enter game number:");
            int gameID = GetGameID();

            Console.Write("How many years? ");
            int yearCount = GetYearCount();
            
            DownloadImages(gameID, yearCount);
            CreateGif(gameID);
            Console.ReadLine();
        }

        private static int GetGameID()
        {
            return GetEnteredNumber("Invalid game number. Try Again: ");
        }

        private static int GetYearCount()
        {
            return GetEnteredNumber("Invalid year count. Try Again: ");
        }

        private static int GetEnteredNumber(string invalidMessage)
        {
            while (true)
            {
                var enteredYearCount = Console.ReadLine();

                if (Int32.TryParse(enteredYearCount, out int enteredNumber))
                {
                    return enteredNumber;
                }
                else
                {
                    Console.Write(invalidMessage);
                }
            }
        }

        private static void DownloadImages(int gameID, int numberOfYears)
        {
            int historyCount = numberOfYears * 2;

            for (int i = 0; i <= historyCount; i++)
            {
                string baseUrl = string.Format("http://www.playdiplomacy.com/games/1/{0}/game-history-{1}-{2}", gameID, gameID, i);

                string[] urls = { baseUrl + "-O.png",
                                  baseUrl + "-R.png",
                                  baseUrl + "-B.png"};

                foreach (var url in urls)
                {
                    var image = GetImage(url);
                    if (image != null)
                    {
                        images.Add(image);
                    }
                }
            }
        }

        private static Image<Rgba32> GetImage(string url)
        {
            using (WebClient wc = new WebClient())
            {
                byte[] bytes;

                try
                {
                    bytes = wc.DownloadData(url);
                }
                catch (Exception e)
                {
                    return null;
                }

                return Image.Load(bytes);
            }
        }

        private static void CreateGif(int gameID)
        {
            if(images.Count == 0)
            {
                Console.WriteLine("Failed to download any images");
                return;
            }

            var animatedGif = images[0];
            images.RemoveAt(0);

            var blackFrame = new Image<Rgba32>(animatedGif.Width, animatedGif.Height);
            blackFrame.Mutate(image => image.BackgroundColor(Rgba32.Black));
            images.Add(blackFrame);

            foreach (var image in images)
            {
                var frameData = image.Frames[0];
                animatedGif.Frames.AddFrame(frameData);
            }

            foreach (var frame in animatedGif.Frames)
            {
                frame.MetaData.FrameDelay = 50;
            }

            using (FileStream output = File.OpenWrite(string.Format("{0}.gif", gameID)))
            {
                animatedGif.SaveAsGif(output);
                Console.WriteLine("Saved to: " + output.Name);
            }
        }
    }
}
