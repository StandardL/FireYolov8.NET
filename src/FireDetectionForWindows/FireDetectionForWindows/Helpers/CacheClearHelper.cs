using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireDetectionForWindows.Services;
using Windows.Storage;

namespace FireDetectionForWindows.Helpers;
public class CacheClearHelper
{
    public CacheClearHelper()
    {
    }

    public async Task Clean()
    {
        var inputCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "input_Fire");
        var outputCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "output_Fire");

        // 清理上述文件夹内的所有文件
        if (Directory.Exists(inputCache))
        {
            Directory.Delete(inputCache, true);
        }
        if (Directory.Exists(outputCache))
        {
            Directory.Delete(outputCache, true);
        }

        await Task.CompletedTask;
    }
}
