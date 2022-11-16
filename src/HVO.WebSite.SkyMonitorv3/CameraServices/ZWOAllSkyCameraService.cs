using HVO.WebSite.SkyMonitorv3.CameraServices.ZWO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ZWOptical.ASISDK;
using static HVO.WebSite.SkyMonitorv3.CameraServices.ZWO.ASICameraDLL2;

namespace HVO.WebSite.SkyMonitorv3.CameraServices
{
    public class ZWOAllSkyCameraService : IAllSkyCameraService, IDisposable
    {
        private bool _disposed = false;
        private bool _isCameraInitialized = false;

        private readonly ILogger<ZWOAllSkyCameraService> _logger;
        private readonly CameraServiceOptions _cameraServiceOptions;
        private readonly ZWOCameraServiceOptions _zwoOptions;
        private Camera _asiCamera = null;

        public ZWOAllSkyCameraService(ILogger<ZWOAllSkyCameraService> logger, IOptions<CameraServiceOptions> cameraServiceOptions,  IOptions<ZWOCameraServiceOptions> zwoOptions)
        {
            this._logger = logger;
            this._cameraServiceOptions = cameraServiceOptions.Value;
            this._zwoOptions = zwoOptions.Value;    
        }

        public async Task InitializeCamera()
        {
            if (_isCameraInitialized)
            {
                return; 
            }

            this._asiCamera = ASICameras.GetCameraByIndex(0);
            _isCameraInitialized = true;    
        }


        public async Task StartCameraAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                if (this._isCameraInitialized == false)
                {
                    // No camera available
                    throw new Exception("Camera not initialized");
                }

                if (this._asiCamera.status != ASI_STATUS.CLOSED)
                {
                    // No camera available
                    throw new Exception("Camera is already OPEN");
                }

