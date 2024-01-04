// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use strict"

var ZipArchiweDestination = "";
var zip = new JSZip();
var isProcessed = false;
var mode = "files"  // files / text
var ajaxCall;

document.addEventListener("DOMContentLoaded", function () {

    let displayStartInfo = getCookie("displayStartInfo");
    if (displayStartInfo != "n") {
    document.getElementById("startInfoDiv").style.display = "block";
    clickAnimation("startInfoDiv");
    }
   

});



function getCookie(name) {
    let cookieName = name + "=";
    let arr = document.cookie.split(';');
    for (let i = 0; i < arr.length; i++) {
        var c = arr[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(cookieName) == 0)
            return c.substring(cookieName.length, c.length);
    }
    return null;
}




//display Images after input dialog
function onUserDataImageSelected(event) {
    if (document.getElementById("dropZone").classList.contains("noBackground")) {
        document.getElementById("dropZone").classList.remove("noBackground");
        document.getElementById("logoContainer").style.display = "block";

    }
    if (event.target.files[0].size > 52428800) {
        return;
    }
    document.getElementById("dropZone").innerHTML = "";
    document.getElementById("dropZone").classList.remove("dropZoneAlternativeBackground");
    if (isProcessed == true) {
        document.getElementById("displayUploadedImg").innerHTML = "";
        isProcessed = false;
    }

    let divEl = document.createElement("div");
    divEl.style.display = "grid";
    var childDiv = document.getElementById("displayUploadedImg").appendChild(divEl);

    let imgEl = document.createElement("img");
    childDiv.appendChild(imgEl);

    let spanEl = document.createElement("span");
    childDiv.appendChild(spanEl);


    var selectedFile = event.target.files[0];
    var reader = new FileReader();

    let fileExtension = selectedFile.name.substring(selectedFile.name.lastIndexOf('.') + 1, selectedFile.name.length)
    let fileName = selectedFile.name;

    reader.onload = function (event) {
        //imgEl.src = event.target.result;
        if (fileExtension == "jpg" || fileExtension == "jpeg" || fileExtension == "png" || fileExtension == "gif" || fileExtension == "svc")
            imgEl.src = "../img/image-file.svg"
        else if (fileExtension == "doc" || fileExtension == "docx" || fileExtension == "txt" || fileExtension == "rtf" || fileExtension == "mobi")
            imgEl.src = "../img/text-file.svg";
        else if (fileExtension == "pdf")
            imgEl.src = "../img/pdf-file.svg";
        else if (fileExtension == "mp3" || fileExtension == "wav" || fileExtension == "aac" || fileExtension == "wma" || fileExtension == "MP4" || fileExtension == "flac" || fileExtension == "m4a")
            imgEl.src = "../img/music-file.svg";
        else if (fileExtension == "zip" || fileExtension == "rar" || fileExtension == "7z" || fileExtension == "gzip" || fileExtension == "tar")
            imgEl.src = "../img/zip-file.svg";
        else
            imgEl.src = "../img/empty-file.svg";

        spanEl.innerText = fileName.substring(0, 30);
    };

    reader.readAsDataURL(selectedFile);
}




//prefent from form submit byt validation works
/*
function onloadIndex() {
    $("#addFilesForm").on("submit", function (e) {
        e.preventDefault()
        encrypt();
    });
}
*/

//add images to array for saving it later
var filesArr = [];
function addImage2formData() {
    let files = $('#fileUpload').prop("files");

    if (files[0].size > 52428800) {
        alert("File cannot be larger than 50MB")
        return;
    }

    filesArr.push(files[0]);

}



