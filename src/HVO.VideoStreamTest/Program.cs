using System.IO.Pipes;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;
using System.Net;
using System.Net.Security;
using System.Threading;


namespace HVO.VideoStreamTest
{
    internal class Program
    {
        //        private static byte[] imageData = new byte[0];

        private const string AllSkyImageLocation = "https://192.168.0.6/indi-allsky/images/latest.jpg";
        //private const string AllSkyImageLocation = "http://192.168.0.4/image/cam98";

        private const string FFMpegBinary = "ffmpeg.exe";
        private const string PipeName = "test_pipe";

        private const string FFMpegArgs = "-hide_banner -y";
        private const string FFMpegInputArgsAudio = "-f lavfi -i anullsrc";
        private const string FFMpegInputArgsVideo = @"-f image2pipe -r 30 -s 1950x1310 -i \\.\pipe\test_pipe";

        private const string FFMpegOutputArgsAudio = "-c:a aac -b:a 8k -ac 1"; // -ar 44100";
        private const string FFMpegOutputArgsVideo = "-c:v libx264 -preset ultrafast -crf 17 -tune zerolatency -movflags +faststart -vf \"scale=1280:-2\" -g 60"; // -b:v 2500k -maxrate 2500k -bufsize 2500k";
        private const string FFMpegOutputArgsTarget = "-f flv rtmp://a.rtmp.youtube.com/live2/13jx-fvct-7qgm-g30v-crzw";

        private const byte FPS = 30;

        // ffmpeg.exe
        // -hide_banner -y
        // -f lavfi -i anullsrc 
        // -f image2pipe -r 30 -s 1950x1310 -i \\.\pipe\test_pipe
        // -c:v libx264 -preset fast -crf 18 -tune zerolatency -movflags +faststart -vf "scale=1280:-2" -g 60 -b:v 2500k -maxrate 2500k -bufsize 2500k 
        // -c:a aac -b:a 8k -ac 1 -ar 44100 
        // -f flv rtmp://a.rtmp.youtube.com/live2/13jx-fvct-7qgm-g30v-crzw


        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

            while (true)
            {
                using (var namedPipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte))
                {
                    Console.Error.WriteLine("Waiting for connection...");
                    namedPipeServer.WaitForConnection();

                    Console.Error.WriteLine("Connected");

                    // Get the initial image data so we know the size of the image
                    var imageData = DownloadImageAsBytes(AllSkyImageLocation);

                    // start a timer that will download the image form the website once every 5 seconds and save it to a byte array
                    using (var downlodTimer = new System.Timers.Timer(5000) { AutoReset = true })
                    {
                        downlodTimer.Elapsed += (sender, e) =>
                        {
                            try
                            {
                                imageData = DownloadImageAsBytes(AllSkyImageLocation);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(ex.Message);
                            }
                        };
                        downlodTimer.Enabled = true;

                        // Start a timer that will send the image data to the client once every 1/30 second (30 fps)
                        using (var imageFpsTimer = new System.Timers.Timer(1000 / FPS) { AutoReset = true })
                        {
                            imageFpsTimer.Elapsed += (sender, e) =>
                            {
                                try
                                {
                                    // Send the image data to the client
                                    if (namedPipeServer.IsConnected)
                                    {
                                        namedPipeServer.Write(imageData, 0, imageData.Length);
                                        namedPipeServer.Flush();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine(ex.Message);
                                }
                            };

                            imageFpsTimer.Enabled = true;
                            Console.ReadLine();


                            //var processStartInfo = new System.Diagnostics.ProcessStartInfo
                            //{
                            //    FileName = FFMpegBinary,
                            //    Arguments = $"{FFMpegArgs} {FFMpegInputArgsAudio} {FFMpegInputArgsVideo} {FFMpegOutputArgsAudio} {FFMpegOutputArgsVideo} {FFMpegOutputArgsTarget}",
                            //    //Arguments = @" -hide_banner -y -f lavfi -i anullsrc  -r 30 -f image2pipe -s 1950x1310  -i \\.\pipe\test_pipe -c:v libx264 -preset fast -crf 18 -tune zerolatency -movflags +faststart -vf ""scale=1280:-2"" -c:a aac -b:a 8k -ac 1 -g 60 -f flv rtmp://a.rtmp.youtube.com/live2/13jx-fvct-7qgm-g30v-crzw",
                            //    UseShellExecute = true,
                            //    //RedirectStandardInput = true,
                            //    //RedirectStandardOutput = true,
                            //    //RedirectStandardError = true,
                            //    //CreateNoWindow = true
                            //};
                            //try
                            //{
                            //    using (var process = new System.Diagnostics.Process() { StartInfo = processStartInfo })
                            //    {
                            //        process.EnableRaisingEvents = true;

                            //        // Any errors write them out (DEBUG CODE)
                            //        //process.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);
                            //        //process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);

                            //        process.Start();

                            //        // Begin looking for errors
                            //        //process.BeginErrorReadLine();
                            //        //process.BeginOutputReadLine();

                            //        Console.Error.WriteLine("Waiting for connection...");
                            //        await server.WaitForConnectionAsync(cancellationTokenSource.Token);
                            //        Console.Error.WriteLine("Connected");

                            //        // Enable the pipe FPS writer
                            //        timer.Enabled = true;

                            //        // Wait for the process to exit.
                            //        await process.WaitForExitAsync(cancellationTokenSource.Token);
                            //    };
                            //}
                            //catch { }
                            //finally
                            //{
                            //    // Disable the pipe FPS writer
                            //    timer.Enabled = false;
                            //}

                            //cancellationTokenSource.Cancel();
                        }
                    }
                }
            }
        }


public static byte[] DownloadImageAsBytes(string url)
    {
        using (WebClient webClient = new WebClient())
        {
            return webClient.DownloadData(url);
        }
    }


    public static byte[] GetJpegData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        return File.ReadAllBytes(filePath);
    }


    public static byte[] ConvertJpgToBmpBytes(string sourcePath)
        {
            using (var image = Image.FromFile(sourcePath))
            {
                using (var bitmap = new Bitmap(image))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                        return memoryStream.ToArray();
                    }
                }
            }
        }

    }
}
