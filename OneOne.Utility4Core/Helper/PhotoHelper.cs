using System;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;


namespace OneOne.Utility4Core.Helper
{
    public class PhotoHelper
    {
        /// <summary>
        /// ��ȡָ��mimeType��ImageCodecInfo
        /// </summary>
        private static ImageCodecInfo GetImageCodecInfo(string mimeType)
        {
            ImageCodecInfo[] CodecInfo = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in CodecInfo)
            {
                if (ici.MimeType == mimeType) return ici;
            }
            return null;
        }

        /// <summary>
        ///  ��ȡinputStream�е�Bitmap����
        /// </summary>
        public static Bitmap GetBitmapFromStream(Stream inputStream)
        {
            Bitmap bitmap = new Bitmap(inputStream);
            return bitmap;
        }

        /// <summary>
        /// ��Bitmap����ѹ��ΪJPGͼƬ����
        /// </summary>
        /// </summary>
        /// <param name="bmp">Դbitmap����</param>
        /// <param name="saveFilePath">Ŀ��ͼƬ�Ĵ洢��ַ</param>
        /// <param name="quality">ѹ��������Խ����ƬԽ�������Ƽ�80</param>
        public static void CompressAsJPG(Bitmap bmp, string saveFilePath, int quality)
        {
            EncoderParameter p = new EncoderParameter(System.DrawingCore.Imaging.Encoder.Quality, quality); ;
            EncoderParameters ps = new EncoderParameters(1);
            ps.Param[0] = p;
            bmp.Save(saveFilePath, GetImageCodecInfo("image/jpeg"), ps);
            bmp.Dispose();
        }

        /// <summary>
        /// ��inputStream�еĶ���ѹ��ΪJPGͼƬ����
        /// </summary>
        /// <param name="inputStream">ԴStream����</param>
        /// <param name="saveFilePath">Ŀ��ͼƬ�Ĵ洢��ַ</param>
        /// <param name="quality">ѹ��������Խ����ƬԽ�������Ƽ�80</param>
        public static void CompressAsJPG(Stream inputStream, string saveFilePath, int quality)
        {
            Bitmap bmp = GetBitmapFromStream(inputStream);
            CompressAsJPG(bmp, saveFilePath, quality);
        }

        /// <summary>
        /// ��ȡͼƬ������Ϣ
        /// </summary>
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        public static string SavePic(Bitmap bmp, string fileName, int width, int height, bool isCompress, int quality)
        {
            string sRet = string.Empty;
            System.IO.FileStream fs;

            Bitmap newBmp;
            Graphics g;
            float oWidth = 0; //ԭʼ���
            float oHeight = 0; //ԭʼ�߶�
            float newWidth = 0; //�¿��
            float newHeight = 0; //�¸߶�

            oWidth = bmp.Width;
            oHeight = bmp.Height;
            if (oWidth > width && oHeight > height)
            {
                if (oWidth / width < oHeight / height)
                {
                    //����Ҫ����
                    newWidth = height / oHeight * oWidth;
                    newHeight = height;
                }
                else
                {
                    //����Ҫ����
                    newHeight = width / oWidth * oHeight;
                    newWidth = width;
                }
            }
            else if (oWidth > width || oHeight > height)
            {
                if (oWidth > width)
                {
                    newWidth = width;
                    newHeight = oHeight / oWidth * width;
                }
                else
                {
                    newWidth = oWidth / oHeight * height;
                    newHeight = height;
                }
            }
            else
            {
                newWidth = oWidth;
                newHeight = oHeight;
            }

            System.DrawingCore.Imaging.ImageAttributes ia;

            ia = new System.DrawingCore.Imaging.ImageAttributes();
            newBmp = new Bitmap(width, height);
            g = Graphics.FromImage(newBmp);
            g.SmoothingMode = System.DrawingCore.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.DrawingCore.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.DrawingCore.Text.TextRenderingHint.AntiAlias;
            g.CompositingQuality = System.DrawingCore.Drawing2D.CompositingQuality.HighQuality;
            g.FillRectangle(Brushes.White, 0, 0, newBmp.Width, newBmp.Height);
            g.DrawImage(bmp, (width - newWidth) / 2, (height - newHeight) / 2, newWidth, newHeight);

            string sPath = fileName.Substring(0, fileName.LastIndexOf("\\"));
            if (!System.IO.Directory.Exists(sPath))
            {
                System.IO.Directory.CreateDirectory(sPath);
            }

            if (isCompress)
            {
                CompressAsJPG(newBmp, fileName, quality);
            }
            else
            {
                fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
                newBmp.Save(fs, System.DrawingCore.Imaging.ImageFormat.Jpeg);
                fs.Close();
                fs.Dispose();
            }
            g.Dispose();
            newBmp.Dispose();

            return sRet;
        }