//save case form and images
async function encrypt() {
    let summarySize = 0;

    if (mode == "text") {
        encryptText();
        return;
    }



    if (filesArr.length == 0) {
        alert("No files added");
        return;
    }

    if (nextStep == false)
        document.getElementById("passwordWindow").style.display = "block";
    await waitUserInput();

    if (passwordVal == "") return;

    let formData = new FormData();

    formData.append("passwordVal", passwordVal);


    passwordVal = ""; //reset pass
    document.getElementById("passWindowIcon").classList.add("tableButtonAddPassword")
    document.getElementById("passWindowIcon").classList.remove("tableButtonAddPasswordGreen");


    for (let i = 0; i < filesArr.length; i++) {
        formData.append("uploadedFiles", filesArr[i]);
        summarySize += filesArr[i].size;
    }

    formData.append("summarySize", summarySize);

    if (summarySize > 52428800) {
        alert("Files cannot exceed 50MB.")
        return;
    }

    filesArr = []; //empty
    document.getElementById("displayUploadedImg").style.opacity = 0.2;
    document.getElementById("fileUpload").value = "";

    document.getElementById("customSpinner").style.display = "block";
    document.getElementById("processingMsg").innerText = "Uploading files..."

    ajaxCall = $.ajax({
        //######## Upload/Download status
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            //Upload progress
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    //Do something with upload progress
                    console.log("Upload progress: " + percentComplete * 100 + "%");

                    document.getElementById("progressBarContainer").style.display = "block";
                    document.getElementById("progressInfo").innerText = "";
                    document.getElementById("progressProcentage").innerText ="Uploading: " + parseInt(percentComplete * 100) + "%"
                    document.getElementById("progressBar").style.width = percentComplete*100+"%"

                }
            }, false);
            //Download progress
            xhr.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    //Do something with download progress
                    console.log("Download progress: "+percentComplete);
                }
            }, false);
            return xhr;
        },
        //######## Upload/Download status

        type: "POST",
        url: "/index?handler=AjaxEncrypt",
        //cache: false,
        contentType: false,
        processData: false,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
      
        data: formData,
        success: function (data) {
            document.getElementById("processingMsg").innerText = "Preparing files..."

            document.getElementById("progressInfo").innerText = "Processing...";
            document.getElementById("progressProcentage").innerText = "";
            document.getElementById("progressBar").style.width = "0";

            document.getElementById("displayUploadedImg").innerHTML = "";
            document.getElementById("displayUploadedImg").style.opacity = 1;

            if (data.length == 0)
                return;

            isProcessed = true;

            //zip = new JSZip();
            ZipArchiweDestination = data[0].zipArchiweDestination;

            document.getElementById("displayFilesesProcessed").innerHTML = "Files processed: " + data[0].filesCountSummary;
            document.getElementById("displayBytesProcessed").innerHTML = "Bytes processed: " + data[0].bytesCountSummary;

            setTimeout(function () {

                document.getElementById("dropZone").innerText = "";
                document.getElementById("progressBarContainer").style.display = "none";


                data.forEach(function (element) {

                    if (element.errorContent != null) {
                        if (!document.getElementById("dropZone").classList.contains("noBackground")) {
                            document.getElementById("dropZone").classList.add("noBackground");
                            document.getElementById("logoContainer").style.display = "none";

                        }
                        document.getElementById("dropZone").innerHTML += `<p class="errorElName">${element.errorContent}</p>`;
                        return; //quivalent to continue
                    }


                    let tempDiv = document.createElement("div");
                    tempDiv.style.display = "grid";
                    let imgSrc = "";
                    let fileExtension = element.fileExtension.replace(".", "");;

                    if (fileExtension == "jpg" || fileExtension == "jpeg" || fileExtension == "png" || fileExtension == "gif" || fileExtension == "svc")
                        imgSrc = "../img/image-file-lock.svg"
                    else if (fileExtension == "doc" || fileExtension == "docx" || fileExtension == "txt" || fileExtension == "rtf" || fileExtension == "mobi")
                        imgSrc = "../img/text-file-lock.svg";
                    else if (fileExtension == "pdf")
                        imgSrc = "../img/pdf-file-lock.svg";
                    else if (fileExtension == "mp3" || fileExtension == "wav" || fileExtension == "aac" || fileExtension == "wma" || fileExtension == "MP4" || fileExtension == "flac" || fileExtension == "m4a")
                        imgSrc = "../img/music-file-lock.svg";
                    else if (fileExtension == "zip" || fileExtension == "rar" || fileExtension == "7z" || fileExtension == "gzip" || fileExtension == "tar")
                        imgSrc = "../img/zip-file-lock.svg";
                    else
                        imgSrc = "../img/empty-file-lock.svg";

                    let shortName = element.name.substring(0, 30);

                    tempDiv.innerHTML = `<a  download="" href="${element.filePath}"> <img class="encryptedFile" src = "${imgSrc}"> </a><span>${shortName}</span>`


                    //zip.file(element.name, element.base64String, { base64: true });


                    document.getElementById("displayUploadedImg").appendChild(tempDiv);


                });


            }, 850);



            //   zip.generateAsync({ type: "base64" }).then(function (content) {
            //      location.href = "data:application/zip;base64," + content;
            //   });


        },
        failure: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
            document.getElementById("processingMsg").innerText = "Failure"

        },
        error: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
            document.getElementById("processingMsg").innerText = "Failure"

        },
        complete: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
            document.getElementById("processingMsg").innerText = ""

        }
    });

}

