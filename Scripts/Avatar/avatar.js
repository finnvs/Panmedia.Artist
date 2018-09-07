// ================================================================
//  Description: Avatar Upload supporting script
//  License:     MIT - check License.txt file for details
//  Author:      Codative Corp. (http://www.codative.com/) 
//  with some site specific changes by Panmedia (www.panmedia.dk)
// ================================================================
var jcrop_api,
    boundx,
    boundy,
    xsize,
    ysize;

// ToDo - change the size limit of the file. You may need to change web.config if larger files are necessary.
var maxSizeAllowed = 5;     // Upload limit in MB
var maxSizeInBytes = maxSizeAllowed * 1024 * 1024;
var keepUploadBox = false;  // ToDo - Remove if you want to keep the upload box
var keepCropBox = false;    // ToDo - Remove if you want to keep the crop box
var rotateValue = 0;
// fix for paths where an absolute url is needed
var environ = window.location.host;
if (environ === "localhost:30321") {
    var baseurl = window.location.protocol + "//" + window.location.host + "/" + "OrchardLocal/";
} else {
    var baseurl = window.location.protocol + "//" + window.location.host + "/";
}
// alert("BaseURL: " + baseurl);

var progressCustom = new LoadingOverlayProgress({
    bar: {
        "background": 'red',
        "top": "50px",
        "height": "30px",
        "border-radius": "15px"
    },
    text: {
        "color": 'red',
        "font-family": "monospace",
        "top": "25px"
    }
});

// example - call specific function only when window has loaded
// window.addEventListener('load', pageInit);

// $(function () {
function pageInit() {
    if (typeof $('#avatar-upload-form') !== undefined) {
        initAvatarUpload();
        $('#avatar-max-size').html(maxSizeAllowed);
        $('#avatar-upload-form button[id=cancel_btn]').click(function () {
            cancel_btn_handler();
        });
        $('#avatar-upload-form input:file').on("change", function (e) {
            var files = e.currentTarget.files;
            for (var x in files) {
                if (files[x].name !== "item" && typeof files[x].name !== "undefined") {
                    if (files[x].size <= maxSizeInBytes) {
                        // Submit the selected file
                        $('#avatar-upload-form .upload-file-notice').removeClass('bg-danger');
                        // alert("Submitting file");
                        $('#avatar-upload-form').submit();
                    } else {
                        // File too large
                        $('#avatar-upload-form .upload-file-notice').addClass('bg-danger');
                    }
                }
            }           
        });
    }
}


function initAvatarUpload() {
    // alert("initAvatarUpload was called.")
    $('#avatar-upload-form').ajaxForm({

        // standard progress bar
        //beforeSend: function () {
        //    updateProgress(0);
        //    $('#avatar-upload-form').addClass('hidden');
        //},
        //uploadProgress: function (event, position, total, percentComplete) {
        //    updateProgress(percentComplete);
        //},

        // overlay page while uploading
        beforeSend: function () {
            $.LoadingOverlay("show", {
                custom: progressCustom.Init()
            });
        },        
        success: function (data) {
            updateProgress(100);
            if (data.success === false) {
                $('#status').html(data.errorMessage);
            } else {
                // alert("Success handler initiated");
                $('#preview-pane .preview-container img').attr('src', data.fileName);
                var img = $('#crop-avatar-target');
                img.attr('src', data.fileName);

                if (!keepUploadBox) {
                    $('#avatar-upload-box').addClass('hidden');
                }
                $('#avatar-crop-box').removeClass('hidden');
                initAvatarCrop(img);
            }
        },

        error: function (data) {
            alert("Error handler executing:" + data.responsetext);
        },

        complete: function (xhr) {
            $.LoadingOverlay("hide");
        }
    });
}

function updateProgress(percentComplete) {
    $('.upload-percent-bar').width(percentComplete + '%');
    $('.upload-percent-value').html(percentComplete + '%');
    if (percentComplete === 0) {
        $('#upload-status').empty();
        $('.upload-progress').removeClass('hidden');
    }
}

