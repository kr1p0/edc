using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace endeCryptOnline.Pages
{
    [DisableRequestSizeLimit] //otherwise the size limit is ~30MB
    public class IndexModel : PageModel
    {
        [BindProperty]
        public int  DisplaySummaryFiles { get; set; }
        [BindProperty]
        public long DisplaySummaryBytes { get; set; }

        private readonly IWebHostEnvironment _rootEnv;

        public IndexModel(IWebHostEnvironment env)
        {
            _rootEnv = env; //for root path
        }

       
        enum messageType
        {
            error,
            success
        }
        class messageJs
        {
            public messageJs(string type, string content)
            {
                MsgType = type;
                Content = content;
            }
            public string MsgType { get; set; }
            public string Content { get; set; }
        }



        public class FileProcessedSummary
        {
            public FileProcessedSummary(int filesCount, long bytesCount)
            {
                FilesCount = filesCount;
                BytesCount = bytesCount;
            }
            public int FilesCount { get; set; }
            public long BytesCount { get; set; }
        }
        private (int FilesCount , long BytesCount) SaveSummaryProcessed(string rootDestiantion, FileProcessedSummary filesSummary)
        {
            try
            {
                var FilePath = Path.Combine(rootDestiantion, "summary/");
                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);
                FilePath = Path.Combine(FilePath, "summaryData.json");

                //Read data if exist 
                if (System.IO.File.Exists(FilePath))
                {
                    var readFile = System.IO.File.ReadAllText(FilePath);
                    var readFileData = JsonSerializer.Deserialize<FileProcessedSummary>(readFile);
                    filesSummary.FilesCount += readFileData.FilesCount;
                    filesSummary.BytesCount += readFileData.BytesCount;
                }
                if(filesSummary.FilesCount > 0 || filesSummary.BytesCount > 0)
                {
                    string jsonString = JsonSerializer.Serialize(filesSummary);
                    System.IO.File.WriteAllText(FilePath, jsonString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save summary processed data Err: " + ex.Message);
            }

            return (filesSummary.FilesCount, filesSummary.BytesCount);
        }



        public void OnGet()
        {
             (DisplaySummaryFiles, DisplaySummaryBytes) = SaveSummaryProcessed(_rootEnv.WebRootPath, new FileProcessedSummary(0, 0));
        }

        public async Task<IActionResult> OnPostAjaxEncrypt(string passwordVal, List<IFormFile> uploadedFiles , long summarySize)
        {
            var resultLi = new List<Models.PrepareFiles>();
            if (string.IsNullOrEmpty(passwordVal))
            {
                var fileObj = new Models.PrepareFiles();
                fileObj.ErrorContent = "> No password received";
                resultLi.Add(fileObj);
                return new JsonResult(resultLi);
            }

            if (uploadedFiles == null || uploadedFiles.Count == 0)
            {
                var fileObj = new Models.PrepareFiles();
                fileObj.ErrorContent = "> No files received";
                resultLi.Add(fileObj);
                return new JsonResult(resultLi);
            }


            string webRootPath = _rootEnv.WebRootPath;
            var folderName = Guid.NewGuid(); //unique folder name
            var FilePath = Path.Combine(webRootPath, "uploads/" + folderName);
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

            var summaryProcessed = SaveSummaryProcessed(webRootPath, new FileProcessedSummary(uploadedFiles.Count, summarySize));
          
            foreach (var singleFile in uploadedFiles)
            {
                if (singleFile != null)
                {
                    using (var ms = new MemoryStream())
                    {

                        singleFile.CopyTo(ms);
                        var ByteArr = ms.ToArray();

                        string fileExtension = Path.GetExtension(singleFile.FileName);
                        //Replacing # because of error when downloading file where path contains #
                        var fileName = Path.GetFileNameWithoutExtension(singleFile.FileName).Replace("#", "");
                        var fileObj = new Models.PrepareFiles();

                        if (Models.EncryptCl.checkIfEncrypted(ByteArr))
                        {
                            fileObj.ErrorContent = "> " + fileName + fileExtension + ": File is already encrypted";
                            resultLi.Add(fileObj);
                            continue;
                        }

                        try
                        {
                            byte[] output = await Task.Run(() => Models.EncryptCl.enDeCryptimplementedKey(ByteArr, passwordVal, "encrypt"));

                            var dateT = "-" + DateTime.Now.ToString("dd-MM-yyyy");
                            fileName += dateT + fileExtension;
                            var filePath = Path.Combine(FilePath, fileName.ToString());
                            filePath = Models.PrepareFiles.checkDirectory(filePath);

                            await System.IO.File.WriteAllBytesAsync(filePath, output);

                            fileObj.Name = Path.GetFileName(filePath);
                            fileObj.FileExtension = fileExtension;
                            fileObj.FilePath = "uploads/" + folderName + "/" + Path.GetFileName(filePath);
                            fileObj.FilesCountSummary = summaryProcessed.FilesCount;
                            fileObj.BytesCountSummary = summaryProcessed.BytesCount;
                        }
                        catch(Exception ex)
                        {
                            fileObj.ErrorContent = "> " + fileName + fileExtension + ": " + ex.Message;
                        }
                        finally{
                            resultLi.Add(fileObj);
                        }
                    }
                }
            }

            resultLi[0].ZipArchiweDestination = FilePath;
            return new JsonResult(resultLi);
        }


        //############Encrypt and return data to client in ToBase64String format, via ajax 
        //############By bigger data, to slow in browser 
        /* 
        public async Task<IActionResult> OnPostAjaxEncrypt(string passwordVal, List<IFormFile> uploadedFiles)
        {
            var resultLi = new List<Models.PrepareFiles>();
            if (string.IsNullOrEmpty(passwordVal))
            {
                var fileObj = new Models.PrepareFiles();
                fileObj.ErrorContent = "> No password received";
                resultLi.Add(fileObj);
                return new JsonResult(resultLi);
            }
          
            if (uploadedFiles == null || uploadedFiles.Count==0)
            {
                var fileObj = new Models.PrepareFiles();
                fileObj.ErrorContent = "> No files received";
                resultLi.Add(fileObj);
                return new JsonResult(resultLi);
            }

            var filesAsByteArr =  new Models.PrepareFiles().IformFileToByteArr(uploadedFiles);

            string webRootPath = _rootEnv.WebRootPath;
            var FilePath = Path.Combine(webRootPath, "upload");
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

           
            foreach (var singleFile in filesAsByteArr)
            {
                var fileObj = new Models.PrepareFiles();
                if (Models.EncryptCl.checkIfEncrypted(singleFile.ByteArr))
                {
                    fileObj.ErrorContent = "> " + singleFile.Name + ": File is already encrypted";
                    resultLi.Add(fileObj);
                    continue;
                }

                byte[] output = await Task.Run(() => Models.EncryptCl.enDeCryptimplementedKey(singleFile.ByteArr, passwordVal, "encrypt"));
                //output = Models.EncryptCl.addFileExt(singleFile.Name, output, "kr1p0");  //no need because file format is saved in file name
                //var filePath = Path.Combine(FilePath, singleFile.Name.ToString());
                //await System.IO.File.WriteAllBytesAsync(checkDirectory(filePath), output);
                fileObj.Name = singleFile.Name;
                fileObj.Base64String = Convert.ToBase64String(output);

                resultLi.Add(fileObj);
            }
            return new JsonResult(resultLi);
        }
        */

        public async Task<IActionResult> OnPostAjaxDecrypt(string passwordVal, List<IFormFile> uploadedFiles , long summarySize)
        {
            var resultLi = new List<Models.PrepareFiles>();
            if (string.IsNullOrEmpty(passwordVal))
            {
                var fileObj = new Models.PrepareFiles();
                fileObj.ErrorContent = "> No password received";
                resultLi.Add(fileObj);
                return new JsonResult(resultLi);
            }

            if (uploadedFiles == null || uploadedFiles.Count == 0)
            {
                var fileObj = new Models.PrepareFiles();
                fileObj.ErrorContent = "> No files received";
                resultLi.Add(fileObj);
                return new JsonResult(resultLi);
            }


            string webRootPath = _rootEnv.WebRootPath;
            var folderName = Guid.NewGuid(); //unique folder name
            var FilePath = Path.Combine(webRootPath, "uploads/" + folderName);
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

            var summaryProcessed = SaveSummaryProcessed(webRootPath, new FileProcessedSummary(uploadedFiles.Count, summarySize));

            foreach (var singleFile in uploadedFiles)
            {
                if (singleFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        var fileObj = new Models.PrepareFiles();
                        singleFile.CopyTo(ms);
                        var ByteArr = ms.ToArray();

                        string fileExtension = Path.GetExtension(singleFile.FileName);
                        //Replacing # because of error when downloading file where path contains #
                        var fileName = Path.GetFileNameWithoutExtension(singleFile.FileName).Replace("#", "");

                        if (!Models.EncryptCl.checkIfEncrypted(ByteArr))
                        {
                            fileObj.ErrorContent = "> " + fileName + fileExtension + ": File is not encrypted";
                            resultLi.Add(fileObj);
                            continue;
                        }
                        try
                        {
                            byte[] output = await Task.Run(() => Models.EncryptCl.enDeCryptimplementedKey(ByteArr, passwordVal, "decrypt"));

                            if (output == null)
                            {
                                fileObj.ErrorContent = "> " + fileName + fileExtension + ": Wrong password";
                                //resultLi.Add(fileObj); ADDED TO LIST IN finally
                                continue;
                            }

                            var dateT = "-" + DateTime.Now.ToString("dd-MM-yyyy");
                            fileName += dateT + fileExtension;
                            var filePath = Path.Combine(FilePath, fileName.ToString());
                            filePath = Models.PrepareFiles.checkDirectory(filePath);

                            await System.IO.File.WriteAllBytesAsync(filePath, output);

                            fileObj.Name = Path.GetFileName(filePath);
                            fileObj.FileExtension = fileExtension;
                            fileObj.FilePath = "uploads/" + folderName + "/" + Path.GetFileName(filePath);
                            fileObj.FilesCountSummary = summaryProcessed.FilesCount;
                            fileObj.BytesCountSummary = summaryProcessed.BytesCount;
                        }
                        catch (Exception ex)
                        {
                            fileObj.ErrorContent = "> " + fileName + fileExtension + ": " +ex.Message;
                        }
                        finally
                        {
                            resultLi.Add(fileObj);
                        }
                    }
                }
            }
            resultLi[0].ZipArchiweDestination = FilePath;
            return new JsonResult(resultLi);
        }

        public IActionResult OnPostAjaxEncryptText(string passwordVal, string text)
        {
            string result;

            if (string.IsNullOrEmpty(passwordVal) || string.IsNullOrEmpty(text))
                return new JsonResult("Error: No pass or content #backend site.");

            if (Models.StringEncryptCl.checkIfEncrypted(text))
                return new JsonResult(false);

            try
            {
                 result = Models.StringEncryptCl.stringEnDeCryptAddeddMarker(text, passwordVal, "encrypt");
            }
            catch(Exception ex)
            {
                result = "Error occured: " + ex.Message;
            }
            return new JsonResult(result);
        }

        public IActionResult OnPostAjaxDecryptText(string passwordVal, string text)
        {
            string result;
            if (string.IsNullOrEmpty(passwordVal) || string.IsNullOrEmpty(text))
                return new JsonResult("Error: No pass or content #backend site.");

            if (!Models.StringEncryptCl.checkIfEncrypted(text))
                return new JsonResult(false);

            try
            {
                result = Models.StringEncryptCl.stringEnDeCryptAddeddMarker(text, passwordVal, "decrypt");
            }
            catch (Exception ex)
            {
                result = "Error occured: " + ex.Message;
            }
            return new JsonResult(result);
        }

      
        public IActionResult? OnPostAjaxCreateZip(string FilePath)
        {

            if (string.IsNullOrEmpty(FilePath) )
            {
                var msg = new messageJs(messageType.error.ToString(),
                    "Path not found");
                return new JsonResult(msg);
            }

            if (!Directory.Exists(FilePath))
            {
                var msg = new messageJs(messageType.error.ToString(),
                    "No files found on the server [Files are deleted from the server after 5 minutes].");
                return new JsonResult(msg);
            }

            try
            {
                var result = Models.PrepareFiles.CreateZipFile(FilePath, FilePath);
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                var msg = new messageJs(messageType.error.ToString(), "Error occured: " + ex.Message);
                return new JsonResult(msg);
                
            }
          
        }


    }
}



