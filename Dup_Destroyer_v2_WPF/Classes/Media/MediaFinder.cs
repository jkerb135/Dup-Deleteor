using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using Dup_Destroyer_v2_WPF.Classes.FileScanner;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Audio.NAudio;

namespace Dup_Destroyer_v2_WPF.Classes.Media
{
    class MediaFinder : BackgroundWorker
    {
        private static readonly string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private readonly List<FileAttrib> _audio = new List<FileAttrib>();
        private readonly IEnumerable<FileAttrib> _files;

        public MediaFinder(IEnumerable<FileAttrib> files)
        {
            WorkerReportsProgress = true;
            _files = files;
        }

        private IEnumerable<FileAttrib> AnalyzeAudio(List<FileAttrib> files)
        {

            for (var i = 0; i < files.Count(); i++)
            {
                var f = files[i];
                f.MediaHash = ConvertToMono(f.FilePath);

                Console.WriteLine(String.Join(" ", f.MediaHash));
                for (var j = i + 1; j < files.Count(); j++)
                {
                    var f2 = files[j];
                    //f2.MediaHash = ConvertToMono(f2.FilePath);

                    ReportProgress(0,
                        new Tuple<string, string>("false", "Comparing " + f.FileName + " with " + f2.FileName));
                   //f2.MediaHash = ConvertToMono(f2.FilePath);

                    if (!_audio.Contains(f2))
                    {
                        _audio.Add(f2);
                    }
                    Thread.Sleep(50);
                }
            }
            return _audio;
        }

        private static float[] ConvertToMono(string path)
        {
            var service = new NAudioService();
            return service.ReadMonoSamplesFromFile(path, 5512, 10, 0);
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            e.Result = AnalyzeAudio(_files as List<FileAttrib>);
        }
    }
}