/*
//encrypt and return encrypted files in string64 form. The files are not saved on server. Works slow, problems with AWS
async function encrypt() {
    if (mode == "text") {
        encryptText();
        return;
    }



    if (filesArr.length == 0) {
        alert("No files added");
        return;
    }

    if (nextStep == false)
        document.getElementById("passwordWindow").style.display = "block";
    await waitUserInput();

    if (passwordVal == "") return;

    let formData = new FormData();

    formData.append("passwordVal", passwordVal);


    passwordVal = ""; //reset pass
    document.getElementById("passWindowIcon").classList.add("tableButtonAddPassword")
    document.getElementById("passWindowIcon").classList.remove("tableButtonAddPasswordGreen");


    for (let i = 0; i < filesArr.length; i++) {
        formData.append("uploadedFiles", filesArr[i]);
    }
    //alert(Array.from(formData.keys()).length);
    filesArr = []; //empty
    document.getElementById("displayUploadedImg").style.opacity = 0.2;
    document.getElementById("fileUpload").value = "";

    document.getElementById("customSpinner").style.display = "block";


    $.ajax({
        type: "POST",
        url: "/index?handler=AjaxEncrypt",
        //cache: false,
        contentType: false,
        processData: false,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: formData,
        success: function (data) {

            document.getElementById("displayUploadedImg").innerHTML = "";
            document.getElementById("displayUploadedImg").style.opacity = 1;

            if (data.length == 0)
                return;

            isProcessed = true;

            zip = new JSZip();


            setTimeout(function () {

                document.getElementById("dropZone").innerText = "";
                data.forEach(function (element) {


                    if (element.errorContent != null)
                        document.getElementById("dropZone").innerHTML += `<p class="errorElName">${element.errorContent}</p>`;

                    if (element.base64String == null)
                        return;

                    let tempDiv = document.createElement("div");
                    tempDiv.style.display = "grid";

                    let imgSrc = "";
                    let fileExtension = element.name.substring(element.name.lastIndexOf('.') + 1, element.name.length)
                    if (fileExtension == "jpg" || fileExtension == "jpeg" || fileExtension == "png" || fileExtension == "gif" || fileExtension == "svc")
                        imgSrc = "../img/image-file-lock.svg"
                    else if (fileExtension == "doc" || fileExtension == "docx" || fileExtension == "txt" || fileExtension == "rtf" || fileExtension == "mobi")
                        imgSrc = "../img/text-file-lock.svg";
                    else if (fileExtension == "pdf")
                        imgSrc = "../img/pdf-file-lock.svg";
                    else if (fileExtension == "mp3" || fileExtension == "wav" || fileExtension == "aac" || fileExtension == "wma" || fileExtension == "MP4" || fileExtension == "flac" || fileExtension == "m4a")
                        imgSrc = "../img/music-file-lock.svg";
                    else if (fileExtension == "zip" || fileExtension == "rar" || fileExtension == "7z" || fileExtension == "gzip" || fileExtension == "tar")
                        imgSrc = "../img/zip-file-lock.svg";
                    else
                        imgSrc = "../img/empty-file-lock.svg";

                    let shortName = element.name.substring(0, 30);
                    tempDiv.innerHTML = `<a  download="${element.name}" href="data:application;base64,${element.base64String}"> <img class="encryptedFile" src = "${imgSrc}"> </a><span>${shortName}</span>`

                    zip.file(element.name, element.base64String, { base64: true });


                    document.getElementById("displayUploadedImg").appendChild(tempDiv);


                });


            }, 850);



            //   zip.generateAsync({ type: "base64" }).then(function (content) {
            //      location.href = "data:application/zip;base64," + content;
            //   });


        },
        failure: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
        },
        error: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
        },
        complete: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
        }
    });

}
*/


