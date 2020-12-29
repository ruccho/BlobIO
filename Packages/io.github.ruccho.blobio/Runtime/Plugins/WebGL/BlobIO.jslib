mergeInto(LibraryManager.library, {

    // BlobIOMakeDownload(byte[] array, int length, string mime, string filename)
    BlobIOMakeDownload: function(array, length, mime, filename) {
        var subArray = HEAPU8.subarray(array, array + length);
        var mimeStr = Pointer_stringify(mime);
        var filenameStr = Pointer_stringify(filename);
        var blob = new Blob([subArray], { type: mimeStr });

        var url = window.URL.createObjectURL(blob);

        console.log(url);

        var a = window.document.createElement("a");
        window.document.body.appendChild(a);
        a.download = filenameStr;
        a.href = url;
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    },

    // BlobIOInitialize(Action<int, IntPtr, int, string> callback, string overlayHtml)
    BlobIOInitialize: function(uploadCallbackPtr, overlayHtmlPtr) {

        if (!document.getElementById('blobio-upload-overlay')) {
            var element = document.createElement('div');
            var overlayHtml = Pointer_stringify(overlayHtmlPtr);
            if (overlayHtml) {
                element.innerHTML = overlayHtml;
            } else {
                var overlayHtmlDefault = '<div id="blobio-upload-overlay" style="background-color: rgba(0, 0, 0, 0.75); position:absolute; width: 100%; height: 100%; margin: 0; top: 0; display: table; color: #fff;"><div id="blobio-upload-area" style="position: relative; width: 100%; height: 100%; cursor:pointer;"><div id="blobio-upload-container" style="position: absolute; top: 50%; left: 50%; transform: translateY(-50%) translateX(-50%); text-align: center;"><span>Click here to select file...</span><br><input type="button" value="Cancel" id="blobio-button-cancel"><input type="file" style="display: none" id="blobio-file"></div></div></div>';
                element.innerHTML = overlayHtmlDefault;
            }
            document.getElementById('unityContainer').appendChild(element);

            document.getElementById('blobio-button-cancel').addEventListener('click', function(e) {
                document.getElementById('blobio-upload-overlay').style.display = 'none';
                e.stopPropagation();
            });

            var uploadArea = document.getElementById('blobio-upload-area');
            uploadArea.addEventListener('click', function(e) {
                document.getElementById('blobio-file').click();
            });

            document.getElementById('blobio-file').addEventListener('change', function(changeEvt) {
                var files = changeEvt.target.files;
                if (files.length == 0) return;

                var reader = new FileReader();
                reader.addEventListener('load', function(loadEvt) {

                    var state = uploadArea.dataset.state;
                    var buffer = new Uint8Array(loadEvt.target.result);
                    var length = buffer.byteLength;
                    console.log(length);
                    var dataPtr = _malloc(length);
                    HEAPU8.set(buffer, dataPtr);

                    var filename = files[0].name;
                    var filenameSize = lengthBytesUTF8(filename) + 1;
                    var filenamePtr = _malloc(filenameSize);
                    stringToUTF8(filename, filenamePtr, filenameSize);

                    // void CALLBACK(IntPtr dataPtr, int length, string filename)
                    Runtime.dynCall('viiii', uploadCallbackPtr, [state, dataPtr, length, filenamePtr]);

                    _free(dataPtr);
                    _free(filenamePtr);

                });

                reader.readAsArrayBuffer(files[0]);
                document.getElementById('blobio-upload-overlay').style.display = 'none';

            }, false);

            document.getElementById('blobio-upload-overlay').style.display = 'none';
        }
    },

    // BlobIOMakeUplaod(int state, string accept)
    BlobIOMakeUpload: function(state, accept) {

        var overlay = document.getElementById('blobio-upload-overlay');
        if (!overlay) {
            alert('BlobIO: Overlay is not found.');
            return;
        }

        overlay.style.display = 'initial';
        document.getElementById('blobio-upload-area').dataset.state = state;
        document.getElementById('blobio-file').accept = Pointer_stringify(accept);

    }

});