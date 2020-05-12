using System;
using Emc.Captiva.Ocr.Sdk;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Google.Api.Gax;
using Google.Apis.Storage.v1.Data;
using Image = Google.Cloud.Vision.V1.Image;

namespace Custom.InputAccel.Ocr.GoogleOCR
{
    class GoogleCalls
    {
        public void AuthImplicit(string projectId)
        {
            // If you don't specify credentials when constructing the client, the
            // client library will look for credentials in the environment.
            GoogleCredential credential = GoogleCredential.GetApplicationDefault();
            
            /* it doesn't seem to sue this anymore
            using (StorageClient storage = StorageClient.Create(credential))
            {
                // Make an authenticated API request.
                
                PagedEnumerable<Buckets, Bucket> buckets = storage.ListBuckets(projectId);
                foreach (Bucket bucket in buckets)
                {
                    MessageBox.Show(bucket.Name);
                }
            }
            */
        }

        public struct Vertices
        {
            public int X;
            public int Y;
        }

        public class Character
        {
            public string Text;
            public float Confidence;
            public readonly Vertices[] Bound = new Vertices[4];
        }

        public void ProcessImage(Bitmap bitmap, IExtractionResultBuilder builder)
        {
            Image image = ConvertBitmapToGoogleImage(bitmap);
            //MessageBox.Show("Here");
            ImageAnnotatorClient client = ImageAnnotatorClient.Create();

            TextAnnotation response = client.DetectDocumentText(image);
            //MessageBox.Show(response.Text);
            if (response == null)
            {
                return;
            }

            //MessageBox.Show(response.Text);
            foreach (Page page in response.Pages)
            {
                foreach (Block block in page.Blocks)
                {
                    foreach (Paragraph paragraph in block.Paragraphs)
                    {
                        foreach (Word word in paragraph.Words)
                        {
                            foreach (Symbol symbol in word.Symbols)
                            {
                                Character s = new Character();

                                s.Text = symbol.Text;
                                s.Confidence = symbol.Confidence;

                                s.Bound[0] = new Vertices();
                                s.Bound[0].X = symbol.BoundingBox.Vertices[0].X;
                                s.Bound[0].Y = symbol.BoundingBox.Vertices[0].Y;
                                s.Bound[1] = new Vertices();
                                s.Bound[1].X = symbol.BoundingBox.Vertices[1].X;
                                s.Bound[1].Y = symbol.BoundingBox.Vertices[1].Y;
                                s.Bound[2] = new Vertices();
                                s.Bound[2].X = symbol.BoundingBox.Vertices[2].X;
                                s.Bound[2].Y = symbol.BoundingBox.Vertices[2].Y;
                                s.Bound[3] = new Vertices();
                                s.Bound[3].X = symbol.BoundingBox.Vertices[3].X;
                                s.Bound[3].Y = symbol.BoundingBox.Vertices[3].Y;

                                Rectangle bounds = new Rectangle(s.Bound[0].X, s.Bound[0].Y, s.Bound[1].X - s.Bound[0].X, s.Bound[3].Y - s.Bound[0].Y);
                                builder.AddNewCharacter(s.Text, (int)(Math.Round(s.Confidence * 100)), bounds);

                                if (symbol.Property?.DetectedBreak != null)
                                {
                                    switch (symbol.Property.DetectedBreak.Type)
                                    {
                                        case TextAnnotation.Types.DetectedBreak.Types.BreakType.EolSureSpace:
                                            builder.AddNewLine();
                                            break;
                                        case TextAnnotation.Types.DetectedBreak.Types.BreakType.Hyphen:
                                            break;
                                        case TextAnnotation.Types.DetectedBreak.Types.BreakType.LineBreak:
                                            builder.AddNewLine();
                                            break;
                                        case TextAnnotation.Types.DetectedBreak.Types.BreakType.Space:
                                            builder.AddWhiteSpace();
                                            break;
                                        case TextAnnotation.Types.DetectedBreak.Types.BreakType.SureSpace:
                                            builder.AddWhiteSpace();
                                            break;
                                        case TextAnnotation.Types.DetectedBreak.Types.BreakType.Unknown:
                                            builder.AddWhiteSpace();
                                            break;
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        Image ConvertBitmapToGoogleImage(Bitmap bitmap)
        {
            using (var bitmapStream = new MemoryStream())
            {
                bitmap.Save(bitmapStream, ImageFormat.Bmp);
                bitmapStream.Position = 0;

                return Image.FromStream(bitmapStream);
            }
        }
    }
}
