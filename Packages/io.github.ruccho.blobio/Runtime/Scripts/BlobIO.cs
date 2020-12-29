using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace Ruccho.BlobIO
{
    public static class BlobIO
    {
        [DllImport("__Internal")]
        private static extern void BlobIOMakeDownload(byte[] array, int length, string mime, string filename);

        [DllImport("__Internal")]
        private static extern void BlobIOInitialize(Action<int, IntPtr, int, string> pInvokeCallback, string overlayHtml = "");

        [DllImport("__Internal")]
        private static extern void BlobIOMakeUpload(int state, string accept);

        public static void MakeDownload(byte[] array, string mime, string filename)
        {
#if UNITY_WEBGL
            if (Application.isEditor)
            {
                Debug.LogWarning("BlobIO can only be used in runtime.");
                return;
            }
#else
            throw new PlatformNotSupportedException();
#endif
            BlobIOMakeDownload(array, array.Length, mime, filename);
        }

        public static void MakeDownloadTextureAsPNG(Texture2D texture, string filename)
        {
            var data = texture.EncodeToPNG();
            MakeDownload(data, "image/png", filename);
        }

        public static void MakeDownloadTextureAsJPG(Texture2D texture, string filename)
        {
            var data = texture.EncodeToJPG();
            MakeDownload(data, "image/jpeg", filename);
        }

        public static void MakeDownloadText(string text, string filename, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var data = encoding.GetBytes(text);
            MakeDownload(data, "text/plain", filename);
        }


        private static int publishedState = 0;

        private static Dictionary<int, Action<UploadedFile>> OnUploadedCallbacks { get; } =
            new Dictionary<int, Action<UploadedFile>>();

        private static bool isInitialized = false;

        public static void Initialize(string overlayMessage, string overlayCancelLabel)
        {
            string overlayHtml = @"<div id=""blobio-upload-overlay"" style=""background-color: rgba(0, 0, 0, 0.75); position:absolute; width: 100%; height: 100%; margin: 0; top: 0; display: table; color: #fff;""><div id=""blobio-upload-area"" style=""position: relative; width: 100%; height: 100%; cursor:pointer;""><div id=""blobio-upload-container"" style=""position: absolute; top: 50%; left: 50%; transform: translateY(-50%) translateX(-50%); text-align: center;""><p>" + overlayMessage  +@"</p><br><input type=""button"" value=""" + overlayCancelLabel + @""" id=""blobio-button-cancel""><input type=""file"" style=""display: none"" id=""blobio-file""></div></div></div>";
            Initialize(overlayHtml);
        }

        public static void Initialize(string overlayHtml)
        {
#if UNITY_WEBGL
            if (Application.isEditor)
            {
                Debug.LogWarning("BlobIO can only be used in runtime.");
                return;
            }
#else
            throw new PlatformNotSupportedException();
#endif
            if (!isInitialized)
            {
                BlobIOInitialize(UploadCallback, overlayHtml);
                isInitialized = true;
            }
            else
            {
                Debug.LogWarning("BlobIO is already initialized.");
            }
        }

        public static void MakeUpload(Action<UploadedFile> callback, string accept = "")
        {
#if UNITY_WEBGL
            if (Application.isEditor)
            {
                Debug.LogWarning("BlobIO can only be used in runtime.");
                return;
            }
#else
            throw new PlatformNotSupportedException();
#endif

            if (!isInitialized)
            {
                BlobIOInitialize(UploadCallback);
                isInitialized = true;
            }

            var stateId = publishedState;
            publishedState++;
            OnUploadedCallbacks.Add(stateId, callback);

            BlobIOMakeUpload(stateId, accept);
        }

        [MonoPInvokeCallback(typeof(Action<int, IntPtr, int, string>))]
        private static void UploadCallback(int stateId, IntPtr dataPtr, int length, string filename)
        {
            try
            {
                byte[] data = new byte[length];
                Marshal.Copy(dataPtr, data, 0, length);

                if (OnUploadedCallbacks.TryGetValue(stateId, out var callback))
                {
                    OnUploadedCallbacks.Remove(stateId);
                    try
                    {
                        callback?.Invoke(new UploadedFile(filename, data));
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        throw;
                    }
                }
                else
                {
                    Debug.LogWarning("BlobIo: Unregistered state.");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public class UploadedFile
        {
            public UploadedFile(string filename, byte[] data)
            {
                Filename = filename;
                Data = data;
            }

            public string Filename { get; }
            public byte[] Data { get; }
        }
    }
}