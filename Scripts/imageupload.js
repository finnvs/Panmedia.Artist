function Init_Upload() {
    // alert("Init Upload called");
    $('#FormUpload input[name=UploadedFile]').change(function (evt) { singleFileSelected(evt); });
    $("#FormUpload button[id=Cancel_btn]").click(function () {
        Cancel_btn_handler();
    });
    $('#FormUpload button[id=Submit_btn]').click(function () {
        UploadFile();
    });


    //$.blockUI.defaults.overlayCSS = {
    //    backgroundColor: '#000',
    //    opacity: 0.6
    //};
    //$.blockUI.defaults.css = {
    //    padding: 0,
    //    margin: 5,
    //    width: '50%',
    //    top: '30%',
    //    left: '25%',
    //    color: '#000',
    //    border: '3px solid #aaa',
    //    backgroundColor: '#fff'
    //};
    //$.blockUI({ message: $('#createView') });
}

function singleFileSelected(evt) {
    //var selectedFile = evt.target.files can use this or select input file element and access it's files object
    var selectedFile = ($("#UploadedFile"))[0].files[0]; //FileControl.files[0];
    if (selectedFile) {
        var FileSize = 0;
        var imageType = /image.*/;
        if (selectedFile.size > 1048576) {
            FileSize = Math.round(selectedFile.size * 100 / 1048576) / 100 + " MB";
        }
        else if (selectedFile.size > 1024) {
            FileSize = Math.round(selectedFile.size * 100 / 1024) / 100 + " KB";
        }
        else {
            FileSize = selectedFile.size + " Bytes";
        }

        if (selectedFile.type.match(imageType)) {
            var reader = new FileReader();
            reader.onload = function (e) {

                $("#Imagecontainer").empty();
                var dataURL = reader.result;
                var img = new Image();
                img.src = dataURL;
                img.className = "thumb";
                $("#Imagecontainer").append(img);

                // add cropping ability to Image container and fade in cropimage area / btn
                initCrop();
                // alert("Init Crop called");
                $("#crop-image-area").fadeIn("slow");
                alert("Image File Path:" + selectedFile);
                $("#hf-uploaded-image-path").val(selectedFile); // needed ref for AJAX post to controller

            };
            reader.readAsDataURL(selectedFile);
        }
        $("#FileName").text("Name : " + selectedFile.name);
        $("#FileType").text("type : " + selectedFile.type);
        $("#FileSize").text("Size : " + FileSize);
    }
}

function UploadFile() {
    //we can create form by passing the form to Constructor of formData object
    //or creating it manually using append function 
    //but please note file name should be same like the action Parameter
    //var dataString = new FormData();
    //dataString.append("UploadedFile", selectedFile);

    var form = $('#FormUpload')[0];
    var selectedFile = ($("#UploadedFile"))[0].files[0];
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    // var dataString = new FormData(form);
    var dataString = new FormData();
    dataString.append("UploadedFile", selectedFile);
    // send token-dude as wella
    dataString.append("__RequestVerificationToken", token);

    var progressCustom = new LoadingOverlayProgress({
        bar: {
            "background": '#000', // red,
            "top": "50px",
            "height": "30px",
            "border-radius": "15px"
        },
        text: {
            "color": '#000', // red,
            "font-family": "monospace",
            "top": "25px"
        }
    });

    $.ajax({
        url: 'http://localhost:30321/OrchardLocal/Profile/ImageEdit',  //Server script to process data
        type: 'POST',
        beforeSend: function () {
            $.LoadingOverlay("show", {
                custom: progressCustom.Init()
            });
        },
        complete: function () {
            $.LoadingOverlay("hide");
        },
        xhr: function () {  // Custom XMLHttpRequest
            var myXhr = $.ajaxSettings.xhr();
            // 01.03.2017 test taget ud for at undgå standard prog bar ved upload til browser af thumb
            //if (myXhr.upload) { // Check if upload property exists                
            //    myXhr.upload.addEventListener('progress', progressHandlingFunction, false); // For handling the progress of the upload
            //}
            return myXhr;
        },
        //Ajax events
        success: successHandler,
        error: errorHandler,
        // complete: completeHandler, // would double up re loader
        // Form data
        data: dataString,
        //Options to tell jQuery not to process data or worry about content-type.
        cache: false,
        contentType: false,
        processData: false
    });
}


