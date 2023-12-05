namespace FireDetectionForWindows.Services;

class ModelVersionStorageService
{
    readonly Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    public void SetVersion(int version) => localSettings.Values["ModelVersion"] = version;
    public int GetVersion()  => localSettings.Values["ModelVersion"] == null ? -1 : (int)localSettings.Values["ModelVersion"];
}