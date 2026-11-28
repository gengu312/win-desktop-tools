# Win Desktop Tools（Windows C# 桌面小工具集）

<p>
  <a href="./README.md"><img src="https://img.shields.io/badge/Docs-English-64748B?style=flat-square" alt="English" /></a>
  <a href="./README.zh-CN.md"><img src="https://img.shields.io/badge/%E6%96%87%E6%A1%A3-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-2563EB?style=flat-square" alt="简体中文" /></a>
</p>

> 原始开发时间：2025-03-14 ~ 2025-03-15

两个连续迭代开发的 Windows 桌面辅助小工具（C# / .NET）。

## 包含工具

| 子目录 | 名称 | 技术栈 | 原始时间 | 主要作用 |
| --- | --- | --- | --- | --- |
| [screen-rotator/](./screen-rotator/README.md) | 屏幕旋转工具 | C# WinForms | 2025-03-14 | 调用 Windows `user32.dll` 接口快速切换显示器方向 |
| [win11-tools/](./win11-tools/README.md) | Win11 小工具（升级版） | C# WPF (.NET 8) | 2025-03-15 | 整合显示方向切换与开机自启设置管理 |
