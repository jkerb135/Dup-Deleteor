﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Image = System.Drawing.Image;

// Created in 2012 by Jakob Krarup (www.xnafan.net).
// Use, alter and redistribute this code freely,
// but please leave this comment :)


namespace Dup_Destroyer_v2_WPF.Classes.ImageScanner
{

    /// <summary>
    /// A class with extensionmethods for comparing images
    /// </summary>
    public static class ImageTool
    {
        private static PathGrayscaleTupleComparer Comparer = new PathGrayscaleTupleComparer();

        /// <summary>
        /// Gets the difference between two images as a percentage
        /// </summary>
        /// <returns>The difference between the two images as a percentage</returns>
        /// <param name="image1Path">The path to the first image</param>
        /// <param name="image2Path">The path to the second image</param>
        /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
        /// <returns>The difference between the two images as a percentage</returns>
        public static float GetPercentageDifference(string image1Path, string image2Path, byte threshold = 3)
        {
            if (CheckIfFileExists(image1Path) && CheckIfFileExists(image2Path))
            {
                Image img1 = Image.FromFile(image1Path);
                Image img2 = Image.FromFile(image2Path);

                float difference = img1.PercentageDifference(img2, threshold);

                img1.Dispose();
                img2.Dispose();

                return difference;
            }
            else return -1;
        }

        /// <summary>
        /// Gets the difference between two images as a percentage
        /// </summary>
        /// <returns>The difference between the two images as a percentage</returns>
        /// <param name="image1Path">The path to the first image</param>
        /// <param name="image2Path">The path to the second image</param>
        /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
        /// <returns>The difference between the two images as a percentage</returns>
        public static float GetBhattacharyyaDifference(string image1Path, string image2Path)
        {
            if (CheckIfFileExists(image1Path) && CheckIfFileExists(image2Path))
            {
                Image img1 = Image.FromFile(image1Path);
                Image img2 = Image.FromFile(image2Path);

                float difference = img1.BhattacharyyaDifference(img2);

                img1.Dispose();
                img2.Dispose();

                return difference;
            }
            else return -1;
        }

        /// <summary>
        /// Find all duplicate images from in list
        /// </summary>
        /// <param name="pathsOfPossibleDuplicateImages">The paths to the images to check for duplicates</param>
        /// <param name="imgBar"></param>
        /// <returns>A list of all the duplicates found, collected in separate Lists (one for each distinct image found)</returns>
        public static List<List<string>> GetDuplicateImages(IEnumerable<string> pathsOfPossibleDuplicateImages)
        {
            var imagePathsAndGrayValues = GetSortedGrayscaleValues(pathsOfPossibleDuplicateImages);
            var duplicateGroups = GetDuplicateGroups(imagePathsAndGrayValues);

            return duplicateGroups.Select(list => list.Select(tuple => tuple.Item1).ToList()).ToList();
        }

        #region Helpermethods

        private static List<Tuple<string, byte[,]>> GetSortedGrayscaleValues(IEnumerable<string> pathsOfPossibleDuplicateImages)
        {
            var imagePathsAndGrayValues = new List<Tuple<string, byte[,]>>();
            foreach (var imagePath in pathsOfPossibleDuplicateImages)
            {
                using (var image = Image.FromFile(imagePath))
                {
                    var grayValues = image.GetGrayScaleValues();
                    var tuple = new Tuple<string, byte[,]>(imagePath, grayValues);
                    imagePathsAndGrayValues.Add(tuple);
                }
            }

            imagePathsAndGrayValues.Sort(Comparer);
            return imagePathsAndGrayValues;
        }

        private static IEnumerable<List<Tuple<string, byte[,]>>> GetDuplicateGroups(IEnumerable<Tuple<string, byte[,]>> imagePathsAndGrayValues)
        {
            var duplicateGroups = new List<List<Tuple<string, byte[,]>>>();
            var currentDuplicates = new List<Tuple<string, byte[,]>>();
            foreach (var tuple in imagePathsAndGrayValues)
            {
                if (currentDuplicates.Any() && Comparer.Compare(currentDuplicates.First(), tuple) != 0)
                {
                    if (currentDuplicates.Count > 1)
                    {
                        duplicateGroups.Add(currentDuplicates);
                        currentDuplicates = new List<Tuple<string, byte[,]>>();
                    }
                    else
                    {
                        currentDuplicates.Clear();
                    }
                }

                currentDuplicates.Add(tuple);
            }
            if (currentDuplicates.Count > 1)
            {
                duplicateGroups.Add(currentDuplicates);
            }
            return duplicateGroups;
        }

        private static bool CheckIfFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File '" + filePath + "' not found!");
            }
            return true;
        }
        #endregion

    }
}