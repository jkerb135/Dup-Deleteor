using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dup_Destroyer_v2_WPF.Classes.FileScanner;

namespace Dup_Destroyer_v2_WPF.Classes.ImageScanner
{
    class ImageAnalyzer : BackgroundWorker
    {
        private readonly List<FileAttrib> _images = new List<FileAttrib>();
        private readonly List<FileAttrib> _files;

        public ImageAnalyzer(List<FileAttrib> files)
        {
            WorkerReportsProgress = true;
            _files = files;
        }

        private IEnumerable<FileAttrib> AnalyzeImages(IReadOnlyList<FileAttrib> files)
        {
            for (var i = 0; i < files.Count(); i++)
            {
                var f = files[i];
                f.ImageHash = ImageHashing.AverageHash(f.FilePath);
                for (var j = i + 1; j < files.Count(); j++)
                {
                    var f2 = files[j];
                    f2.ImageHash = ImageHashing.AverageHash(f2.FilePath);
                    ReportProgress(0, new Tuple<string, string>("false", "Comparing " + f.FileName + " with " + f2.FileName));
                    if (ImageHashing.Similarity(f.ImageHash, f2.ImageHash) >= 90) continue;
                    if (!_images.Contains(f2))
                    {
                        _images.Add(f2);
                    }
                    Thread.Sleep(50);
                }
            }
            return _files.GroupBy(x => x.ImageHash).Where(x => x.Count() > 1).SelectMany(x => x).OrderByDescending(x => x.FileName);
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            e.Result = AnalyzeImages(_files);
        }

        class HashingComparison : IEqualityComparer<FileAttrib>
        {
            public bool Equals(FileAttrib p1, FileAttrib p2)
            {
                return ImageHashing.Similarity(p1.ImageHash, p2.ImageHash) >= 95;
            }

            public int GetHashCode(FileAttrib p)
            {
                return base.GetHashCode();
            }
        }
    }
}
