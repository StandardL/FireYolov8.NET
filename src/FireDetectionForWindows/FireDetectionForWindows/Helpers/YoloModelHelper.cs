using FireDetectionForWindows.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Windows.Storage;
using Yolov8Net;

namespace FireDetectionForWindows.Helpers;

class YoloModelHelper
{
    private readonly Font font;
    private readonly string[] labels = { "fire", "smoke" };
    private readonly string outputPath;
    private readonly string modelPath;
    private int detect_count = 0;
    private readonly int modelVersion = 1;

    public YoloModelHelper()
    {
        font = SystemFonts.CreateFont("Arial", 20);
        outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "output_Fire");
        modelPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fire_Detection_for_Windows", "models");
    }

    public async Task FireYolo(string inputPath, string outputName)
    {
        var modelName = "FireDetect.onnx";

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        if (!Directory.Exists(modelPath))
        {
            Directory.CreateDirectory(modelPath);
        }

        // 模型文件不存在或版本过低时，从Assets中复制模型文件到本地
        var modelVersionStorageService = new ModelVersionStorageService();
        var _modelVersion = modelVersionStorageService.GetVersion();
        if (!Directory.Exists(Path.Combine(modelPath, modelName)) || _modelVersion < modelVersion)  // 或版本过低
        {
            var modelUri = new Uri("ms-appx:///Assets/FireDetect.onnx");
            var modelFile = await StorageFile.GetFileFromApplicationUriAsync(modelUri);
            await modelFile.CopyAsync(StorageFolder.GetFolderFromPathAsync(modelPath).AsTask().Result, modelName, NameCollisionOption.ReplaceExisting);

            modelVersionStorageService.SetVersion(modelVersion);
        }

        using var yolo = YoloV8Predictor.Create(Path.Combine(modelPath, modelName), labels);

        using var image = await Image.LoadAsync(inputPath);
        var predictions = yolo.Predict(image);

        if (predictions.Length > 0)
        {
            detect_count++;
        }
        if (detect_count > 5)
        {
            FireToastService fireToastService = new();
            detect_count = 0;
        }

        DrawBoxes(yolo.ModelInputHeight, yolo.ModelInputWidth, image, predictions);

        image.Save(Path.Combine(outputPath, outputName));
    }
    private void DrawBoxes(int modelInputHeight, int modelInputWidth, Image image, Prediction[] predictions)
    {
        foreach (var pred in predictions)
        {
            var originalImageHeight = image.Height;
            var originalImageWidth = image.Width;

            var x = (int)Math.Max(pred.Rectangle.X, 0);
            var y = (int)Math.Max(pred.Rectangle.Y, 0);
            var width = (int)Math.Min(originalImageWidth - x, pred.Rectangle.Width);
            var height = (int)Math.Min(originalImageHeight - y, pred.Rectangle.Height);

            //Note that the output is already scaled to the original image height and width.

            // Bounding Box Text
            string text = $"{pred.Label.Name} [{pred.Score}]";
            var size = TextMeasurer.MeasureSize(text, new TextOptions(font));

            image.Mutate(d => d.Draw(Pens.Solid(Color.Yellow, 3),
                new Rectangle(x, y, width, height)));

            image.Mutate(d => d.DrawText(text, font, Color.Yellow, new Point(x, (int)(y - size.Height - 1))));

        }
    }

    // 返回output文件夹中的文件路径
    public async Task<string> GetFilePathAsync(string fileName)
    {
        var file = await StorageFile.GetFileFromPathAsync(Path.Combine(outputPath, fileName));
        return file.Path;
    }
}
