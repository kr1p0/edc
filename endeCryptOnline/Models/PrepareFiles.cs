using System.IO.Compression;

namespace endeCryptOnline.Models
{
    public class PrepareFiles
    {
        public PrepareFiles()
        {
        }



        public string Name { get; set; }
        public string FileExtension { get; set; }
        public byte[] ByteArr { get; set; }
        public string ErrorContent { get; set; }
        public string FilePath { get; set; }
        public string Base64String { get; set; }
        public string ZipArchiweDestination { get; set; }
        public int FilesCountSummary { get; set; }
        public long BytesCountSummary { get; set; }

        public List<PrepareFiles> IformFileToByteArr(List<IFormFile> IformFileList)
        {
            List<PrepareFiles> byteArrList = new List<PrepareFiles>();
            try
            {
                foreach (var singleFile in IformFileList)
                {
                    using (var ms = new MemoryStream())
                    {
                        var obj = new PrepareFiles();
                        singleFile.CopyTo(ms);
                        obj.ByteArr = ms.ToArray();

                        var fileName = singleFile.Name;
                        string fileExtension = Path.GetExtension(singleFile.FileName);
                        fileName = Path.GetFileNameWithoutExtension(singleFile.FileName);


                        obj.Name = fileName + fileExtension;
                        byteArrList.Add(obj);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PrepareFiles->IformFileToByteArr: " + ex);
            }

            return byteArrList;
        }

        public static void CleanUploadSpace(string dirName)
        {
            if (string.IsNullOrEmpty(dirName) || !Directory.Exists(dirName))
                return;

            string[] folders = Directory.GetDirectories(dirName);

            foreach (string folder in folders)
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(folder);
                    if (di.CreationTime < DateTime.Now.AddMinutes(-5))
                        di.Delete(true);
                }
                catch (Exception ex) //if exception, try remove first the files, than folder
                {
                    try
                    {
                        string[] files = Directory.GetFiles(folder);
                        foreach (string file in files)
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.CreationTime < DateTime.Now.AddMinutes(-5))
                                fi.Delete();



                            DirectoryInfo di = new DirectoryInfo(folder);
                            if (di.CreationTime < DateTime.Now.AddMinutes(-5))
                                di.Delete(true);
                        }
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("deleteOldFolder Exception: " + ex2.Message);
                    }
                }
            }
        }


        public static string checkDirectory(string originalPath)
        {
            int i = 1;
            string fileExtension = Path.GetExtension(originalPath);

            string fullPath = Path.GetFullPath(originalPath);
            var fullPathWithNoExtension = fullPath.Substring(0, fullPath.LastIndexOf(fileExtension));

            string addToName = "";
            FileInfo dir0 = new FileInfo(originalPath);

            while (dir0.Exists)
            {
                i++;
                addToName = "(" + i + ")";
                string temp = fullPathWithNoExtension + addToName + fileExtension;
                dir0 = new FileInfo(temp);
            }
            var result = fullPathWithNoExtension + addToName + fileExtension;
            return result;
        }



        public static string CreateZipFile(string sourceFolder, string destinationFolder)
        {
            var files = new DirectoryInfo(sourceFolder).GetFiles();
            var zipArchiveName = checkDirectory(destinationFolder + "/ZipArchive.zip");
            using (var stream = System.IO.File.OpenWrite(zipArchiveName))
            using (ZipArchive archive = new ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    archive.CreateEntryFromFile(file.FullName, file.Name, CompressionLevel.Optimal);
                }
            }
            return zipArchiveName.Substring(zipArchiveName.LastIndexOf("uploads"));
        }




    }
}