async function decrypt() {
    let summarySize = 0;

    if (mode == "text") {
        decryptText();
        return;
    }

    if (filesArr.length == 0) {
        alert("No files added");
        return;
    }

    if (nextStep == false)
        document.getElementById("passwordWindow").style.display = "block";
    await waitUserInput();

    if (passwordVal == "") return;

    let formData = new FormData();

    formData.append("passwordVal", passwordVal);

    passwordVal = ""; //reset pass
    document.getElementById("passWindowIcon").classList.add("tableButtonAddPassword")
    document.getElementById("passWindowIcon").classList.remove("tableButtonAddPasswordGreen");

    for (let i = 0; i < filesArr.length; i++) {
        formData.append("uploadedFiles", filesArr[i]);
        summarySize += filesArr[i].size;
    }

    formData.append("summarySize", summarySize);

    if (summarySize > 52428800) {
        alert("Files cannot exceed 50MB.")
        return;
    }

    filesArr = []; //empty
    document.getElementById("displayUploadedImg").style.opacity = 0.2;
    document.getElementById("fileUpload").value = "";

    document.getElementById("customSpinner").style.display = "block";
    document.getElementById("processingMsg").innerText = "Uploading files..."


    ajaxCall = $.ajax({
        //######## Upload/Download status
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            //Upload progress
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    //Do something with upload progress
                    console.log("Upload progress: " + percentComplete * 100 + "%");

                    document.getElementById("progressBarContainer").style.display = "block";
                    document.getElementById("progressInfo").innerText = "";
                    document.getElementById("progressProcentage").innerText = "Uploading: " + parseInt(percentComplete * 100) + "%"
                    document.getElementById("progressBar").style.width = percentComplete * 100 + "%"

                }
            }, false);
            //Download progress
            xhr.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    //Do something with download progress
                    console.log("Download progress: " + percentComplete);
                }
            }, false);
            return xhr;
        },
        //######## Upload/Download status
        type: "POST",
        url: "/index?handler=AjaxDecrypt",
        //cache: false,
        contentType: false,
        processData: false,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: formData,
        success: function (data) {
            document.getElementById("processingMsg").innerText = "Preparing files..."

            document.getElementById("progressInfo").innerText = "Processing...";
            document.getElementById("progressProcentage").innerText = "";
            document.getElementById("progressBar").style.width = "0";

            document.getElementById("displayUploadedImg").innerHTML = "";
            document.getElementById("displayUploadedImg").style.opacity = 1;

            if (data.length == 0)
                return;

            isProcessed = true;

            ZipArchiweDestination = data[0].zipArchiweDestination;

            document.getElementById("displayFilesesProcessed").innerHTML ="Files processed: "+ data[0].filesCountSummary;
            document.getElementById("displayBytesProcessed").innerHTML ="Bytes processed: "+ data[0].bytesCountSummary;

            setTimeout(function () {

                document.getElementById("dropZone").innerText = "";
                document.getElementById("progressBarContainer").style.display = "none";

                data.forEach(function (element) {
                    if (element.errorContent != null) {
                        if (!document.getElementById("dropZone").classList.contains("noBackground")) {
                            document.getElementById("dropZone").classList.add("noBackground");
                            document.getElementById("logoContainer").style.display = "none";
                        }
                        document.getElementById("dropZone").innerHTML += `<p class="errorElName">${element.errorContent}</p>`;
                        return; //quivalent to continue
                    }

                    let tempDiv = document.createElement("div");
                    tempDiv.style.display = "grid";

                    let imgSrc = "";
                    let fileExtension = element.fileExtension.replace(".", "");

                    if (fileExtension == "jpg" || fileExtension == "jpeg" || fileExtension == "png" || fileExtension == "gif" || fileExtension == "svc")
                        imgSrc = "../img/image-file-unlock.svg"
                    else if (fileExtension == "doc" || fileExtension == "docx" || fileExtension == "txt" || fileExtension == "rtf" || fileExtension == "mobi")
                        imgSrc = "../img/text-file-unlock.svg";
                    else if (fileExtension == "pdf")
                        imgSrc = "../img/pdf-file-unlock.svg";
                    else if (fileExtension == "mp3" || fileExtension == "wav" || fileExtension == "aac" || fileExtension == "wma" || fileExtension == "MP4" || fileExtension == "flac" || fileExtension == "m4a")
                        imgSrc = "../img/music-file-unlock.svg";
                    else if (fileExtension == "zip" || fileExtension == "rar" || fileExtension == "7z" || fileExtension == "gzip" || fileExtension == "tar")
                        imgSrc = "../img/zip-file-unlock.svg";
                    else
                        imgSrc = "../img/empty-file-unlock.svg";

                    let shortName = element.name.substring(0, 30);
                    tempDiv.innerHTML = `<a  download="" href="${element.filePath}"> <img class="encryptedFile" src = "${imgSrc}"> </a><span>${shortName}</span>`

                    //zip.file(element.name, element.base64String, { base64: true });

                    document.getElementById("displayUploadedImg").appendChild(tempDiv);

                });


            }, 850);



        },
        failure: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
            document.getElementById("processingMsg").innerText = "Failure"

        },
        error: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
            document.getElementById("processingMsg").innerText = "Failure"

        },
        complete: function (response) {
            document.getElementById("displayUploadedImg").style.opacity = 1;
            document.getElementById("processingMsg").innerText = ""
        }
    });

}

