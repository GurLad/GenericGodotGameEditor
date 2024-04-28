using Godot;
using System;
using System.Collections.Generic;

namespace GGE.Internal
{
	public static class GGEExtensions
    {
        public static int FindIndex(this ItemList itemList, string query)
        {
            for (int i = 0; i < itemList.ItemCount; i++)
            {
                if (itemList.GetItemText(i) == query)
                {
                    return i;
                }
            }
            return -1;
        }

        public static string FixFileName(this string str)
        {
            return str.Replace("\"", "").Replace("\\", "").Replace("/", "").Replace(":", "").Replace("?", "").Replace("|", "").Replace("*", "").Replace("<", "").Replace(">", "");
        }

        public static List<Texture2D> Split(this Image source, int numFrames)
        {
            List<Texture2D> result = new List<Texture2D>();
            int frameWidth = source.GetWidth() / numFrames;
            for (int i = 0; i < numFrames; i++)
            {
                result.Add(ImageTexture.CreateFromImage(source.GetRegion(new Rect2I(i * frameWidth, 0, frameWidth, source.GetHeight()))));
            }
            return result;
        }

        public static Image Combine(this List<Texture2D> source)
        {
            if (source.Count > 0)
            {
                Image template = source[0].GetImage();
                Image result = Image.Create(template.GetWidth() * source.Count, template.GetHeight(), false, template.GetFormat());
                Rect2I sourceRect = new Rect2I(0, 0, template.GetWidth(), template.GetHeight());
                for (int i = 0; i < source.Count; i++)
                {
                    result.BlitRect(source[i].GetImage(), sourceRect, new Vector2I(i * template.GetWidth(), 0));
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public static void Init(this FileDialog fileDialog)
        {
            fileDialog.Filters = new string[] { "*.png, *.gif, *.jpg ; Image files" };
            fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
            fileDialog.Access = FileDialog.AccessEnum.Filesystem;
            //Window root = GetTree().Root;
            //fileDialog.ContentScaleAspect = root.ContentScaleAspect;
            //fileDialog.ContentScaleFactor = root.ContentScaleFactor;
            //fileDialog.ContentScaleMode = root.ContentScaleMode;
            //fileDialog.ContentScaleSize = root.ContentScaleSize;
            //fileDialog.ContentScaleStretch = root.ContentScaleStretch;
            fileDialog.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
            //fileDialog.Mode = Window.ModeEnum.Maximized;
            fileDialog.UseNativeDialog = true;
        }
    }
}