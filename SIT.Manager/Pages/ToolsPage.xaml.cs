using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using SIT.Manager.Classes;
using SIT.Manager.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ToolsPage : Page
    {
        public ToolsPage()
        {
            this.InitializeComponent();
        }

        private async void OpenEFTFolderButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (App.ManagerConfig.InstallPath == null)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "配置错误",
                    Content = "\"安装路径\" 未配置。转到 设置 配置客户端安装路径。",
                    CloseButtonText = "好"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }
            else
            {
                if (Directory.Exists(App.ManagerConfig.InstallPath))
                    Process.Start("explorer.exe", App.ManagerConfig.InstallPath);
            }
        }

        private async void OpenBepInExFolderButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (App.ManagerConfig.InstallPath == null)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "配置错误",
                    Content = "\"安装路径\" 未配置。转到 设置 配置客户端安装路径。",
                    CloseButtonText = "好"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }

            string dirPath = App.ManagerConfig.InstallPath + @"\BepInEx\plugins\";
            if (Directory.Exists(dirPath))
                Process.Start("explorer.exe", dirPath);
            else
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "配置错误",
                    Content = $"无法找到指定路径。检查 BepInEx 是否已安装。\n路径: {dirPath}",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }
        }

        private async void OpenSITConfigButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            string path = @"\BepInEx\config\";
            string sitCfg = @"SIT.Core.cfg";

            // Different versions of SIT has different names
            if (!File.Exists(App.ManagerConfig.InstallPath + path + sitCfg))
            {
                sitCfg = "com.sit.core.cfg";
            }

            if (!File.Exists(App.ManagerConfig.InstallPath + path + sitCfg))
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "配置错误",
                    Content = $"无法找到 \"{sitCfg}\"。请确认 SIT 已正确安装并至少启动过一次游戏。",
                    CloseButtonText = "好"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }

            Process.Start("explorer.exe", App.ManagerConfig.InstallPath + path + sitCfg);
        }

        private async void OpenEFTLogButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            string logPath = @"%userprofile%\AppData\LocalLow\Battlestate Games\EscapeFromTarkov\Player.log";
            logPath = Environment.ExpandEnvironmentVariables(logPath);
            if (File.Exists(logPath))
            {
                Process.Start("explorer.exe", logPath);
            }
            else
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "配置错误",
                    Content = "运行日志文件未找到。",
                    CloseButtonText = "好"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }
        }

        private async void InstallSITButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            GithubRelease? selectedVersion;

            SelectSitVersionDialog selectWindow = new()
            {
                XamlRoot = Content.XamlRoot
            };

            ContentDialogResult result = await selectWindow.ShowAsync();

            selectedVersion = selectWindow.version;

            if (selectedVersion == null || result != ContentDialogResult.Primary)
            {
                return;
            }

            await Task.Run(() => Utils.InstallSIT(selectedVersion));
            ManagerConfig.Save();
        }

        private void OpenLocationEditorButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = App.m_window as MainWindow;

            window.contentFrame.Navigate(typeof(LocationEditor));
        }
        private async void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Prompt the user for their choice using a dialog.
                ContentDialog choiceDialog = new ContentDialog
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "清除缓存",
                    Content = "确实要清除 逃离塔科夫 本地缓存或全部缓存？",
                    PrimaryButtonText = "清除 逃离塔科夫 本地缓存",
                    SecondaryButtonText = "清除全部缓存",
                    CloseButtonText = "取消操作"
                };

                ContentDialogResult result = await choiceDialog.ShowAsync();

                // Process the user's choice.
                if (result == ContentDialogResult.Primary)
                {
                    // User chose to clear EFT local cache.
                    string eftCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Battlestate Games", "EscapeFromTarkov");

                    // Check if the directory exists.
                    if (Directory.Exists(eftCachePath))
                    {
                        // Delete all files within the directory.
                        foreach (string file in Directory.GetFiles(eftCachePath))
                        {
                            File.Delete(file);
                        }

                        // Delete all subdirectories and their contents.
                        foreach (string subDirectory in Directory.GetDirectories(eftCachePath))
                        {
                            Directory.Delete(subDirectory, true);
                        }

                        // Optionally, display a success message or perform additional actions.
                        Utils.ShowInfoBar("缓存已清除", "逃离塔科夫 本地缓存已成功清除！", InfoBarSeverity.Informational);
                    }
                    else
                    {
                        // Handle the case where the cache directory does not exist.
                        Utils.ShowInfoBar("清除缓存时出错", $"逃离塔科夫 本地缓存目录：{eftCachePath} 未找到。", InfoBarSeverity.Error);
                    }
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    // User chose to clear everything.
                    try
                    {
                        // Read the 'ManagerConfig.json' file to get the InstallPath.
                        string configFilePath = "ManagerConfig.json"; // Update with the correct path.
                        string jsonContent = File.ReadAllText(configFilePath);

                        // Parse JSON to get the InstallPath.
                        JObject configObject = JObject.Parse(jsonContent);
                        string installPath = configObject["InstallPath"]?.ToString();
                        string serverPath = configObject["AkiServerPath"]?.ToString();

                        if (!string.IsNullOrEmpty(installPath) && !string.IsNullOrEmpty(serverPath))
                        {
                            // Combine the installPath and serverPath with the additional subpath.
                            string cachePath = Path.Combine(installPath, "BepInEx", "cache");
                            string serverCachePath = Path.Combine(serverPath, "user", "cache");

                            // Clear both EFT local cache and additional path.
                            foreach (string file in Directory.GetFiles(cachePath))
                            {
                                File.Delete(file);
                            }

                            foreach (string subDirectory in Directory.GetDirectories(cachePath))
                            {
                                Directory.Delete(subDirectory, true);
                            }

                            foreach (string file in Directory.GetFiles(serverCachePath))
                            {
                                File.Delete(file);
                            }

                            foreach (string subDirectory in Directory.GetDirectories(serverCachePath))
                            {
                                Directory.Delete(subDirectory, true);
                            }

                            // Optionally, display a success message or perform additional actions.
                            Utils.ShowInfoBar("缓存已清除", "所有缓存文件均已清除。请重新启动服务器！", InfoBarSeverity.Informational);
                        }

                        else if (!string.IsNullOrEmpty(installPath) && string.IsNullOrEmpty(serverPath))
                        {
                            // Combine the installPath with the additional subpath.
                            string cachePath = Path.Combine(installPath, "BepInEx", "cache");

                            // Clear both EFT local cache and additional path.
                            foreach (string file in Directory.GetFiles(cachePath))
                            {
                                File.Delete(file);
                            }

                            foreach (string subDirectory in Directory.GetDirectories(cachePath))
                            {
                                Directory.Delete(subDirectory, true);
                            }

                            // Optionally, display a success message or perform additional actions.
                            Utils.ShowInfoBar("缓存已清除", "所有缓存文件均已清除！", InfoBarSeverity.Informational);
                        }

                        else
                        {
                            // Handle the case where InstallPath is not found or empty.
                            Utils.ShowInfoBar("清除缓存时出错", "未能在 'ManagerConfig.json' 中找到 逃离塔科夫 游戏安装目录。", InfoBarSeverity.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that may occur during the process.
                        Utils.ShowInfoBar("错误", $"清除缓存时出错：{ex.Message}", InfoBarSeverity.Error);
                    }
                }
                // No need to handle the Cancel case separately as it will naturally exit the method.
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the process.
                Utils.ShowInfoBar("错误", $"清除缓存时出错：{ex.Message}", InfoBarSeverity.Error);
            }
        }
    }
}