function initAvatarCrop(img) {
    img.Jcrop({
        onChange: updatePreviewPane,
        onSelect: updatePreviewPane,
        aspectRatio: xsize / ysize
    }, function () {
        var bounds = this.getBounds();
        boundx = bounds[0];
        boundy = bounds[1];

        jcrop_api = this;
        jcrop_api.setOptions({ allowSelect: true });
        jcrop_api.setOptions({ allowMove: true });
        jcrop_api.setOptions({ allowResize: true });
        jcrop_api.setOptions({ aspectRatio: 1 });

        // Maximise initial selection around the centre of the image,
        // but leave enough space so that the boundaries are easily identified.
        var padding = 10;
        var shortEdge = (boundx < boundy ? boundx : boundy) - padding;
        var longEdge = boundx < boundy ? boundy : boundx;
        var xCoord = longEdge / 2 - shortEdge / 2;
        jcrop_api.animateTo([xCoord, padding, shortEdge, shortEdge]);

        var pcnt = $('#preview-pane .preview-container');
        xsize = pcnt.width();
        ysize = pcnt.height();
        $('#preview-pane').appendTo(jcrop_api.ui.holder);
        jcrop_api.focus();
    });
    // call the ghostbusters, show rotate buttons            
    initRotateAvatar();
    $('#rotate-apply').removeClass('hidden');
    $('#rotate-minus90').removeClass('hidden');
    $('#rotate-180').removeClass('hidden');
    $('#rotate-cancel').removeClass('hidden');
}

function updatePreviewPane(c) {
    if (parseInt(c.w) > 0) {
        var rx = xsize / c.w;
        var ry = ysize / c.h;

        $('#preview-pane .preview-container img').css({
            width: Math.round(rx * boundx) + 'px',
            height: Math.round(ry * boundy) + 'px',
            marginLeft: '-' + Math.round(rx * c.x) + 'px',
            marginTop: '-' + Math.round(ry * c.y) + 'px'
        });
    }
}

function saveAvatar() {
    var img = $('#preview-pane .preview-container img');
    // add AntiforgeryToken
    var form = $('#avatar-upload-form')[0];
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    // alert("Value of AF token: " + token); // works as advertised! 
    $('#avatar-crop-box button').addClass('disabled');

    $.ajax({
        type: "POST",        
        // url: "Avatar/Save",  // dette skal være en absolut url, ellers kommer Orchards routing ikke i spil
        // og requestet vil gå til /OrchardLocal/Panmedia.Artist/Artist/Avatar/Save, som ikke findes        
        // url: 'http://localhost:30321/OrchardLocal/Avatar/Save',  // Controller Avatar, method Save - url håndteres af Orchard routing
        url: baseurl + 'Avatar/Save',
        traditional: true,

        beforeSend: function () {
            $.LoadingOverlay("show", {
                custom: progressCustom.Init()
            });
        },
        complete: function () {
            $.LoadingOverlay("hide");
        },

        data: {
            w: img.css('width'),
            h: img.css('height'),
            l: img.css('marginLeft'),
            t: img.css('marginTop'),
            fileName: img.attr('src'),
            rotateVal: rotateValue,
            __RequestVerificationToken: token
        }
    }).done(function (data) {
        if (data.success === true) {
            $('#avatar-result img').attr('src', data.avatarFileLocation);
            $('#avatar-result').removeClass('hidden');
            if (!keepCropBox) {
                $('#avatar-crop-box').addClass('hidden');
            }
            if (data.redirecturl) {
                // alert("Redirect string: " + data.redirecturl);
                window.location.href = baseurl + data.redirecturl;
            }
        } else {
            alert(data.errorMessage);
        }
    }).fail(function (e) {
        alert('Cannot upload avatar at this time - current baseurl is : ' + baseurl);
    });
}

