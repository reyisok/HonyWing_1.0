using System.Drawing;
using System.Threading.Tasks;

namespace HonyWing.Core.Interfaces
{
    /// <summary>
    /// 点击动画服务接口
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-17 10:35:00
    /// @version: 1.0.0
    /// </summary>
    public interface IClickAnimationService
    {
        /// <summary>
        /// 在指定位置显示点击动画
        /// </summary>
        /// <param name="position">点击位置</param>
        Task ShowClickAnimationAsync(Point position);
    }
}