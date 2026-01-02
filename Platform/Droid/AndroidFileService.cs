// NavigationDrawerStarter/Platform/Android/AndroidFileService.cs
using Android.OS;
using EfcToXamarinAndroid.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Environment = Android.OS.Environment;

namespace NavigationDrawerStarter.Platform.Droid
{
    public class AndroidFileService : IFileService
    {
        public async Task<string> PickFileAsync(string title, string[] fileTypes)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = title,
                    FileTypes = new FilePickerFileType(
                        new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.Android, fileTypes }
                        })
                };

                var result = await FilePicker.PickAsync(options);
                return result?.FullPath;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SaveFileAsync(string content, string fileName, string filePath)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch
            {
                return null;
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public string GetDownloadsPath()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                return Android.App.Application.Context.GetExternalFilesDir(Environment.DirectoryDownloads).AbsolutePath;
            }
            else
            {
                return Android.OS.Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;
            }
        }
    }
}