function initRotateAvatar() {
    var img = $('#preview-pane .preview-container img');    
    $('#rotate-apply').on('click', function () {
        // simplest possible call to jQueryRotate library - and it works!        
        rotateValue = 90;
        img.rotate(90);        
    });

    $('#rotate-minus90').on('click', function () {               
        rotateValue = -90;
        img.rotate(-90);
    });

    $('#rotate-180').on('click', function () {
        rotateValue = 180;
        img.rotate(180);
    });

    $('#rotate-cancel').on('click', function () {                
        rotateValue = 0;
        img.rotate(0);
    });

    // Canvas implementation as used in Orchard.ImageEditor - not fully functional atm

    //var pcnt = $('#preview-pane .preview-container');
    //xsize = pcnt.width();
    //ysize = pcnt.height();

    // var img = $('#preview-pane .preview-container img');
    //alert("img src attr: " + img.attr("src"));
    //alert("pcnt height: " + ysize);
    //alert("img css height: " + img.css("height"));

    //$('#rotate-apply').on('click', function () {
    //    var image = new Image();
    //    image.onload = function () {
    //        var canvas = document.createElement('canvas');
    //        var degrees = "90"; // $("#rotate-angle").val();

    //        if (degrees != "180") {
    //            //canvas.width = img.css("height");
    //            //canvas.height = img.css("width");
    //            canvas.width = ysize; // originally, w and h are interchanged to h and w
    //            canvas.height = xsize;
    //            //canvas.width = xsize; // experiment to see what will happen keeping w and h - image disappears in preview :-)
    //            //canvas.height = ysize;
    //        } else {
    //            canvas.width = img.css("width");
    //            canvas.height = img.css("height");
    //        }

    //        $(img).css({
    //            width: canvas.width,
    //            height: canvas.height
    //        });

    //        var context = canvas.getContext('2d');

    //        context.translate(canvas.width / 2, canvas.height / 2);
    //        context.rotate(degrees * Math.PI / 180);
    //        context.drawImage(image, -image.width / 2, -image.height / 2, image.width, image.height);

    //        // img src repræsenterer nu det roterede billede. Det ændres, men drejer bare ikke..
    //        img.src = canvas.toDataURL("image/png");           
            
    //    };
    //    image.src = img.attr("src"); // img.src; er undefined så vi bruger url istedet
    //});
}

function uploadAvatar() {    
    var form = $('#avatar-upload-form')[0];
    // var selectedFile = ($("#UploadedFile"))[0].files[0];
    var img = $('#preview-pane .preview-container img');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    // create a canvas, fill it with the cropped / rotated 'img' 
    // and utilize canvas.toDataUrl function in order to send a blob-as-file    
    var canvas = document.createElement('canvas');    
    var context = canvas.getContext('2d');
    context.drawImage(img[0], 0, 0);
    var dataUrl = canvas.toDataURL("image/png");
    var dataString = new FormData();
    var blob = dataURItoBlob(dataUrl);
    // hmm.this will send a file with filename 'blob'.. and not tilted, even if rotated?   
    dataString.append("file", blob);
    dataString.append("filename", img[0].fileName);
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
        // url: 'http://localhost:30321/OrchardLocal/Profile/ImageEdit',  //Server script to process data
        url: baseurl + 'Profile/ImageEdit',
        type: 'POST',
        beforeSend: function () {
            $.LoadingOverlay("show", {
                custom: progressCustom.Init()
            });
        },
        complete: function () {
            $.LoadingOverlay("hide");
        },
        xhr: function () { 
            var myXhr = $.ajaxSettings.xhr();           
            return myXhr;
        },
        //Ajax events
        success: successHandler,
        error: errorHandler,        
        data: dataString,
        //Options to tell jQuery not to process data or worry about content-type.
        cache: false,
        contentType: false,
        processData: false
    });
}

function successHandler(response) {
   
    if (response.redirect) {
        window.location.href = response.redirect;
    }
    else {
        // Huu - ballade. Redirect..
        window.location = baseurl;  //'http://localhost:30321/OrchardLocal/ProfileRedirect';
    }
}

function errorHandler(xhr, ajaxOptions, thrownError) {
    alert("There was an error attempting to upload the file. (" + thrownError + ")");
}

// support sending a cropped and rotated image as a blob -
// ref http://stackoverflow.com/questions/18253378/javascript-blob-upload-with-formdata

function dataURItoBlob(dataURI) {
    var byteString = atob(dataURI.split(',')[1]);
    var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
    var ab = new ArrayBuffer(byteString.length);
    var ia = new Uint8Array(ab);
    for (var i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }
    var bb = new Blob([ab], { "type": mimeString });
    return bb;
}

// go to site root on cancel image upload
function cancelImageUpload() {    
    window.location = baseurl;
}

