using CommandLine;
using CommandLine.Text;

namespace MotionPhoto
{
    public class Options
    {
        [Option('s', "secrets", Required = false, DefaultValue = "client_secret.json",
          HelpText = "Specify filename with Google credentials")]
        public string SecretsFilename { get; set; }

        [Option('u', "url", Required = true,
         HelpText = "D-Link base Url to camera (e.g. https://mycamera.com:30000)")]
        public string DLinkUrl { get; set; }

        [Option('l', "login", Required = true,
          HelpText = "D-Link login name")]
        public string DLinkLogin { get; set; }

        [Option('p', "password", Required = true,
          HelpText = "D-Link password")]
        public string DLinkPassword { get; set; }

        [Option('d', "logFile", Required = false,
          HelpText = "Log filename (e.g. log.txt")]
        public string LogFilename { get; set; }
        
        [Option('v', "verbose", DefaultValue = false,
          HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

}
