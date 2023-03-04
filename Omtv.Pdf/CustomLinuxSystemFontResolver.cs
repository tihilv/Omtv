using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Omtv.Pdf
{
    public static class CustomLinuxSystemFontResolver
    {
        private const string libfontconfig = "libfontconfig.so.1";
        private static readonly Lazy<IntPtr> fcConfig = new Lazy<IntPtr>(new Func<IntPtr>(CustomLinuxSystemFontResolver.FcInitLoadConfigAndFonts));

        [DllImport("libfontconfig.so.1")]
        private static extern IntPtr FcInitLoadConfigAndFonts();

        [DllImport("libfontconfig.so.1")]
        public static extern FcPatternHandle FcPatternCreate();

        [DllImport("libfontconfig.so.1")]
        public static extern int FcPatternGetString(IntPtr p, [MarshalAs(UnmanagedType.LPStr)] string obj, int n, ref IntPtr s);

        [DllImport("libfontconfig.so.1")]
        public static extern void FcPatternDestroy(IntPtr pattern);

        [DllImport("libfontconfig.so.1")]
        public static extern FcObjectSetHandle FcObjectSetCreate();

        [DllImport("libfontconfig.so.1")]
        public static extern int FcObjectSetAdd(
            FcObjectSetHandle os,
            [MarshalAs(UnmanagedType.LPStr)] string obj);

        [DllImport("libfontconfig.so.1")]
        public static extern void FcObjectSetDestroy(IntPtr os);

        [DllImport("libfontconfig.so.1")]
        public static extern FcFontSetHandle FcFontList(
            IntPtr config,
            FcPatternHandle pattern,
            FcObjectSetHandle os);

        [DllImport("libfontconfig.so.1")]
        public static extern void FcFontSetDestroy(IntPtr fs);

        private static string GetString(IntPtr handle, string obj)
        {
            IntPtr zero = IntPtr.Zero;
            return FcPatternGetString(handle, obj, 0, ref zero) == 0 ? Marshal.PtrToStringAnsi(zero) : (string) null;
        }

        private static IEnumerable<string> ResolveFontConfig()
        {
            IntPtr config = fcConfig.Value;
            using (CustomLinuxSystemFontResolver.FcPatternHandle pattern = FcPatternCreate())
            {
                using (CustomLinuxSystemFontResolver.FcObjectSetHandle os = FcObjectSetHandle.Create("family", "style", "file"))
                {
                    using (CustomLinuxSystemFontResolver.FcFontSetHandle fs = FcFontList(config, pattern, os))
                    {
                        FcFontSet fset = fs.Read();
                        for (int index = 0; index < fset.nfont; ++index)
                        {
                            IntPtr handle = Marshal.ReadIntPtr(fset.fonts, index * Marshal.SizeOf<IntPtr>());
                            string str1 = GetString(handle, "family");
                            string str2 = GetString(handle, "style");
                            string str3 = GetString(handle, "file");
                            if (str1 != null && str2 != null && str3 != null)
                                yield return str3;
                        }
                    }
                }
            }
        }

        public static List<string> Resolve()
        {
            try
            {
                return ResolveFontConfig().Where<string>((Func<string, bool>) (x => x.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return ResolveFallback().Where<string>((Func<string, bool>) (x => x.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))).ToList();
            }
        }

        private static IEnumerable<string> ResolveFallback()
        {
            List<string> fontList = new List<string>();
            HashSet<string> stringSet = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
            foreach (string searchPath in SearchPaths())
            {
                if (!stringSet.Contains(searchPath))
                {
                    stringSet.Add(searchPath);
                    AddFontsToFontList(searchPath);
                }
            }
            return (IEnumerable<string>) fontList.ToArray();

            void AddFontsToFontList(string path)
            {
                if (!Directory.Exists(path))
                    return;
                foreach (string enumerateDirectory in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                    fontList.AddRange(Directory.EnumerateFiles(enumerateDirectory, "*", SearchOption.AllDirectories));
            }
        }

        private static IEnumerable<string> SearchPaths()
        {
            List<string> stringList = new List<string>();
            try
            {
                Regex regex = new Regex("<dir>(?<dir>.*)</dir>", RegexOptions.Compiled);
                using (StreamReader streamReader = new StreamReader((Stream) File.OpenRead("/etc/fonts/fonts.conf")))
                {
                    string input;
                    while ((input = streamReader.ReadLine()) != null)
                    {
                        Match match = regex.Match(input);
                        if (match.Success)
                        {
                            string str = match.Groups["dir"].Value.Trim();
                            if (str.StartsWith("~"))
                                str = Environment.GetEnvironmentVariable("HOME") + str.Substring(1);
                            stringList.Add(str);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
            stringList.Add("/usr/share/fonts");
            stringList.Add("/usr/local/share/fonts");
            stringList.Add(Environment.GetEnvironmentVariable("HOME") + "/.fonts");
            return (IEnumerable<string>) stringList;
        }

        public class FcPatternHandle : SafeHandle
        {
            private FcPatternHandle()
                : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => this.handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                FcPatternDestroy(this.handle);
                return true;
            }
        }

        public class FcObjectSetHandle : SafeHandle
        {
            private FcObjectSetHandle()
                : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => this.handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                FcObjectSetDestroy(this.handle);
                return true;
            }

            public static FcObjectSetHandle Create(params string[] objs)
            {
                FcObjectSetHandle os = FcObjectSetCreate();
                foreach (string str in objs)
                    FcObjectSetAdd(os, str);
                FcObjectSetAdd(os, "");
                return os;
            }
        }

        public struct FcFontSet
        {
            public int nfont;
            public int sfont;
            public IntPtr fonts;
        }

        public class FcFontSetHandle : SafeHandle
        {
            private FcFontSetHandle()
                : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => this.handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                FcFontSetDestroy(this.handle);
                return true;
            }

            public FcFontSet Read() => Marshal.PtrToStructure<CustomLinuxSystemFontResolver.FcFontSet>(this.handle);
        }
    }
}