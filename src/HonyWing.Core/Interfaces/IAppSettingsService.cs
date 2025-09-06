using HonyWing.Core.Models;

namespace HonyWing.Core.Interfaces
{
    /// <summary>
    /// 应用程序设置服务接口
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:45:00
    /// @version: 1.0.0
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// 加载应用程序设置
        /// </summary>
        /// <returns>应用程序设置</returns>
        Task<AppSettings> LoadAsync();
        
        /// <summary>
        /// 保存应用程序设置
        /// </summary>
        /// <param name="settings">要保存的设置</param>
        Task SaveAsync(AppSettings settings);
        
        /// <summary>
        /// 获取默认设置
        /// </summary>
        /// <returns>默认设置</returns>
        AppSettings GetDefaultSettings();
    }
}