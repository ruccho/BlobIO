using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Ruccho.BlobIO.Examples
{
    public class BlobIOExample : MonoBehaviour
    {
        [SerializeField] private InputField targetInputField = default;
        [SerializeField] private string overlayMessage = "Click to select file...";
        [SerializeField] private string overlayCancelLabel = "Cancel";

        private void Start()
        {
            BlobIO.Initialize(overlayMessage, overlayCancelLabel);
        }

        public void Load()
        {
            BlobIO.MakeUpload((f) =>
            {
                Debug.Log($"BlobIOExample: File \"{f.Filename}\" uploaded. ");
                targetInputField.text = Encoding.UTF8.GetString(f.Data);
            }, ".txt");
        }

        public void Save()
        {
            BlobIO.MakeDownloadText(targetInputField.text, "example.txt");
        }
    }
}