using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacadeFELogic.Helper
{
    public static class AsciiHelper
    {
        public static List<string> ConvertToASCII(FacadeSystem facadeSystem, 
                                                  out List<string> nodeASCII, 
                                                  out List<string> elementFeASCII,
                                                  out List<string> boundaryTranASCII,
                                                  out List<string> boundaryRotASCII,
                                                  out List<string> hingeASCII,
                                                  out List<string> loadASCII)
        {
            //get ascii lists
            nodeASCII = facadeSystem.GetNodeASCII();
            elementFeASCII = facadeSystem.GetElementFeASCII();
            boundaryTranASCII = facadeSystem.GetBoundaryTranASCII();
            boundaryRotASCII = facadeSystem.GetBoundaryRotASCII();
            hingeASCII = facadeSystem.GetHingesASCII();
            loadASCII = facadeSystem.GetLoadsASCII();

            //combine them
            return CombineASCII("...", nodeASCII, elementFeASCII, boundaryTranASCII, boundaryRotASCII, hingeASCII, loadASCII);
        }

        private static List<string> CombineASCII(string separator, params List<string>[] asciiLists)
        {
            List<string> combinedASCII = new List<string>();

            foreach (List<string> asciiList in asciiLists)
            {
                combinedASCII.AddRange(asciiList);
                combinedASCII.Add(separator);
            }

            return combinedASCII;
        }

        public static void exportASCII(List<string> listASCII, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllLines(filePath, listASCII);
        }

        public static string MakeFilePath(string folderPath, string filename, string extension)
        {
            string filenameWithtExtension = filename + extension;

            if (Directory.Exists(folderPath))
            {
                return Path.Combine(folderPath, filenameWithtExtension);
            }

            return null;
        }
    }
}