function interruptAjaxCall() {
    ajaxCall.abort();
    setTimeout(function () {
        document.getElementById("progressBarContainer").style.display = "none";
        document.getElementById("customSpinner").style.display = "none";
        document.getElementById("processingMsg").innerText = ""
        removeAllFiles();
    },800)
}



function openBase64InNewTab(data) {

    var newTab = window.open();
    newTab.document.body.innerHTML = `<img src="data:image/jpg;base64,${data}" width="fit-content">`;
}


function dragOverHandler(ev) {
    if (document.getElementById("dropZone").classList.contains("noBackground")) {
        document.getElementById("dropZone").classList.remove("noBackground");
        document.getElementById("logoContainer").style.display = "block";
    }
    console.log('File(s) in drop zone');

    document.getElementById("dropZone").innerHTML = "";
    document.getElementById("dropZone").classList.add("dropZoneAlternativeBackground");

    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();
}

function dragLeaveHandler(ev) {
    document.getElementById("dropZone").classList.remove("dropZoneAlternativeBackground");
}


function dropHandler(ev) {
    let toLargeFileALert = false;
    document.getElementById("dropZone").classList.remove("dropZoneAlternativeBackground");
    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();



    if (ev.dataTransfer.items) {

        if (isProcessed == true) {
            document.getElementById("displayUploadedImg").innerHTML = "";
            isProcessed = false;
        }

        // Use DataTransferItemList interface to access the file(s)
        for (let i = 0; i < ev.dataTransfer.items.length; i++) {
            // If dropped items aren't files, reject them
            if (ev.dataTransfer.items[i].kind === 'file') {
                const file = ev.dataTransfer.items[i].getAsFile();


                let entry = ev.dataTransfer.items[i].webkitGetAsEntry();
                if (entry.isDirectory) { //folders
                    continue;
                }
                if (file.size > 52428800) { //folders
                    if (toLargeFileALert == false) {
                        alert("File cannot be larger than 50MB")
                        toLargeFileALert = true;
                    }
                    continue;
                }


                filesArr.push(file);

                let divEl = document.createElement("div");
                divEl.style.display = "grid";
                var childDiv = document.getElementById("displayUploadedImg").appendChild(divEl);

                let imgEl = document.createElement("img");
                childDiv.appendChild(imgEl);

                let spanEl = document.createElement("span");
                childDiv.appendChild(spanEl);


                let fileExtension = file.name.substring(file.name.lastIndexOf('.') + 1, file.name.length)
                if (fileExtension == "jpg" || fileExtension == "jpeg" || fileExtension == "png" || fileExtension == "gif" || fileExtension == "svc")
                    imgEl.src = "../img/image-file.svg"
                else if (fileExtension == "doc" || fileExtension == "docx" || fileExtension == "txt" || fileExtension == "rtf" || fileExtension == "mobi")
                    imgEl.src = "../img/text-file.svg";
                else if (fileExtension == "pdf")
                    imgEl.src = "../img/pdf-file.svg";
                else if (fileExtension == "mp3" || fileExtension == "wav" || fileExtension == "aac" || fileExtension == "wma" || fileExtension == "MP4" || fileExtension == "flac" || fileExtension == "m4a")
                    imgEl.src = "../img/music-file.svg";
                else if (fileExtension == "zip" || fileExtension == "rar" || fileExtension == "7z" || fileExtension == "gzip" || fileExtension == "tar")
                    imgEl.src = "../img/zip-file.svg";
                else
                    imgEl.src = "../img/empty-file.svg";

                spanEl.innerText = file.name.substring(0, 30);
            }
        }
    } else {

        document.getElementById("displayUploadedImg").innerHTML = "";

        // Use DataTransfer interface to access the file(s)
        for (let i = 0; i < ev.dataTransfer.files.length; i++) {


            let entry = ev.dataTransfer.items[i].webkitGetAsEntry();
            if (entry.isDirectory) { //folders
                continue;
            }


            filesArr.push(ev.dataTransfer.files[i]);
            let imgEl = document.createElement("img");
            document.getElementById("displayUploadedImg").appendChild(imgEl);

            let fileExtension = ev.dataTransfer.files[i].name.substring(ev.dataTransfer.files[i].name.lastIndexOf('.') + 1, ev.dataTransfer.files[i].name.length)
            if (fileExtension == "jpg" || fileExtension == "jpeg" || fileExtension == "png" || fileExtension == "gif" || fileExtension == "svc")
                imgEl.src = "../img/image-file.svg"
            else if (fileExtension == "doc" || fileExtension == "docx" || fileExtension == "txt" || fileExtension == "rtf" || fileExtension == "mobi")
                imgEl.src = "../img/text-file.svg";
            else if (fileExtension == "pdf")
                imgEl.src = "../img/pdf-file.svg";
            else if (fileExtension == "mp3" || fileExtension == "wav" || fileExtension == "aac" || fileExtension == "wma" || fileExtension == "MP4" || fileExtension == "flac" || fileExtension == "m4a")
                imgEl.src = "../img/music-file.svg";
            else if (fileExtension == "zip" || fileExtension == "rar" || fileExtension == "7z" || fileExtension == "gzip" || fileExtension == "tar")
                imgEl.src = "../img/zip-file.svg";
            else
                imgEl.src = "../img/empty-file.svg";

            spanEl.innerText = ev.dataTransfer.files[i].name.substring(0, 37);;
        }
    }
}




