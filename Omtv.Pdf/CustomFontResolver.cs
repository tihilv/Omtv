using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Internal;
using SixLabors.Fonts;

namespace Omtv.Pdf
{
  public class CustomFontResolver : IFontResolver
  {
    private static readonly Dictionary<string, FontFamilyModel> InstalledFonts = new Dictionary<string, FontFamilyModel>();
    private static readonly List<string> SSupportedFonts = new List<String>();

    public string DefaultFontName => "Arial";

    static CustomFontResolver()
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        var fonts = Directory.GetFiles("/Library/Fonts/", "*.ttf", SearchOption.AllDirectories).ToList();
        SetupFontsFiles(fonts);
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        var fonts = CustomLinuxSystemFontResolver.Resolve();
        SetupFontsFiles(fonts);
      }
      else
      {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
          throw new NotImplementedException("MyFontResolver not implemented for this platform (PdfSharpCore.Utils.MyFontResolver.cs).");
        string path1 = Environment.ExpandEnvironmentVariables("%SystemRoot%\\Fonts");
        List<string> stringList = new List<string>();
        string[] files1 = Directory.GetFiles(path1, "*.ttf", SearchOption.AllDirectories);
        stringList.AddRange(files1);
        string path2 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\Microsoft\\Windows\\Fonts");
        if (Directory.Exists(path2))
        {
          string[] files2 = Directory.GetFiles(path2, "*.ttf", SearchOption.AllDirectories);
          stringList.AddRange(files2);
        }

        SetupFontsFiles(stringList);
      }
    }

    public static void SetupFontsFiles(IEnumerable<string> sSupportedFonts)
    {
      List<FontFileInfo> source = new List<FontFileInfo>();
      foreach (string sSupportedFont in sSupportedFonts)
      {
        try
        {
          FontFileInfo fontFileInfo = FontFileInfo.Load(sSupportedFont);
          source.Add(fontFileInfo);
          SSupportedFonts.Add(sSupportedFont);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine(ex);
        }
      }

      foreach (IGrouping<string, FontFileInfo> fontList in source.GroupBy((Func<FontFileInfo, string>)(info => info.FamilyName)))
      {
        try
        {
          string key = fontList.Key;
          FontFamilyModel fontFamilyModel = DeserializeFontFamily(key, fontList);
          InstalledFonts.Add(key.ToLower(), fontFamilyModel);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine(ex);
        }
      }
    }

    private static FontFamilyModel DeserializeFontFamily(string fontFamilyName, IEnumerable<FontFileInfo> fontList)
    {
      FontFamilyModel fontFamilyModel = new FontFamilyModel
      {
        Name = fontFamilyName
      };
      if (fontList.Count() == 1)
      {
        fontFamilyModel.FontFiles.Add(XFontStyle.Regular, fontList.First().Path);
      }
      else
      {
        foreach (FontFileInfo font in fontList)
        {
          XFontStyle key = font.GuessFontStyle();
          if (!fontFamilyModel.FontFiles.ContainsKey(key))
            fontFamilyModel.FontFiles.Add(key, font.Path);
        }
      }

      return fontFamilyModel;
    }

    public virtual byte[] GetFont(string faceFileName)
    {
      using (MemoryStream destination = new MemoryStream())
      {
        foreach (var f in InstalledFonts)
        {
          Console.WriteLine($"Font: {f.Key} - {f.Value.Name}");
        }
        Console.WriteLine($"Required font: {faceFileName}");
        
        string path = "";
        try
        {
          path = SSupportedFonts.ToList().First((Func<string, bool>)(x => x.ToLower().Contains(Path.GetFileName(faceFileName).ToLower())));
          using (Stream stream = File.OpenRead(path))
          {
            stream.CopyTo(destination);
            destination.Position = 0L;
            return destination.ToArray();
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          throw new Exception("No Font File Found - " + faceFileName + " - " + path);
        }
      }
    }

    public bool NullIfFontNotFound { get; set; }

    public virtual FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
      if (InstalledFonts.Count == 0)
        throw new FileNotFoundException("No Fonts installed on this device!");
      FontFamilyModel fontFamilyModel;
      if (InstalledFonts.TryGetValue(familyName.ToLower(), out fontFamilyModel))
      {
        if (isBold & isItalic)
        {
          string path;
          if (fontFamilyModel.FontFiles.TryGetValue(XFontStyle.BoldItalic, out path))
            return new FontResolverInfo(Path.GetFileName(path));
        }
        else if (isBold)
        {
          string path;
          if (fontFamilyModel.FontFiles.TryGetValue(XFontStyle.Bold, out path))
            return new FontResolverInfo(Path.GetFileName(path));
        }
        else
        {
          string path;
          if (isItalic && fontFamilyModel.FontFiles.TryGetValue(XFontStyle.Italic, out path))
            return new FontResolverInfo(Path.GetFileName(path));
        }

        string path1;
        return fontFamilyModel.FontFiles.TryGetValue(XFontStyle.Regular, out path1) ? new FontResolverInfo(Path.GetFileName(path1)) : new FontResolverInfo(Path.GetFileName(fontFamilyModel.FontFiles.First<KeyValuePair<XFontStyle, string>>().Value));
      }

      return NullIfFontNotFound ? null : new FontResolverInfo(Path.GetFileName(InstalledFonts.First().Value.FontFiles.First<KeyValuePair<XFontStyle, string>>().Value));
    }

    private readonly struct FontFileInfo
    {
      private FontFileInfo(string path, FontDescription fontDescription)
      {
        Path = path;
        FontDescription = fontDescription;
      }

      public string Path { get; }

      public FontDescription FontDescription { get; }

      public string FamilyName => FontDescription.FontFamilyInvariantCulture;

      public XFontStyle GuessFontStyle()
      {
        switch (FontDescription.Style)
        {
          case FontStyle.Bold:
            return XFontStyle.Bold;
          case FontStyle.Italic:
            return XFontStyle.Italic;
          case FontStyle.BoldItalic:
            return XFontStyle.BoldItalic;
          default:
            return XFontStyle.Regular;
        }
      }

      public static FontFileInfo Load(string path)
      {
        FontDescription fontDescription = FontDescription.LoadDescription(path);
        return new FontFileInfo(path, fontDescription);
      }
    }
  }
}