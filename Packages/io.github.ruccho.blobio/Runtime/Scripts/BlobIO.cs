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
        private static extern void BlobIOMakeUpload(int state, string accept,
            Action<int, IntPtr, int, string> pInvokeCallback);

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

            var stateId = publishedState;
            publishedState++;
            OnUploadedCallbacks.Add(stateId, callback);

            BlobIOMakeUpload(stateId, accept, UploadCallback);
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
                        callback?.Invoke(new UploadedFile("", data));
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