$(".displayUploadedImgCaseTableContainer").bind("DOMNodeInserted", function () {
    //var width = getComputedStyle(document.body).getPropertyValue('--mainContainerWidth')
    //or DOMNodeRemoved
    const widthVariable = getComputedStyle(document.documentElement)
        .getPropertyValue('--mainContainerWidth'); // #999999
    let border = '3vw';
    if (window.innerWidth <= 768) {
        border = '0px';
    }
    this.style.width = `calc(${widthVariable} - ${border})`;
    //document.getElementById("drop_zone").style.height= "calc((100vh / 1.5) - 60px)";

});

$(".displayUploadedImgCaseTableContainer").bind("DOMNodeRemoved", function () {
    //or DOMNodeInserted
    this.style.width = "0px";
    //document.getElementById("drop_zone").style.height = "calc(100vh / 1.5)";
});


/*
$("#InfoZone").bind("DOMNodeInserted", function () {
    document.getElementById("InfoZone").style.transform = "rotateY(0deg)";
    document.getElementById("dropZone").style.transform = "rotateY(180deg)";
    setTimeout(function () {
        document.getElementById("closeInfoZoneBtn").style.display = "block";
    }, 1400);


});

*/


//hide spinner when processed
$("#dropZone").bind("DOMNodeInserted", function () {
    document.getElementById("customSpinner").style.display = "none";
});
$("#displayUploadedImg").bind("DOMNodeInserted", function () {
    document.getElementById("customSpinner").style.display = "none";
});



