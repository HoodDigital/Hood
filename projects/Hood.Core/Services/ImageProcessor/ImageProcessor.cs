using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Hood.Services
{
    public class ImageProcessor
    {
        public static void ResizeImage(string FileNameInput, string FileNameOutput, double ResizeHeight, double ResizeWidth, ImageFormat OutputFormat)
        {
            using (System.Drawing.Image photo = new Bitmap(FileNameInput))
            {
                double aspectRatio = (double)photo.Width / photo.Height;
                double boxRatio = ResizeWidth / ResizeHeight;
                double scaleFactor = 0;

                if (photo.Width < ResizeWidth && photo.Height < ResizeHeight)
                {
                    //keep the image the same size since it is already smaller than our max width / height
                    scaleFactor = 1.0;
                }
                else
                {
                    if (boxRatio > aspectRatio)
                        scaleFactor = ResizeHeight / photo.Height;
                    else
                        scaleFactor = ResizeWidth / photo.Width;
                }

                int newWidth = (int)(photo.Width * scaleFactor);
                int newHeight = (int)(photo.Height * scaleFactor);

                using (Bitmap bmp = new Bitmap(newWidth, newHeight))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        g.DrawImage(photo, 0, 0, newWidth, newHeight);

                        if (ImageFormat.Png.Equals(OutputFormat))
                        {
                            bmp.Save(FileNameOutput, OutputFormat);
                        }
                        else if (ImageFormat.Jpeg.Equals(OutputFormat))
                        {
                            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
                            EncoderParameters encoderParameters;
                            using (encoderParameters = new System.Drawing.Imaging.EncoderParameters(1))
                            {
                                // use jpeg info[1] and set quality to 90
                                encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(Encoder.Quality, 90L);
                                bmp.Save(FileNameOutput, info[1], encoderParameters);
                            }
                        }
                    }
                }
            }
        }
    }
}