        public static string SavePic(Bitmap bmp, string fileName, int width, int height)
        {
            string sRet = string.Empty;
            System.IO.FileStream fs;

            Bitmap newBmp;
            Graphics g;
            float oWidth = 0; //ԭʼ���
            float oHeight = 0; //ԭʼ�߶�
            float newWidth = 0; //�¿��
            float newHeight = 0; //�¸߶�

            oWidth = bmp.Width;
            oHeight = bmp.Height;
            if (oWidth > width && oHeight > height)
            {
                if (oWidth / width < oHeight / height)
                {
                    //����Ҫ����
                    newWidth = height / oHeight * oWidth;
                    newHeight = height;
                }
                else
                {
                    //����Ҫ����
                    newHeight = width / oWidth * oHeight;
                    newWidth = width;
                }
            }
            else
            {
                newWidth = oWidth;
                newHeight = oHeight;
            }

            System.DrawingCore.Imaging.ImageAttributes ia;

            ia = new System.DrawingCore.Imaging.ImageAttributes();
            newBmp = new Bitmap(width, height);
            g = Graphics.FromImage(newBmp);
            g.SmoothingMode = System.DrawingCore.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.DrawingCore.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.DrawingCore.Text.TextRenderingHint.AntiAlias;
            g.CompositingQuality = System.DrawingCore.Drawing2D.CompositingQuality.HighQuality;
            g.FillRectangle(Brushes.White, 0, 0, newBmp.Width, newBmp.Height);
            g.DrawImage(bmp, (width - newWidth) / 2, (height - newHeight) / 2, newWidth, newHeight);

            string sPath = fileName.Substring(0, fileName.LastIndexOf("\\"));
            if (!System.IO.Directory.Exists(sPath))
            {
                System.IO.Directory.CreateDirectory(sPath);
            }

            fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
            newBmp.Save(fs, System.DrawingCore.Imaging.ImageFormat.Jpeg);

            fs.Close();
            g.Dispose();
            newBmp.Dispose();
            fs.Dispose();

            return sRet;
        }

        public static byte[] ZoomPicture(Stream inputStream, int thumbWidthSize, int thumbHeightSize, string fileExtension)
        {
            Bitmap bmp = new Bitmap(inputStream);

            Bitmap newBmp;
            Graphics g;

            #region ��ȡ�ȱ������ŵĿ�Ⱥͳ���

            int bitMapWidth = thumbWidthSize;
            int bitMapHeight = thumbHeightSize;

            if (thumbWidthSize > 0 && thumbHeightSize == 0)
            {
                bitMapWidth = thumbWidthSize;
                bitMapHeight = bmp.Height * bitMapWidth / bmp.Width;
            }
            if (thumbWidthSize == 0 && thumbHeightSize > 0)
            {
                bitMapHeight = thumbHeightSize;
                bitMapWidth = bmp.Width * bitMapHeight / bmp.Height;
            }
            if (thumbWidthSize > 0 && thumbHeightSize > 0)
            {
                bitMapWidth = thumbWidthSize;
                bitMapHeight = thumbHeightSize;
            }
            #endregion

            newBmp = new Bitmap(bitMapWidth, bitMapHeight);
            g = Graphics.FromImage(newBmp);
            g.SmoothingMode = System.DrawingCore.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.DrawingCore.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.DrawingCore.Text.TextRenderingHint.AntiAlias;
            g.CompositingQuality = System.DrawingCore.Drawing2D.CompositingQuality.HighQuality;
            g.Clear(System.DrawingCore.Color.White);
            g.FillRectangle(Brushes.White, 0, 0, newBmp.Width, newBmp.Height);
            //g.DrawImage(bmp, x, y, bitMapWidth, bitMapHeight);

            g.DrawImage(bmp, new Rectangle(0, 0, bitMapWidth, bitMapHeight), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);

            MemoryStream stream = null;

            switch (fileExtension.ToLower())
            {
                case ".jpg":
                    stream = FromBitmap(newBmp, ImageFormat.Png);
                    break;
                case ".png":
                    newBmp.MakeTransparent();
                    stream = FromBitmap(newBmp,ImageFormat.Png);
                    break;
                case ".gif":
                    stream = FromBitmap(newBmp, ImageFormat.Png);
                    break;
            }

            return stream.GetBuffer();
        }

        /// <summary>
        /// Bitmapת����Stream
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static MemoryStream FromBitmap(Bitmap bitmap, ImageFormat format)
        {
            var stream = new MemoryStream();

            bitmap.Save(stream, format);
            stream.Position = 0;

            return stream;
        }
    }
}