/*
function closeInfoZone() {
    document.getElementById("dropZone").style.transform = "";
    document.getElementById("InfoZone").style.transform = "";
    document.getElementById("closeInfoZoneBtn").style.display = "none";
    setTimeout(function () {
        document.getElementById("InfoZone").innerText = "";
    }, 1400);

}
*/

function removeAllFiles() {
    if (document.getElementById("dropZone").classList.contains("noBackground")) {
        document.getElementById("dropZone").classList.remove("noBackground");
        document.getElementById("logoContainer").style.display = "block";
    }
    document.getElementById("displayUploadedImg").innerHTML = "";
    isProcessed = false;
    filesArr = [];
    zip = new JSZip();
    ZipArchiweDestination = "";
    document.getElementById("fileUpload").value = "";
    document.getElementById("InfoZone").value = "";

    document.getElementById("dropZone").innerHTML = "";
    document.getElementById("dropZone").classList.remove("dropZoneAlternativeBackground");
}

function displayPasswindow() {
    let pasWindow = document.getElementById("passwordWindow");
    if ($('#passwordWindow').css('display') == 'none')
        pasWindow.style.display = "block";
    else
        pasWindow.style.display = "none";
}

function passInputKeyUp() {
    if (event.key === 'Enter' && document.getElementById("passwordInput").value != "") {
        passwordVal = document.getElementById("passwordInput").value;
        nextStep = true;
        document.getElementById("passwordInput").value = "";
        document.getElementById("passwordWindow").style.display = "none";
        document.getElementById("passWindowIcon").classList.remove("tableButtonAddPassword");
        document.getElementById("passWindowIcon").classList.add("tableButtonAddPasswordGreen")
    }
}
function passInputOkClick() {
    if (document.getElementById("passwordInput").value != "") {
        passwordVal = document.getElementById("passwordInput").value;
        nextStep = true;
        document.getElementById("passwordInput").value = "";
        document.getElementById("passwordWindow").style.display = "none";
        document.getElementById("passWindowIcon").classList.remove("tableButtonAddPassword");
        document.getElementById("passWindowIcon").classList.add("tableButtonAddPasswordGreen")
    }

}

function closePasswordWindowClick() {
    document.getElementById("passwordWindow").style.display = "none";
    nextStep = true;
}



//awaiting for password
const timeout = async ms => new Promise(res => setTimeout(res, ms));
let nextStep = false; // this is to be changed on user input
let passwordVal = "";

async function waitUserInput() {
    while (nextStep === false) await timeout(200); // pauses script
    nextStep = false; // reset var
}



function saveAsZip() {

    //FileSaver librabry 
    if (!isProcessed) {
        alert("No processed files");
        return;
    }

    if (ZipArchiweDestination == "") {
        alert("No zip archiwe destination");
        return;
    }

    $.ajax({
        type: "POST",
        url: "/index?handler=AjaxCreateZip",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: { FilePath: ZipArchiweDestination },

        success: function (data) {
            if (data.msgType == "error"){
                alert(data.content);
                removeAllFiles();
                return;
            }
            let tempDiv = document.createElement("div");
            document.getElementById("displayUploadedImg").appendChild(tempDiv);
            var element = document.createElement('a');
            element.setAttribute('href', data);
            element.setAttribute('download', "ZipArchive.zip");
            element.style.display = 'none';
            document.body.appendChild(element);
            element.click();

            document.body.removeChild(element);
        },
        failure: function (response) {
        },
        error: function (response) {
        },
        complete: function (response) {
        }
    });





    /* for download string64 files
    zip.generateAsync({ type: "blob" }).then(function (content) {
        // see FileSaver.js
        saveAs(content, "endeCryptOnline.zip");
    });
    */

}

function collpasedNavbarClick(sender) {
    sender.classList.remove("show")
}

function selectTextArea(sender) {
    removeAllFiles();
    document.getElementById("InfoZone").style.transform = "rotateY(0deg)";
    document.getElementById("dropZone").style.transform = "rotateY(180deg)";
    document.getElementById("InfoZone").focus();
    document.getElementById("logoContainer").style.display = "none";

    mode = "text";

    document.getElementById("filesTextSelectorBackground").style.left = "calc(100% - 45px)";
    document.getElementById("displaySelectedTextFiles").innerHTML = "Text" + "&nbsp;";
}