// //SAVING IN DB , KEEPING THE ORIGINAL IMAGE SIZE
/*
var ImageDataLi = new List<Models.CaseImage>();
foreach (var singleFile in uploadedFile)
{
    if (singleFile != null)
    {
        Models.CaseImage imageObj = new Models.CaseImage();
        imageObj.ImageName = singleFile.FileName;

        MemoryStream ms = new MemoryStream();
        singleFile.CopyTo(ms);
        byte[] logoByteArray = ms.ToArray();
        imageObj.ImageBytes = logoByteArray;
        ImageDataLi.Add(imageObj);
    }
}
var result = Models.Case.InsertCase(CaseObj, ImageDataLi);
*/



//FOR SAVING IMAGE ON SERVER
/*
var fileNameList = new List<string>();
foreach (var singleFile in uploadedFile)
{
    if (singleFile != null)
    {
        var fileName = singleFile.Name;
        string fileExtension = Path.GetExtension(singleFile.FileName);
        fileName = Path.GetFileNameWithoutExtension(singleFile.FileName);
        string webRootPath = _rootEnv.WebRootPath;
        var FilePath = Path.Combine(webRootPath, "uploads/img");
        if (!Directory.Exists(FilePath))
            Directory.CreateDirectory(FilePath);
        //fileName = Guid.NewGuid() + fileExtension; //unique file name
        var dateT = "-" + DateTime.Now.ToString("dd-MM-yyyy");
        fileName += dateT + fileExtension;
        var filePath = Path.Combine(FilePath, fileName.ToString());
        filePath = checkDirectory(filePath);
        using (FileStream fs = System.IO.File.Create(filePath))
        {
            singleFile.CopyTo(fs);
        }
        fileNameList.Add(Path.GetFileName(filePath));
    }
}
var result = Models.Case.InsertCase(CaseObj , fileNameList);
*/
