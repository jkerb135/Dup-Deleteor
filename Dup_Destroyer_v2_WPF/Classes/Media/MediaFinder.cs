using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DupDestroyer_v2.Classes;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Audio.NAudio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;

namespace Dup_Destroyer_v2_WPF.Classes.Media
{
    class MediaFinder : BackgroundWorker
    {
        private static readonly string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private readonly DirectoryInfo _path;
        private readonly string[] _musicFileFilters = { "*.mp3", "*.ogg", "*.flac", "*.wav" };
        private readonly FingerprintService _soundFingerprinting = new FingerprintService();
        private readonly IModelService modelService = new InMemoryModelService();
        private readonly IAudioService audioService = new NAudioService();
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();

        public MediaFinder(DirectoryInfo path)
        {
            WorkerReportsProgress = true;
            _path = path;
        }

        public List<FileAttrib> GetFiles(DirectoryInfo rootDirectoryInfo)
        {
            var filesList = new List<FileAttrib>();
            var listOfFilters = _musicFileFilters as IList<string> ?? _musicFileFilters.ToList();
            ReportProgress(1, new Tuple<string, string>("true", ""));
            ReportProgress(0, new Tuple<string, string>("false", "Gathering File Data in " + rootDirectoryInfo.FullName));
            Thread.Sleep(100);
            foreach (var filter in listOfFilters)
            {
                try
                {
                    var files = rootDirectoryInfo.GetFiles(filter, SearchOption.TopDirectoryOnly);
                    ReportProgress(files.Count(), new Tuple<string, string>("true", ""));
                    foreach (var file in files)
                    {
                        try
                        {
                            ReportProgress(0,
                                new Tuple<string, string>("false", "Adding files from " + rootDirectoryInfo.FullName));
                            filesList.Add(FileAttrib.GetFileInfo(file));
                            Thread.Sleep(100);
                        }
                        catch (PathTooLongException)
                        {
                            /*255 item range is too long*/
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    /*Admin Access*/
                }
                catch (SecurityException)
                {
                    /*you don't have the permission of viewing this files*/
                }
                catch (DirectoryNotFoundException)
                {
                    /*directory not found*/
                }
                catch (ArgumentNullException)
                {
                    /*bad parameters*/
                }
            }
            try
            {
                var directories = rootDirectoryInfo.GetDirectories();
                foreach (var directory in directories)
                {
                    try
                    {
                        filesList.AddRange(GetFiles(directory));
                    }
                    catch (PathTooLongException)
                    {
                        //continue the foreach
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
            catch (DirectoryNotFoundException)
            {
                /*directory not found*/
                return filesList;
            }

            return filesList;
        }


        public void StoreAudioFileFingerprintsInDatabaseForLaterRetrieval(FileAttrib file)
        {
            TrackData track = new TrackData("GBBKS1200164", "Adele", "Skyfall", "Skyfall", 2012, 290);

            // store track metadata in the database
            var trackReference = modelService.InsertTrack(track);

            // create sub-fingerprints and its hash representation
            var hashedFingerprints = fingerprintCommandBuilder
                                        .BuildFingerprintCommand()
                                        .From(file.FilePath)
                                        .WithDefaultFingerprintConfig()
                                        .UsingServices(audioService)
                                        .Hash()
                                        .Result;

            // store sub-fingerprints and its hash representation in the database 
            modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }

        public TrackData GetBestMatchForSong(string queryAudioFile)
        {
            const int secondsToAnalyze = 10; // number of seconds to analyze from query file
            const int startAtSecond = 0; // start at the begining

            // query the underlying database for similar audio sub-fingerprints
            var queryResult = queryCommandBuilder.BuildQueryCommand()
                                                 .From(queryAudioFile, secondsToAnalyze, startAtSecond)
                                                 .WithDefaultConfigs()
                                                 .UsingServices(modelService, audioService)
                                                 .Query()
                                                 .Result;
            if (queryResult.IsSuccessful)
            {
                return queryResult.BestMatch.Track; // successful match has been found
            }

            return null; // no match has been found
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            var files = GetFiles(_path);
            foreach (var file in files)
            {
                StoreAudioFileFingerprintsInDatabaseForLaterRetrieval(file);
            }
            //CreateFingerprintSignaturesFromFile(files);
        }

        
    }
}