function selectFilesArea(sender) {
    document.getElementById("dropZone").style.transform = "";
    document.getElementById("InfoZone").style.transform = "";


    setTimeout(function () {
        document.getElementById("InfoZone").value = "";
        document.getElementById("logoContainer").style.display = "block";
    }, 1400);


    mode = "files";

    document.getElementById("filesTextSelectorBackground").style.left = "0";
    document.getElementById("displaySelectedTextFiles").innerHTML = "Files";

}




async function encryptText() {

    let text = document.getElementById("InfoZone").value;

    if (text == "") {
        alert("No content.");
        return;
    }

    if (nextStep == false)
        document.getElementById("passwordWindow").style.display = "block";
    await waitUserInput();

    if (passwordVal == "") return;

    $.ajax({
        type: "POST",
        url: "/index?handler=AjaxEncryptText",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: { passwordVal, text },

        success: function (data) {

            if (data == false)
                alert("Text is already encrypted.");
            else
                document.getElementById("InfoZone").value = data;

        },
        failure: function (response) {

        },
        error: function (response) {

        },
        complete: function (response) {

        }
    });

    passwordVal = ""; //reset pass
    document.getElementById("passWindowIcon").classList.add("tableButtonAddPassword")
    document.getElementById("passWindowIcon").classList.remove("tableButtonAddPasswordGreen");
}

async function decryptText() {

    let text = document.getElementById("InfoZone").value;

    if (text == "") {
        alert("No content.");
        return;
    }

    if (nextStep == false)
        document.getElementById("passwordWindow").style.display = "block";
    await waitUserInput();

    if (passwordVal == "") return;

    $.ajax({
        type: "POST",
        url: "/index?handler=AjaxDecryptText",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: { passwordVal, text },
        success: function (data) {
            if (data == false)
                alert("Text is not encrypted.");
            else
                document.getElementById("InfoZone").value = data;



        },
        failure: function (response) {

        },
        error: function (response) {

        },
        complete: function (response) {

        }
    });

    passwordVal = ""; //reset pass
    document.getElementById("passWindowIcon").classList.add("tableButtonAddPassword")
    document.getElementById("passWindowIcon").classList.remove("tableButtonAddPasswordGreen");
}


function closeDiv(sender) {
    sender.parentElement.style.display = "none";
    document.cookie = `displayStartInfo= n ; max-age=31536000 ;path=/`; //60*60*24*365 = 1year //;path=/ for all subpages
}



function clickAnimation(id) {
    $("#" + id).animate(
        { deg: 5 },
        {
            duration: 200,
            step: function (now) {
                $(this).css({ transform: 'rotate(' + now + 'deg)' });
            }
        }
    );
    $("#" + id).animate(
        { deg: -4 },
        {
            duration: 200,
            step: function (now) {
                $(this).css({ transform: 'rotate(' + now + 'deg)' });
            }
        }
    );
    $("#" + id).animate(
        { deg: 3 },
        {
            duration: 200,
            step: function (now) {
                $(this).css({ transform: 'rotate(' + now + 'deg)' });
            }
        }
    );
    $("#" + id).animate(
        { deg: 0 },
        {
            duration: 200,
            step: function (now) {
                $(this).css({ transform: 'rotate(' + now + 'deg)' });
            }
        }
    );
}



observeDomChanges("displayFilesesProcessed");
observeDomChanges("displayBytesProcessed");
function observeDomChanges(domID) {
    // identify an element to observe
    var elementToObserve = window.document.getElementById(domID);

    // create a new instance of 'MutationObserver' named 'observer', 
    // passing it a callback function
    var observer = new MutationObserver(function (mutationsList, observer) {
        console.log(mutationsList);

        document.getElementById(domID).style.opacity = 0;

        setTimeout(function () {
            for (let opacity = 0; opacity < 1.05; opacity = opacity + 0.05) {
                setTimeout(function () {
                    document.getElementById(domID).style.opacity = opacity;
                }, 2000 * opacity)
            }
        },1500)
       
    });

    // call 'observe' on that MutationObserver instance, 
    // passing it the element to observe, and the options object
    observer.observe(elementToObserve, { characterData: false, childList: true, attributes: false });
}
