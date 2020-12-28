# BlobIO
 This package adds the feature of letting the user download arbitrary data as a file or load a local file of the user's choice in the Unity WebGL build.

![2020-12-28 22-08-03 mp4](https://user-images.githubusercontent.com/16096562/103216778-3204ed00-495a-11eb-8df6-9620f9a81d83.gif)

## Requirements
 This project is built with Unity 2019.4.13f1.

## Installation
 This package is made for Unity Package Manager (UPM).
 
 1. Open `Window` > `Package Manager` window.
 2. Click `+` > `Add package from git URL`.
 3. Type `https://github.com/ruccho/BlobIO.git?path=/Packages/io.github.ruccho.blobio` and click `Add`.

## Usage

```csharp:Sample.cs

using Ruccho.BlobIO;


public void LoadText()
{
    BlobIO.MakeUpload((f) =>
    {
        Debug.Log($"Filename: \"{f.Filename}\"");
        Debug.Log($"Loaded text: {Encoding.UTF8.GetString(f.Data)}");
    }, ".txt");
}

public void SaveText(string text)
{
    BlobIO.MakeDownloadText(text, "example.txt");
}

```