// 01.03.2017 test taget ud for at undgå standard prog bar ved upload til browser af thumb
//function progressHandlingFunction(e) {
//    if (e.lengthComputable) {
//        var percentComplete = Math.round(e.loaded * 100 / e.total);
//        $("#FileProgress").css("width", percentComplete + '%').attr('aria-valuenow', percentComplete);
//        $('#FileProgress span').text(percentComplete + "%");
//    }
//    else {
//        $('#FileProgress span').text('unable to compute');
//    }
//}


function completeHandler() {
    $('#createView').empty();
    $('.CreateLink').show();
    // $.unblockUI();
}


//function successHandler(data) {
function successHandler(response) {
    // data statuscode er undefined ved return fra controller (der returneres ikke JSON, jvf. 
    // http://stackoverflow.com/questions/199099/how-to-manage-a-redirect-request-after-a-jquery-ajax-call)

    //if (data.statusCode === 200) {
    //    $('#FilesList tr:last').after(data.NewRow);
    //    alert(data.status);
    //}
    //else {
    //    alert(data.status);
    //}
    // $.unblockUI();
    // nødvendigt med en redirect hvis success handleren skal kaldes da XHR requestet har naflet pipeline?
    // window.location = 'http://localhost:30321/OrchardLocal/ProfileRedirect';

    // der er åbenbart ikke en redirect? men en hardcoded url vil den tage
    if (response.redirect) {
        alert("ImageUpload.js - redirect to: " + response.redirect)
        window.location.href = response.redirect;
    }
    else {
        // Huu - ballade. Process the expected results...
        window.location = 'http://localhost:30321/OrchardLocal/ProfileRedirect';
    }
}

function errorHandler(xhr, ajaxOptions, thrownError) {
    alert("There was an error attempting to upload the file. (" + thrownError + ")");
}

function OnDeleteAttachmentSuccess(data) {

    if (data.ID && data.ID !== "") {
        $('#Attachment_' + data.ID).fadeOut('slow');
    }
    else {
        alter("Unable to Delete");
        console.log(data.message);
    }
}

function Cancel_btn_handler() {
    $('#createView').empty();
    $('.CreateLink').show();
    // $.unblockUI();
    window.location = 'http://localhost:30321/OrchardLocal';
}


/* Added functionality to enable JCrop */

//************************************** JavaScript for cropping of image *****************************************

// Q: scope correct with this?
var imageCropWidth = 0;
var imageCropHeight = 0;
var cropPointX = 0;
var cropPointY = 0;



// TEST: Imagecontainer indsat istedet for eg $('#uploaded-image').Jcrop({
function initCrop() {
    $('#Imagecontainer').Jcrop({
        onChange: setCoordsAndImgSize,
        aspectRatio: 1
    });

    $("#hl-crop-image").on("click", function (e) {
        e.preventDefault();
        cropImage();
    });
}

function destroyCrop() {
    var jcropApi = $('#Imagecontainer').data('Jcrop');

    if (jcropApi !== undefined) {
        jcropApi.destroy();
        $('#Imagecontainer').attr('style', "").attr("src", "");
    }
}

function setCoordsAndImgSize(e) {

    imageCropWidth = e.w;
    imageCropHeight = e.h;

    cropPointX = e.x;
    cropPointY = e.y;
}

function cropImage() {

    if (imageCropWidth === 0 && imageCropHeight === 0) {
        alert("Please select crop area.");
        return;
    }

    // when sending an AJAX request 'manually' in Orchard or MVC in general, the antiforgery token must be supplied too
    var form = $('#FormUpload')[0];
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    // alert("Value of AF token: " + token); // works as advertised!    

    $.ajax({
        url: 'http://localhost:30321/OrchardLocal/Image/CropImage',
        type: 'POST',
        data: {
            imagePath: $("#hf-uploaded-image-path").val(), // denne sættes til url for posted pic
            cropPointX: cropPointX,
            cropPointY: cropPointY,
            imageCropWidth: imageCropWidth,
            imageCropHeight: imageCropHeight,
            __RequestVerificationToken: token
        },
        success: function (data) {

            alert("Success handler executing.");
            $("#hf-cropped-image-path").val(data.photoPath);

            $("#my-cropped-image")
                .attr("src", data.photoPath + "?t=" + new Date().getTime())
                .show();

            $("#btn-my-submit").fadeIn("slow");
        },
        error: function (data) {
            alert("Error handler executing.");
        }
    });
}

//************************************** JavaScript for cropping of image END **************************************

