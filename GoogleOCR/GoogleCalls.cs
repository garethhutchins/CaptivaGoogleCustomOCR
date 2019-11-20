using Emc.Captiva.Ocr.Sdk;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Custom.InputAccel.Ocr.GoogleOCR
{
    class GoogleCalls
    {
        public object AuthImplicit(string projectId)
        {
            // If you don't specify credentials when constructing the client, the
            // client library will look for credentials in the environment.
            
            var credential = GoogleCredential.GetApplicationDefault();
            var storage = StorageClient.Create(credential);
            // Make an authenticated API request.
            var buckets = storage.ListBuckets(projectId);
            foreach (var bucket in buckets)
            {
                Console.WriteLine(bucket.Name);
            }
            return null;
        }


        public class vertices
        {
            public int x;
            public int y;
        }
        public class Symbol
        {
            public string text;
            public float confidence;
            public vertices[] bound = new vertices[4];
        }

        public void ProcessImage(Bitmap bitmap, IExtractionResultBuilder builder)
        {

            byte[] bytes = (byte[])TypeDescriptor.GetConverter(bitmap).ConvertTo(bitmap, typeof(byte[]));
            var image = Google.Cloud.Vision.V1.Image.FromBytes(bytes);
            var client = ImageAnnotatorClient.Create();
            var response = client.DetectDocumentText(image);

            foreach (var page in response.Pages)

            {
                foreach (var block in page.Blocks)
                {
                    foreach (var paragraph in block.Paragraphs)
                    {
                        Console.WriteLine(string.Join("\n", paragraph.Words));
                        foreach (var word in paragraph.Words)
                        {
                            foreach (var symbol in word.Symbols)
                            {
                                Symbol s = new Symbol();
                                s.text = symbol.Text;
                                s.confidence = symbol.Confidence;
                                
                                s.bound[0] = new vertices();
                                s.bound[0].x = symbol.BoundingBox.Vertices[0].X;
                                s.bound[0].y = symbol.BoundingBox.Vertices[0].Y;
                                s.bound[1] = new vertices();
                                s.bound[1].x = symbol.BoundingBox.Vertices[1].X;
                                s.bound[1].y = symbol.BoundingBox.Vertices[1].Y;
                                s.bound[2] = new vertices();
                                s.bound[2].x = symbol.BoundingBox.Vertices[2].X;
                                s.bound[2].y = symbol.BoundingBox.Vertices[2].Y;
                                s.bound[3] = new vertices();
                                s.bound[3].x = symbol.BoundingBox.Vertices[3].X;
                                s.bound[3].y = symbol.BoundingBox.Vertices[3].Y;
                                MessageBox.Show(s.bound[0].x.ToString() + " " + s.bound[0].y.ToString() + ", " + s.bound[1].x.ToString() + " " + s.bound[1].y.ToString() + ", " + s.bound[2].x.ToString() + " " + s.bound[2].y.ToString() + ", " + s.bound[3].x.ToString() + " " + s.bound[3].y.ToString() + ", ");
                                MessageBox.Show("x=" + s.bound[1].x + " y=" + s.bound[3].y + "width= " + Math.Abs(s.bound[0].x - s.bound[1].x) + " height=" + Math.Abs(s.bound[0].y - s.bound[2].y));
                                Rectangle bounds = new Rectangle(s.bound[1].x, s.bound[3].y, Math.Abs(s.bound[0].x - s.bound[1].x), Math.Abs(s.bound[0].y - s.bound[2].y));
                                builder.AddNewCharacter(s.text, (int)s.confidence, bounds);

                                if (symbol.Property.DetectedBreak != null)
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

    }

}
