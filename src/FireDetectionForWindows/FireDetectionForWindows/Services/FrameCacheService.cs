using Windows.Graphics.Imaging;
using Windows.Storage;

namespace FireDetectionForWindows.Services;
public class FrameCacheService
{
    private readonly SoftwareBitmap softwareBitmap;
    private readonly string cachePath;
    public FrameCacheService()
    {
        cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "input_Fire");
    }

    public FrameCacheService(SoftwareBitmap _softBit)
    {
        softwareBitmap = _softBit;
        cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "input_Fire");
    }

    public SoftwareBitmap GetSoftwareBitmap()
    {
        return softwareBitmap;
    }

    // 写入应用缓存数据文件夹
    public async Task WriteToFileAsync(string fileName)
    {
        if (!Directory.Exists(cachePath))
        {
            Directory.CreateDirectory(cachePath);
        }
        // 打开指定的缓存文件夹
        var folder = await StorageFolder.GetFolderFromPathAsync(cachePath);
        var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
        encoder.SetSoftwareBitmap(softwareBitmap);
        await encoder.FlushAsync();
    }

    // 返回应用缓存数据文件夹中的文件路径
    public async Task<string> GetFilePathAsync(string fileName)
    {
        var file = Path.Combine(cachePath, fileName);
        await Task.CompletedTask;
        return file;
    }
}
