using System;
using System.Collections.ObjectModel;
using HonyWing.Core.Models;

namespace HonyWing.Core.Interfaces
{
    /// <summary>
    /// 日志服务接口
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:35:00
    /// @version: 1.0.0
    /// </summary>
    public interface ILogService : IDisposable
    {
        /// <summary>
        /// 日志条目集合，用于UI绑定
        /// </summary>
        ObservableCollection<LogEntry> LogEntries { get; }

        /// <summary>
        /// 最大日志条目数量
        /// </summary>
        int MaxLogEntries { get; set; }

        /// <summary>
        /// 是否启用自动滚动到最新日志
        /// </summary>
        bool AutoScrollToLatest { get; set; }

        /// <summary>
        /// 当前日志过滤级别
        /// </summary>
        string FilterLevel { get; set; }

        /// <summary>
        /// 日志搜索关键词
        /// </summary>
        string SearchKeyword { get; set; }

        /// <summary>
        /// 初始化日志服务
        /// </summary>
        void Initialize();

        /// <summary>
        /// 清空所有日志
        /// </summary>
        void ClearLogs();

        /// <summary>
        /// 导出日志到文件
        /// </summary>
        /// <param name="filePath">导出文件路径</param>
        /// <param name="format">导出格式 (txt, csv)</param>
        /// <returns>是否导出成功</returns>
        bool ExportLogs(string filePath, string format = "txt");

        /// <summary>
        /// 应用日志过滤
        /// </summary>
        void ApplyFilter();

        /// <summary>
        /// 获取指定级别的日志数量
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <returns>日志数量</returns>
        int GetLogCount(string level);

        /// <summary>
        /// 日志条目添加事件
        /// </summary>
        event EventHandler<LogEntry>? LogEntryAdded;

        /// <summary>
        /// 日志过滤变更事件
        /// </summary>
        event EventHandler? FilterChanged;
    }
}