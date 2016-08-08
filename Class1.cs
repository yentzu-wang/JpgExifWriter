/*
Built by Frank Wang, aka ZombieWang @GitHub, August 2016

Copyright (C) 2016 Yen-Tzu Wang

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Windows.Media.Imaging;
using System.IO;

namespace JpgExifWriter
{
    public class Class1
    {

        /// <summary>
        /// Adding/overriding comment, date of taken and GPS infomation from jpg files
        /// </summary>
        /// <param name="imageFilePath">imageFilePath(with file name)</param>
        /// <param name="comments">comment</param>
        /// <param name="wgs84_X">GPS_X</param>
        /// <param name="wgs84_Y">GPS_Y</param>
        /// <param name="photoDate">date of taken</param>
        private void addImageComment(string imageFilePath, string comments, string wgs84_X, string wgs84_Y, string photoDate)
        {
            BitmapDecoder decoder = null;
            BitmapFrame bitmapFrame = null;
            BitmapMetadata metadata = null;
            FileInfo originalImage = new FileInfo(imageFilePath);

            if (File.Exists(imageFilePath))
            {
                // load the jpg file with a JpegBitmapDecoder    
                using (Stream jpegStreamIn = File.Open(imageFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    decoder = new JpegBitmapDecoder(jpegStreamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                }

                bitmapFrame = decoder.Frames[0];
                metadata = (BitmapMetadata)bitmapFrame.Metadata;

                if (bitmapFrame != null)
                {
                    BitmapMetadata metaData = (BitmapMetadata)bitmapFrame.Metadata.Clone();

                    if (metaData != null)
                    {
                        // modify the metadata   
                        metaData.Comment = comments;

                        if (string.IsNullOrEmpty(metaData.DateTaken))
                        {
                            //metaData.DateTaken = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            metaData.DateTaken = photoDate;
                        }

                        // write GPS infomation
                        if (metaData.GetQuery("/app1/ifd/gps/{ushort=2}") == null && metaData.GetQuery("/app1/ifd/gps/{ushort=4}") == null)
                        {
                            metaData.SetQuery("/app1/ifd/gps/{ushort=4}", ConvertULONG(Convert.ToDouble(wgs84_X)));
                            metaData.SetQuery("/app1/ifd/gps/{ushort=2}", ConvertULONG(Convert.ToDouble(wgs84_Y)));
                        }
                        // get an encoder to create a new jpg file with the new metadata.      
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapFrame, bitmapFrame.Thumbnail, metaData, bitmapFrame.ColorContexts));
                        //string jpegNewFileName = Path.Combine(jpegDirectory, "JpegTemp.jpg");

                        // Delete the original
                        originalImage.Delete();

                        // Save the new image 
                        using (Stream jpegStreamOut = File.Open(imageFilePath, FileMode.CreateNew, FileAccess.ReadWrite))
                        {
                            encoder.Save(jpegStreamOut);
                        }
                    }
                }
            }
        }

        private ulong[] ConvertULONG(double Coordinate)
        {
            ulong degree = 0;
            ulong min = 0;
            ulong sec = 0;

            double temp = Math.Round((double)1 / 60 * 10000000000000000000) / 10000000000000000000;

            degree = Convert.ToUInt64(((int)Coordinate)) + 0x100000000;
            min = Convert.ToUInt64((int)((Coordinate % 1) * 60)) + 0x100000000;
            sec = Convert.ToUInt64(((Coordinate % 1) % temp) * 3600) * 100 + 0x6400000000;


            double tRes = Coordinate;

            ulong[] result = { degree, min, sec };
            return result;
        }

    }
}
