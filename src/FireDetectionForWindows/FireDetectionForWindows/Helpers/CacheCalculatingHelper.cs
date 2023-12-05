using Windows.Storage;

namespace FireDetectionForWindows.Helpers;
public class CacheCalculatingHelper
{
    public CacheCalculatingHelper()
    {
    }

    public async Task<long> GetCacheSize()
    {
        var inputCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "input_Fire");
        var outputCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "output_Fire");
        long inputSize, outputSize, totalSize;

        // 计算上述文件夹内的所有文件大小
        if (Directory.Exists(inputCache))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(inputCache);
            inputSize = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
        }
        else
        {
            inputSize = 0;
        }
        if (Directory.Exists(outputCache))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(outputCache);
            outputSize = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
        }
        else
        {
            outputSize = 0;
        }

        totalSize = inputSize + outputSize;
        await Task.CompletedTask;
        return totalSize;
    }
}
