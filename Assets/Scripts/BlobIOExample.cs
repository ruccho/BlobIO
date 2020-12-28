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

        public void Load()
        {
            BlobIO.MakeUpload((f) =>
            {
                Debug.Log($"BlobIOExample: File \"{f.Filename}\" uploaded. ");
                targetInputField.text = Encoding.UTF8.GetString(f.Data);
            });
        }

        public void Save()
        {
            BlobIO.MakeDownloadText(targetInputField.text, "example.txt");
        }
    }
}