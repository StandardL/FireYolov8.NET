using FireDetectionForWindows.ViewModels;
using FireDetectionForWindows.Helpers;
using FireDetectionForWindows.Services;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Media.Capture.Frames;
using Windows.ApplicationModel;
using CommunityToolkit.WinUI.Controls;
using Windows.Graphics.Imaging;
using System.Runtime.CompilerServices;


namespace FireDetectionForWindows.Views;

public sealed partial class MonitorPage : Page
{
    public MonitorViewModel ViewModel
    {
        get;
    }
    //private CameraHelper _cameraHelper;
    private VideoFrame _currentVideoFrame;
    private SoftwareBitmapSource _softwareBitmapSource;
    private static SemaphoreSlim? semaphoreSlim;
    private bool _isModelOn;

    private YoloModelHelper _yoloModelHelper;

    public MonitorPage()
    {
        ViewModel = App.GetService<MonitorViewModel>();
        InitializeComponent();

        Loaded += MonitorPage_Loaded;
        Unloaded += MonitorPage_Unloaded;

        semaphoreSlim = new(1);
        _yoloModelHelper = new();
        _isModelOn = false;
    }

    private async void MonitorPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MonitorPage_Loaded;

        Load();
        await GCCollect();
        //await InitializeAsync();
    }
    private void MonitorPage_Unloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= MonitorPage_Unloaded;
        _softwareBitmapSource?.Dispose();
    }

    private async void Load()
    {
        await semaphoreSlim!.WaitAsync();

        var availableFrameSourceGroups = await CameraHelper.GetFrameSourceGroupsAsync();
        var cameraHelper = new CameraHelper() { FrameSourceGroup = availableFrameSourceGroups.FirstOrDefault() };
        UnsubscribeFromEvents();

        if (mainCam != null)
        {
            mainCam.PreviewFailed += mainCam_PreviewFailed!;
            await mainCam.StartAsync(cameraHelper!);
            mainCam.CameraHelper.FrameArrived += mainCam_FrameArrived!;
            /* 初始化相机选择框 */
            camComboBox.ItemsSource = availableFrameSourceGroups;
            camComboBox.SelectionChanged += CamComboBox_SelectionChanged;
            camComboBox.SelectedIndex = 0;
        }

        _softwareBitmapSource = new();

        semaphoreSlim.Release();
    }

    private void mainCam_PreviewFailed(object sender, PreviewFailedEventArgs e)
    {
        // TODO: Show some error message.
        infoBar.Message = e.Error;
        infoBar.IsOpen = true;
    }

    private void mainCam_FrameArrived(object sender, FrameEventArgs e)
    {
        _currentVideoFrame = e.VideoFrame;
    }
    private void CameraHelper_FrameArrived(object sender, FrameEventArgs e)
    {
        _currentVideoFrame = e.VideoFrame;
    }

    /// <summary>
    /// 相机选择框发生改变时
    /// </summary>
    private void CamComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (camComboBox.SelectedItem is MediaFrameSourceGroup selectedGroup && mainCam.CameraHelper != null)
        {
            mainCam.CameraHelper.FrameSourceGroup = selectedGroup;
        }
    }

    private async void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        var softwareBitmap = _currentVideoFrame?.SoftwareBitmap;
        if (softwareBitmap != null)
        {
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            await _softwareBitmapSource!.SetBitmapAsync(softwareBitmap);
            var image = new Image() { Source = _softwareBitmapSource };
            var dialog = new ContentDialog()
            {
                Title = "当前画面",
                Content = image,
                CloseButtonText = "关闭",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (mainCam.CameraHelper != null)
        {
            mainCam.CameraHelper.FrameArrived -= CameraHelper_FrameArrived!;
        }

        mainCam.PreviewFailed -= mainCam_PreviewFailed!;
    }
    private async Task CleanUpAsync()
    {
        if (camComboBox != null)
        {
            camComboBox.SelectionChanged -= CamComboBox_SelectionChanged;
        }
        /*if (_cameraHelper != null)
        {

            _cameraHelper.FrameArrived -= CameraHelper_FrameArrived!;
            await _cameraHelper.CleanUpAsync();
            _cameraHelper = null!;  // !表示关闭警告
        }*/
        UnsubscribeFromEvents();
        mainCam.Stop();
        await mainCam.CameraHelper.CleanUpAsync();
        _softwareBitmapSource?.Dispose();
    }


    /// <summary>
    /// 回收内存
    /// </summary>
    private async Task GCCollect()
    {
        while (true)
        {
            GC.Collect();
            await Task.Delay(1000);
        }
    }

    private void ModelButton_Checked(object sender, RoutedEventArgs e)
    {
        _isModelOn = true;
        ModelLoop();
    }

    private void ModelButton_Unchecked(object sender, RoutedEventArgs e)
    {
        _isModelOn = false;
        ModelLoop();
    }

    /// <summary>
    /// 循环调用模型推理
    /// 间隔为0.2s
    /// </summary>
    private async void ModelLoop()
    {
        while (_isModelOn)
        {
            await InferenceAsync();
            await Task.Delay(200);
        }
    }

    private async Task InferenceAsync()
    {
        // 获取当前视频帧
        var softwareBitmap = _currentVideoFrame?.SoftwareBitmap;
        if (softwareBitmap != null)
        {

            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {

                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            await _softwareBitmapSource!.SetBitmapAsync(softwareBitmap);

            // 获取当前时间戳
            var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            var inputName = $"input_{timestamp}.jpg";
            var outputName = $"output_{timestamp}.jpg";

            // 保存当前视频帧至缓存
            FrameCacheService _frameCacheService = new(softwareBitmap);
            await _frameCacheService.WriteToFileAsync(inputName);

            // 传入模型进行推理 
            var inputPath = await _frameCacheService.GetFilePathAsync(inputName);
            await _yoloModelHelper.FireYolo(inputPath, outputName);

            // 显示预测结果在界面上
            var outputPath = await _yoloModelHelper.GetFilePathAsync(outputName);
            predCam.Source = new BitmapImage(new Uri(outputPath));
        }
    }

}