                try
                {
                    this._asiCamera.OpenCamera(); // Will set internal status to 'OPEN'
                    try
                    {

                        this._asiCamera.StartVideoCapture(); // Will set internal status  to "VIDEO'
                        try
                        {
                            // Allocate the memory needed for the image 
                            //IntPtr dataPtr = Marshal.AllocHGlobal((int)bufferSize);
                            //GC.AddMemoryPressure(bufferSize);

                            //try
                            //{
                            //    while (cancellationToken.IsCancellationRequested == false)
                            //    {
                            //        if (this._asiCamera.GetVideoData(dataPtr, bufferSize, -1))
                            //        {
                            //            using (var currentImage = GenerateImage(dataPtr, bufferSize, this._asiCamera.CaptureAreaInfo.ImageType, imageWidth, imageHeight))
                            //            {
                            //                // Call the even handler(s) for this camera image;
                            //                this._asiCamera.OnVideoImageReceived(this, new EventArgs(currentImage, ...));
                            //            }
                            //        }

                            //        // This allows the cancellation signals to not get starved out.
                            //        await Task.Delay(1, cancellationToken);
                            //    }
                            //}
                            //finally
                            //{
                            //    Marshal.FreeHGlobal(dataPtr);
                            //    GC.RemoveMemoryPressure(bufferSize);
                            //}
                        }
                        finally 
                        {
                            this._asiCamera?.StopVideoCapture();
                        }
                    }
                    finally 
                    {
                        this._asiCamera?.CloseCamera();
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }

        private Image GenerateImage(IntPtr dataPtr, long bufferSize, ASI_IMG_TYPE imageType, int imageWidth, int imageHeight)
        {
            // This method is ran in the context of the image transfer loop from the camera. It should be as quick as possible. We 
            // create a compatable 24bpp image.
            var bitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            {
                switch (imageType)
                {
                    case ASICameraDll.ASI_IMG_TYPE.ASI_IMG_Y8:
                    case ASICameraDll.ASI_IMG_TYPE.ASI_IMG_RAW8:
                        {
                            using (var indexedBitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format8bppIndexed))
                            {
                                var bitmapData = indexedBitmap.LockBits(new Rectangle(0, 0, indexedBitmap.Width, indexedBitmap.Height), ImageLockMode.WriteOnly, indexedBitmap.PixelFormat);
                                unsafe
                                {
                                    Buffer.MemoryCopy(dataPtr.ToPointer(), bitmapData.Scan0.ToPointer(), bufferSize, bufferSize); // UNSAFE
                                }

                                indexedBitmap.UnlockBits(bitmapData);

                                //get and fillup a grayscale-palette
                                var colorPalette = indexedBitmap.Palette;

                                Parallel.For(0, 255, i =>
                                {
                                    colorPalette.Entries[i] = Color.FromArgb(255, i, i, i);
                                });

                                indexedBitmap.Palette = colorPalette;

                                // This copies our indexed bitmap over to the new 24bpp image
                                using (var graphics = Graphics.FromImage(bitmap))
                                {
                                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                                    graphics.TranslateTransform(bitmap.Width / 2, bitmap.Height / 2);
                                    graphics.RotateTransform(this.ImageCircleRotationAngle);
                                    graphics.TranslateTransform(-bitmap.Width / 2, -bitmap.Height / 2);

                                    graphics.DrawImage(indexedBitmap, 0, 0, indexedBitmap.Width, indexedBitmap.Height);
                                }
                            }

                            break;
                        }
                    case ASICameraDll.ASI_IMG_TYPE.ASI_IMG_RGB24:
                        {
                            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                            unsafe
                            {
                                Buffer.MemoryCopy(dataPtr.ToPointer(), bitmapData.Scan0.ToPointer(), bufferSize, bufferSize); // UNSAFE
                            }
                            bitmap.UnlockBits(bitmapData);

                            break;
                        }
                    case ASICameraDll.ASI_IMG_TYPE.ASI_IMG_RAW16:
                        {
                            Span<byte> buffer;
                            unsafe
                            {
                                buffer = new Span<byte>(dataPtr.ToPointer(), (int)bufferSize);  // UNSAFE
                            }

                            var bitmapBytes = new short[imageWidth * imageHeight * 3];
                            int j = 0;
                            for (int i = 0; i < buffer.Length; i += 2)
                            {
                                var b = BitConverter.ToInt16(buffer.Slice(i, 2));
                                bitmapBytes[j++] = b;
                                bitmapBytes[j++] = b;
                                bitmapBytes[j++] = b;
                            }


                            // WE SHOULD TRY SCALING THE 16 bit data into 8 BIT  map 0-ushort.maxvalue into 0-255

                            using (var bitmap48 = new Bitmap(imageWidth, imageHeight, PixelFormat.Format48bppRgb))
                            {
                                var bitmapData = bitmap48.LockBits(new Rectangle(0, 0, bitmap48.Width, bitmap48.Height), ImageLockMode.WriteOnly, bitmap48.PixelFormat);

                                Marshal.Copy(bitmapBytes, 0, bitmapData.Scan0, bitmapBytes.Length);
                                bitmap48.UnlockBits(bitmapData);

                                // This copies our indexed bitmap over to the new 24bpp image
                                using (var graphics = Graphics.FromImage(bitmap))
                                {
                                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                                    //graphics.TranslateTransform(bitmap.Width / 2, bitmap.Height / 2);
                                    //graphics.RotateTransform(this.ImageCircleRotationAngle);
                                    //graphics.TranslateTransform(-bitmap.Width / 2, -bitmap.Height / 2);

                                    graphics.DrawImage(bitmap48, 0, 0, bitmap48.Width, bitmap48.Height);
                                }
                            }

                            break;
                        }
                    default:
                        // No image
                        break;
                }
            }

            return bitmap;
        }

        private static byte[] Convert16BitGrayScaleToRgb48(byte[] inBuffer, int width, int height)
        {
            int inBytesPerPixel = 2;
            int outBytesPerPixel = 6;

            byte[] outBuffer = new byte[width * height * outBytesPerPixel];
            int inStride = width * inBytesPerPixel;
            int outStride = width * outBytesPerPixel;

            // Step through the image by row  
            for (int y = 0; y < height; y++)
            {
                // Step through the image by column  
                for (int x = 0; x < width; x++)
                {
                    // Get inbuffer index and outbuffer index 
                    int inIndex = (y * inStride) + (x * inBytesPerPixel);
                    int outIndex = (y * outStride) + (x * outBytesPerPixel);

                    byte hibyte = inBuffer[inIndex + 1];
                    byte lobyte = inBuffer[inIndex];

                    //R
                    outBuffer[outIndex] = lobyte;
                    outBuffer[outIndex + 1] = hibyte;

                    //G
                    outBuffer[outIndex + 2] = lobyte;
                    outBuffer[outIndex + 3] = hibyte;

                    //B
                    outBuffer[outIndex + 4] = lobyte;
                    outBuffer[outIndex + 5] = hibyte;
                }
            }
            return outBuffer;
        }




        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AllSkyCameraService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
