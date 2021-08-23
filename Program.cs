using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TeamNMSL.SDVXAudioExtractor
{
    
    class Metadata {
        public string title;
        public string artist;
    }
    class S3VConverterForSDVX
    {
        static string ReplaceFileName(string s)
        {
            return s.Replace("/", " ").Replace("\\", " ").Replace("*", "x").Replace("\"", "“").Replace("<", "《").Replace(">", "》").Replace("|", "l").Replace("?", "？").Replace(":", "：");
        }
        static void Main(string[] args)
        {
            if (!File.Exists(@".\ffmpeg.exe"))
            {
                Console.WriteLine("Did not find ffmpeg.exe in the program directory to make the program work properly");
                Console.ReadKey();
                Environment.Exit(0);
            }
            Cli();
        }

        static private void Cli()
        {
            Console.WriteLine("1.Batch convert s3v to mp3\n2.Convert a s3v file to mp3\nothers exit");
            switch (Console.ReadLine().Replace("\n", "").Replace("\r", ""))
            {
                case "1":
                    BatchMode();
                    break;
                case "2":
                    SingleFileMode();
                    break;
                default:
                    Environment.Exit(0);
                    break;
            }
        }
        static private void SoundFormatConverter(string SoundFullPath,string SoundOutputPath,Metadata mdt)
        {
            string Arg = $"-i {SoundFullPath} -metadata title=\"{mdt.title}\" -metadata artist=\"{mdt.artist}\" -q:a 0 \"{SoundOutputPath}\"";
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = false,
                    FileName = @".\ffmpeg.exe",
                    Arguments = Arg,
                    RedirectStandardInput = true

                }
            };
            proc.Start();
            while (true)
            {
                if (proc.HasExited)
                {
                    return;
                }
            }
            
        }

        static private void BatchMode() {
            Console.WriteLine("Please put in the Output fold path");
            string OutputPath = Console.ReadLine().Replace("\n", "").Replace("\r", "");
            Console.WriteLine("Please put in the path of mdb.xml");
            string MDB = Console.ReadLine().Replace("\n", "").Replace("\r", "");
            Console.WriteLine("Please put in the path of music(gameroot\\data\\music)");
            string SDVXMusicPath = Console.ReadLine().Replace("\n", "").Replace("\r", "");
            BatchOutput(OutputPath, SDVXMusicPath, MDB);
        }

        static private void BatchOutput(string OutputPath,string SDVXSoundPath,string XMLPath) {
            try
            {
                string xml2json(string xml)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    return JsonConvert.SerializeXmlNode(doc);
                }
                
                
                Encoding shiftjis = Encoding.GetEncoding(932);
                string t = File.ReadAllText(XMLPath, shiftjis);
                t = xml2json(t);
                JObject slst = (JObject)JsonConvert.DeserializeObject(t);
                
                if (!SDVXSoundPath.EndsWith("\\"))
                {
                    SDVXSoundPath += "\\";
                }
                if (!OutputPath.EndsWith("\\"))
                {
                    OutputPath += "\\";
                }
                string artist;
                string title;
                string Fold;
                string Name;
                string id;
                string[] spsound = new string[] { "1n", "2a", "3e", "4i", "4g", "4h", "4v", "4m" };
                Console.WriteLine("Started,Please do what you should do and the program will convert songs,you can go back after a long time.");
                foreach (var songinfo in slst["mdb"]["music"])
                {
                    artist = songinfo["info"]["artist_name"].ToString();
                    title = songinfo["info"]["title_name"].ToString();
                    id = songinfo["@id"].ToString();
                    while (id.Length!=4)
                    {
                        id = "0" + id;
                    }
                    Name = id + "_" + songinfo["info"]["ascii"].ToString();
                    Fold = SDVXSoundPath + Name+"\\";
                    Console.WriteLine($"{id}  {title}  {artist}");
                    if (File.Exists(Fold+Name+".s3v"))
                    {
                        
                            SoundFormatConverter(Fold + Name + ".s3v", OutputPath + ReplaceFileName(id + "_" + artist + "-" + title + ".mp3"), new Metadata
                            {
                                title = title,
                                artist = artist
                            });
                        
                        
                    }

                    foreach (var diff in spsound)
                    {
                        if (File.Exists(Fold + Name + $"_{diff}.s3v"))
                        {
                           
                                SoundFormatConverter(Fold + Name + $"_{diff}.s3v", OutputPath + ReplaceFileName(id + "_" + artist + "-" + title + $"_{diff}.mp3"), new Metadata
                                {
                                    title = title,
                                    artist = artist
                                });
                            
                            
                        }
                    }

                    
                }
                Console.WriteLine("All actions performed");
                Console.ReadKey();
                Console.Clear();
                Cli();
                return;

            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                Console.ReadKey();
                Console.Clear();
                Cli();
                return;
            }
        }

        static private void SingleFileMode() {
            Console.WriteLine("Please put in the path of s3v file you want to convert");
            string Source = Console.ReadLine().Replace("\n", "").Replace("\r", "");
            if (!File.Exists(Source))
            {
                Console.WriteLine("File not found");
                Console.ReadKey();
                Console.Clear();
                Cli();
                return;
            }
            Console.WriteLine("Please put in the full output path of the mp3 file");
            string Target = Console.ReadLine().Replace("\n", "").Replace("\r", "");
            if (File.Exists(Target))
            {
                Console.WriteLine("File have already exist");
                Console.ReadKey();
                Console.Clear();
                Cli();
                return;
            }
            SoundFormatConverter(Source, Target, new Metadata()
            {
                title = " ",
                artist=" "
            });
            Console.WriteLine("Action performed");
            Console.ReadKey();
            Console.Clear();
            Cli();
        }
            
        
    }
}
