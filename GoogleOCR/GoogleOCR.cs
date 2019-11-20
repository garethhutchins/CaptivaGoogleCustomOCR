
using Emc.Captiva.Ocr.Sdk;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Custom.InputAccel.Ocr.GoogleOCR
{
    public class GoogleOCR : Emc.Captiva.Ocr.Sdk.IOcrEngine
    {
        public string Company => "Google";

        public string Version => "0.1";

        public void DoSnippetRecognition(Bitmap bitmap, IOcrSettings settings, IExtractionResultBuilder builder)
        {
            try
            {

                GoogleCalls GC = new GoogleCalls();
                GC.AuthImplicit("hw-recognition");
                MessageBox.Show("Authenticated");
                GC.ProcessImage(bitmap, builder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
       
        public void PopulateSnippetRecognitionDefinition(IOcrSettingDefinitions definitions)
        {
            definitions.AddStringDefinition("EngineName", "GarethGoogle");
            
            
        }
    }

    


}
