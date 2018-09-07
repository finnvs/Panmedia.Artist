using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Panmedia.Artist
{
    [OrchardFeature("Panmedia.Artist")]
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();        
            manifest.DefineScript("ImageUpload")
                .SetUrl("imageupload.js").SetDependencies("jQuery");
            manifest.DefineScript("jQueryBlockUI")
                .SetUrl("BlockUI/jquery.blockUI.js").SetDependencies("jQuery");
            manifest.DefineScript("LoadingOverlay")
                .SetUrl("LoadingOverlay/loadingoverlay.js").SetDependencies("jQuery");
            manifest.DefineScript("LoadingOverlayProgress")
                .SetUrl("LoadingOverlay/loadingoverlay_progress.js").SetDependencies("jQuery");
            manifest.DefineStyle("UploaderStyle").SetUrl("~/Panmedia.Artist/Styles/Uploader-style.css");

            // JCrop references
            manifest.DefineScript("JCrop")
               .SetUrl("JCrop/jquery.Jcrop.js").SetDependencies("jQuery");
            manifest.DefineStyle("JCropStyle").SetUrl("~/Panmedia.Artist/Styles/JCrop/jquery.Jcrop.css");

            // Avatar MS references for handling AJAX form errors 
            manifest.DefineScript("MicrosoftMvcAjax")
              .SetUrl("MicrosoftMvcAjax.js");
            manifest.DefineScript("MicrosoftAjax")
              .SetUrl("MicrosoftAjax.js");
            // Avatar references
            manifest.DefineScript("Avatar")
              .SetUrl("Avatar/avatar.js").SetDependencies("jQuery");
            manifest.DefineScript("AvatarJCrop")
              .SetUrl("Avatar/jquery.Jcrop.js").SetDependencies("jQuery");
            manifest.DefineScript("AvatarJQueryForm")
               .SetUrl("Avatar/jquery.form.js").SetDependencies("jQuery");
            manifest.DefineScript("AvatarJQueryRotate")
               .SetUrl("Avatar/jQueryRotate.js").SetDependencies("jQuery");            
            manifest.DefineScript("AvatarJQueryUnobtrusiveAjax")
               .SetUrl("jquery.unobtrusive-ajax.js").SetDependencies("jQuery");            
            manifest.DefineScript("AvatarRespond")
               .SetUrl("Avatar/respond.js").SetDependencies("jQuery");
            manifest.DefineScript("AvatarRespondMatchMedia")
              .SetUrl("Avatar/respond.matchmedia.addListener.js").SetDependencies("jQuery");

            manifest.DefineStyle("AvatarStyle").SetUrl("~/Panmedia.Artist/Styles/Avatar/avatar.css");
            manifest.DefineStyle("AvatarJCropStyle").SetUrl("~/Panmedia.Artist/Styles/Avatar/jquery.Jcrop.css");


            // Cropit jQuery plugin Test            
            manifest.DefineScript("jQueryCropIt")
               .SetUrl("CropIt/jquery.cropit.js").SetDependencies("jQuery");
            manifest.DefineScript("CropIt")
               .SetUrl("CropIt/cropit.js").SetDependencies("jQuery");
            manifest.DefineScript("CropItConstants")
               .SetUrl("CropIt/constants.js").SetDependencies("jQuery");
            manifest.DefineScript("CropItOptions")
               .SetUrl("CropIt/options.js").SetDependencies("jQuery");
            manifest.DefineScript("CropItPlugin")
               .SetUrl("CropIt/plugin.js").SetDependencies("jQuery");
            manifest.DefineScript("CropItUtils")
               .SetUrl("CropIt/utils.js").SetDependencies("jQuery");
            manifest.DefineScript("CropItZoomer")
               .SetUrl("CropIt/zoomer.js").SetDependencies("jQuery");            

        }
    }
}
