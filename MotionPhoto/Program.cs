using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MotionPhoto
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                var logMessage = new Action<string>((string m) =>
                {
                    if(options.LogFilename != null)
                        File.AppendAllLines(options.LogFilename, new List<string> { m });
                    if(options.Verbose)
                        Console.WriteLine(m);
                });
                var startMessage = "Start webcam monitor motion monitor";
                logMessage(startMessage);
                if (options.SecretsFilename != null)
                    Picasa.ClientSecretFilename = options.SecretsFilename;
                var picasa = new Picasa();
                try
                {
                    picasa.Initialize();
                }
                catch(Exception e)
                {
                    logMessage($"Google credentials are not valid. Message {e.Message}");
                    return;
                }
                var baseUrl = options.DLinkUrl;
                var login = options.DLinkLogin;
                var password = options.DLinkPassword;
                logMessage($"Webcam base url {baseUrl} to access camera");
                var webCam = new WebCam(baseUrl, login, password);
                webCam.LogMessage = logMessage;
                webCam.NewImage = data =>
                {
                    try
                    {
                        picasa.Upload(data);
                    }
                    catch(Exception e)
                    {
                        logMessage($"Uploading image failed. Message {e.Message}");
                    }
                };
                while (true)
                {
                    try
                    {
                        logMessage("Webcam start listening for motion events");
                        webCam.Listen().Wait();
                        logMessage("Webcam stoppedlistening for motion events");
                    }
                    catch (Exception e)
                    {
                        logMessage($"Webcam communcation failed with error {e.Message}");
                        logMessage($"Waiting before connecting again");
                        System.Threading.Thread.Sleep(10000);
                    }
                    finally
                    {
                        logMessage("Restarting...");
                    }
                }
            }
        }
    }
}
