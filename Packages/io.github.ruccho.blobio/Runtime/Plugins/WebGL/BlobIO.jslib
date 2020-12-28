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

    // BlobIOMakeUpload(int state, string accept, Action<int, IntPtr, int, string> callback)
    BlobIOMakeUpload: function(state, accept, callbackPtr) {
        var f = window.document.createElement("input");
        f.style.display = "none";
        f.type = "file";
        f.accept = Pointer_stringify(accept);

        f.addEventListener("change", function(changeEvt) {
            var files = changeEvt.target.files;
            if (files.length == 0) return;

            var reader = new FileReader();
            reader.addEventListener("load", function(loadEvt) {

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
                Runtime.dynCall("viiii", callbackPtr, [state, dataPtr, length, filenamePtr]);

                _free(dataPtr);
                _free(filenamePtr);

            });

            reader.readAsArrayBuffer(files[0]);

        }, false);

        f.click();
    }

});