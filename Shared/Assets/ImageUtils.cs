using System.Reflection;
using UnityEngine;

namespace Shared.Assets
{
    internal class ImageUtils
    {
        public static Texture2D LoadTexture(string path, int width, int height)
        {
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(path);

            if (stream == null)
            {
                return null;
            }

            var reader = new BinaryReader(stream);
            var buffer = new byte[stream.Length];

            reader.Read(buffer);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    IntPtr ptr = (IntPtr)p;
                    texture.LoadRawTextureData(ptr, buffer.Length);
                }
            }

            texture.Apply();

            return texture;
        }

        public static Texture2D LoadTextureFromFile(string filename, TextureFormat format = TextureFormat.BC7)
        {
            if (File.Exists(filename))
            {
                var bytes = File.ReadAllBytes(filename);
                var texture = new Texture2D(2, 2, format, false);

                texture.LoadImage(bytes);

                return texture;
            }

            return null;
        }
    }
}
