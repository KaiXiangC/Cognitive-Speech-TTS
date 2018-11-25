
using System;
using System.IO;
using System.Media;
using System.Threading;
using CognitiveServicesTTS;


namespace TTSSample
{
    internal class Program
    {
        
    /// <summary>
    /// This method is called once the audio returned from the service.
    /// It will then attempt to play that audio file.
    /// Note that the playback will fail if the output audio format is not pcm encoded.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The <see cref="GenericEventArgs{Stream}"/> instance containing the event data.</param>
    private static void PlayAudio(object sender, GenericEventArgs<Stream> args)
        {
            Console.WriteLine(args.EventData);

            // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
            // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
            SoundPlayer player = new SoundPlayer(args.EventData);
            player.PlaySync();
            args.EventData.Dispose();
        }

        /// <summary>
        /// Handler an error when a TTS request failed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GenericEventArgs{Exception}"/> instance containing the event data.</param>
        private static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            Console.WriteLine("Unable to complete the TTS request: [{0}]", e.ToString());
        }

        

        private static void Main(string[] args)
        {
            String apikey = "";
            String input = "";
            if (args.Length <2)
            {
                Console.WriteLine("Usage: speak [API key] [What to say]");
                return;
            }
            else {
                apikey = args[0];
                for (int i=1;i<args.Length;i++)
                    input = input + args[i] + " ";
                input = input.TrimEnd(' ');
            }

            Console.WriteLine("input=" +input);

            Console.WriteLine("Starting Authtentication");
            string accessToken;

            Authentication auth = new Authentication("https://api.cognitive.microsoft.com/sts/v1.0/issueToken", apikey);
            
            try
            {
                accessToken = auth.GetAccessToken();
                Console.WriteLine("Token: {0}\n", accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed authentication.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("Starting TTSSample request code execution.");
            // Synthesis endpoint for old Bing Speech API: https://speech.platform.bing.com/synthesize
            // For new unified SpeechService API: https://westus.tts.speech.microsoft.com/cognitiveservices/v1
            // Note: new unified SpeechService API synthesis endpoint is per region
            string requestUri = "https://speech.platform.bing.com/synthesize";
            var cortana = new Synthesize();

            cortana.OnAudioAvailable += PlayAudio;
            cortana.OnError += ErrorHandler;

            // Reuse Synthesize object to minimize latency
            cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                // Text to be spoken.
                Text = input,
                VoiceType = Gender.Female,
                // Refer to the documentation for complete list of supported locales.
                Locale = "zh-TW",
                // You can also customize the output voice. Refer to the documentation to view the different
                // voices that the TTS service can output.
                // VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, Jessa24KRUS)",
                VoiceName = "Microsoft Server Speech Text to Speech Voice (zh-TW, Yating, Apollo)",
                // VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",
            
                // Service can return audio in different output format.
                OutputFormat = AudioOutputFormat.Riff24Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken,
            }).Wait();
        }
    }
}