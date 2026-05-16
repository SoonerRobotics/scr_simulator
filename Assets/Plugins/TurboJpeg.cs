using System;
using System.Runtime.InteropServices;

namespace Robots
{
    public static class TurboJpeg
    {
        private const string Lib = "turbojpeg";

        [DllImport(Lib)] public static extern IntPtr tjInitCompress();
        [DllImport(Lib)] public static extern int tjDestroy(IntPtr handle);
        [DllImport(Lib)] public static extern int tjCompress2(
            IntPtr handle,
            IntPtr srcBuf,
            int width, int pitch, int height,
            int pixelFormat,
            ref IntPtr jpegBuf,
            ref ulong jpegSize,
            int jpegSubsamp,
            int jpegQual,
            int flags
        );
        [DllImport(Lib)] public static extern void tjFree(IntPtr buf);

        private const int TJPF_RGB    = 0;
        private const int TJSAMP_420  = 2; // good balance of size/quality
        private const int TJFLAG_FASTDCT = 2;

        // Thread-local handle - one compressor per thread, no contention
        [ThreadStatic] private static IntPtr _handle;

        private static IntPtr Handle
        {
            get
            {
                if (_handle == IntPtr.Zero)
                    _handle = tjInitCompress();
                return _handle;
            }
        }

        public static byte[] Encode(byte[] rgb, int width, int height, int quality)
        {
            var jpegBuf = IntPtr.Zero;
            var jpegSize = 0UL;

            unsafe
            {
                fixed (byte* ptr = rgb)
                {
                    var result = tjCompress2(
                        Handle,
                        (IntPtr)ptr,
                        width, 0, height,
                        TJPF_RGB,
                        ref jpegBuf,
                        ref jpegSize,
                        TJSAMP_420,
                        quality,
                        TJFLAG_FASTDCT
                    );

                    if (result != 0)
                        throw new Exception("TurboJPEG encode failed");
                }
            }

            var output = new byte[jpegSize];
            Marshal.Copy(jpegBuf, output, 0, (int)jpegSize);
            tjFree(jpegBuf);
            return output;
        }
    }
}