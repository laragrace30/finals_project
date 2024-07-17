using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Finals_Project
{
    public partial class Form1 : Form
    {
        private HttpClient httpClient;
        private Dictionary<string, string> languageDictionary;
        private const string DetectLanguageApiKey = "bb3e95c93e9dde3314f34b34a8a13c60";
        public Form1()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PopulateLanguages();

            firstLanguage.selectedIndex = 0;
            secondLanguage.selectedIndex = 1;
        }

        private void PopulateLanguages()
        {
            languageDictionary = new Dictionary<string, string>
            {
                { "English", "en" },
                { "Spanish", "es" },
                { "French", "fr" },
                { "Dutch", "nl" },
                { "Chinese", "zh" },
                { "German", "de" },
                { "Italian", "it" },
                { "Portuguese", "pt" },
                { "Russian", "ru" },
                { "Arabic", "ar" },
                { "Hindi", "hi" },
                { "Bengali", "bn" },
                {"Turkmen","tk" },
                {"Zulu","zu" }
            };

            foreach (var language in languageDictionary.Keys)
            {
                firstLanguage.AddItem(language);
                secondLanguage.AddItem(language);
            }
        }

        private async Task<string> TranslateAsync(string text, string firstlang, string seclang)
        {
            string url = $"http://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={firstlang}|{seclang}";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseJson = await response.Content.ReadAsStringAsync();
                var translationResult = JsonConvert.DeserializeObject<TranslationResponse>(responseJson);

                if (translationResult.ResponseStatus == 200 && translationResult.ResponseData != null)
                {
                    if (string.IsNullOrEmpty(translationResult.ResponseData.TranslatedText))
                    {
                        throw new Exception("Translation not provided by the API.");
                    }

                    return translationResult.TranslatedText;
                }
                else
                {
                    throw new Exception("Translation request failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Translation failed. Error: \r\n" + ex.Message);
                return string.Empty;
            }
        }

        private async Task<string> DetectLanguageAsync(string text)
        {
            string url = $"https://ws.detectlanguage.com/0.2/detect?q={Uri.EscapeDataString(text)}&key={DetectLanguageApiKey}";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            var detectionResult = JsonConvert.DeserializeObject<LanguageDetectionResponse>(responseJson);

            if (detectionResult.Data.Detections.Count > 0)
            {
                return detectionResult.Data.Detections[0].Language;
            }
            else
            {
                throw new Exception("Language detection failed.");
            }
        }

        private void close_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }

        private async void translate_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                string detectedLanguage = await DetectLanguageAsync(textBox1.Text);
                string secLang = languageDictionary[secondLanguage.selectedValue.ToString()];

                if (!languageDictionary.ContainsValue(detectedLanguage))
                {
                    MessageBox.Show("Detected language is not supported.");
                    return;
                }

                if (detectedLanguage == secLang)
                {
                    MessageBox.Show("Please select a different target language.");
                    return;
                }

                string translatedText = await TranslateAsync(textBox1.Text, detectedLanguage, secLang);
                textBox2.Text = translatedText;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Translation failed. Error: \r\n" + ex.Message);
            }
        }

        public class TranslationResponse
        {
            [JsonProperty("responseStatus")]
            public int ResponseStatus { get; set; }

            [JsonProperty("responseData")]
            public TranslationData ResponseData { get; set; }

            public string TranslatedText => ResponseData?.TranslatedText;
        }

        public class TranslationData
        {
            [JsonProperty("translatedText")]
            public string TranslatedText { get; set; }
        }

        public class LanguageDetectionResponse
        {
            [JsonProperty("data")]
            public DetectionData Data { get; set; }
        }

        public class DetectionData
        {
            [JsonProperty("detections")]
            public List<Detection> Detections { get; set; }
        }

        public class Detection
        {
            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("isReliable")]
            public bool IsReliable { get; set; }

            [JsonProperty("confidence")]
            public float Confidence { get; set; }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            SwapLanguages();
        }

        private void SwapLanguages()
        {
            var temp = firstLanguage.selectedIndex;
            firstLanguage.selectedIndex = secondLanguage.selectedIndex;
            secondLanguage.selectedIndex = temp;

            var tempText = textBox1.Text;
            textBox1.Text = textBox2.Text;
            textBox2.Text = tempText;
        }
    }
}
