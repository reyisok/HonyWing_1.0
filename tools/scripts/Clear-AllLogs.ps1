<#
.SYNOPSIS
    HonyWing 日志清理脚本

.DESCRIPTION
    完全清理 HonyWing 项目的所有历史日志文件，包括：
    - 应用程序日志
    - 图片处理日志
    - 错误日志
    - 性能日志
    - 系统日志
    - 调试日志
    - 编译构建日志
    - 归档日志
    - NLog内部日志

.PARAMETER WhatIf
    预览模式，显示将要删除的文件但不实际删除

.EXAMPLE
    .\Clear-AllLogs.ps1
    直接清理所有日志，自动结束占用进程

.EXAMPLE
    .\Clear-AllLogs.ps1 -WhatIf
    预览将要删除的日志文件

.NOTES
    @author: Mr.Rey Copyright © 2025
    @created: 2025-01-09 16:35:00
    @modified: 2025-01-09 16:35:00
    @version: 1.0.0
#>

param(
    [switch]$WhatIf
)

# 设置错误处理
$ErrorActionPreference = "Stop"

# 获取脚本所在目录的上级目录（项目根目录）
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$LogsDirectory = Join-Path $ProjectRoot "logs"

Write-Host "=== HonyWing 日志清理脚本 ===" -ForegroundColor Cyan
Write-Host "项目根目录: $ProjectRoot" -ForegroundColor Gray
Write-Host "日志目录: $LogsDirectory" -ForegroundColor Gray
Write-Host ""

# 检查日志目录是否存在
if (-not (Test-Path $LogsDirectory)) {
    Write-Host "日志目录不存在: $LogsDirectory" -ForegroundColor Yellow
    Write-Host "没有需要清理的日志文件。" -ForegroundColor Green
    exit 0
}

# 定义要清理的日志类型和路径
$LogCategories = @(
    @{ Name = "应用程序日志"; Path = "app" },
    @{ Name = "图片处理日志"; Path = "image" },
    @{ Name = "错误日志"; Path = "error" },
    @{ Name = "性能日志"; Path = "performance" },
    @{ Name = "系统日志"; Path = "system" },
    @{ Name = "调试日志"; Path = "debug" },
    @{ Name = "编译构建日志"; Path = "build" }
)

# 统计信息
$TotalFiles = 0
$TotalSize = 0
$FilesToDelete = @()

# 扫描所有日志文件
Write-Host "正在扫描日志文件..." -ForegroundColor Yellow

foreach ($category in $LogCategories) {
    $categoryPath = Join-Path $LogsDirectory $category.Path

    if (Test-Path $categoryPath) {
        # 获取当前分类的所有日志文件
        $files = Get-ChildItem -Path $categoryPath -Recurse -File -Include "*.log", "*.txt", "*.binlog" -ErrorAction SilentlyContinue

        if ($files) {
            $categorySize = ($files | Measure-Object -Property Length -Sum).Sum
            $TotalFiles += $files.Count
            $TotalSize += $categorySize

            Write-Host "  [$($category.Name)] 找到 $($files.Count) 个文件，大小: $([math]::Round($categorySize / 1MB, 2)) MB" -ForegroundColor White

            $FilesToDelete += $files
        }
    }
}

# 检查旧的日志文件（根目录下的历史日志）
$oldLogFiles = Get-ChildItem -Path $LogsDirectory -File -Include "*.log", "*.txt", "*.binlog" -ErrorAction SilentlyContinue
if ($oldLogFiles) {
    $oldLogSize = ($oldLogFiles | Measure-Object -Property Length -Sum).Sum
    $TotalFiles += $oldLogFiles.Count
    $TotalSize += $oldLogSize

    Write-Host "  [历史日志文件] 找到 $($oldLogFiles.Count) 个文件，大小: $([math]::Round($oldLogSize / 1MB, 2)) MB" -ForegroundColor White

    $FilesToDelete += $oldLogFiles
}

# 检查所有子目录中的日志文件（递归扫描）
$allLogFiles = Get-ChildItem -Path $LogsDirectory -Recurse -File -Include "*.log", "*.txt", "*.binlog" -ErrorAction SilentlyContinue
if ($allLogFiles) {
    # 过滤掉已经在分类中统计的文件
    $additionalFiles = $allLogFiles | Where-Object { $_.FullName -notin $FilesToDelete.FullName }
    if ($additionalFiles) {
        $additionalSize = ($additionalFiles | Measure-Object -Property Length -Sum).Sum
        $TotalFiles += $additionalFiles.Count
        $TotalSize += $additionalSize

        Write-Host "  [其他日志文件] 找到 $($additionalFiles.Count) 个文件，大小: $([math]::Round($additionalSize / 1MB, 2)) MB" -ForegroundColor White

        $FilesToDelete += $additionalFiles
    }
}

Write-Host ""
Write-Host "扫描完成！" -ForegroundColor Green
Write-Host "总计: $TotalFiles 个日志文件，总大小: $([math]::Round($TotalSize / 1MB, 2)) MB" -ForegroundColor Cyan
Write-Host ""

# 如果没有找到日志文件
if ($TotalFiles -eq 0) {
    Write-Host "没有找到需要清理的日志文件。" -ForegroundColor Green
    exit 0
}

# WhatIf 模式 - 仅显示将要删除的文件
if ($WhatIf) {
    Write-Host "=== 预览模式 - 将要删除的文件 ===" -ForegroundColor Magenta

    foreach ($category in $LogCategories) {
        $categoryPath = Join-Path $LogsDirectory $category.Path

        if (Test-Path $categoryPath) {
            $files = Get-ChildItem -Path $categoryPath -Recurse -File -Include "*.log", "*.txt", "*.binlog" -ErrorAction SilentlyContinue

            if ($files) {
                Write-Host "[$($category.Name)]" -ForegroundColor Yellow
                foreach ($file in $files) {
                    $relativePath = $file.FullName.Replace($LogsDirectory, "")
                    $fileSize = [math]::Round($file.Length / 1KB, 2)
                    Write-Host "  $relativePath ($fileSize KB)" -ForegroundColor Gray
                }
            }
        }
    }

    if ($oldLogFiles) {
        Write-Host "[历史日志文件]" -ForegroundColor Yellow
        foreach ($file in $oldLogFiles) {
            $fileSize = [math]::Round($file.Length / 1KB, 2)
            Write-Host "  $($file.Name) ($fileSize KB)" -ForegroundColor Gray
        }
    }

    Write-Host "使用不带 -WhatIf 参数执行实际删除操作。" -ForegroundColor Cyan
    exit 0
}

# 强制结束占用日志文件的进程
Write-Host "正在检查并结束占用日志文件的进程..." -ForegroundColor Yellow

# 获取所有可能占用日志文件的进程
$ProcessesToKill = @("HonyWing.UI", "HonyWing", "dotnet", "MSBuild", "devenv", "VBCSCompiler", "csc", "vbc", "fsc", "node", "npm")

foreach ($processName in $ProcessesToKill) {
    try {
        $processes = Get-Process -Name $processName -ErrorAction SilentlyContinue
        if ($processes) {
            foreach ($process in $processes) {
                try {
                    Write-Host "  结束进程: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Gray
                    $process.Kill()
                    $process.WaitForExit(5000)  # 等待最多5秒
                }
                catch {
                    Write-Host "    无法结束进程 $($process.ProcessName): $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }
        }
    }
    catch {
        # 忽略进程不存在的错误
    }
}

# 等待文件句柄释放
Start-Sleep -Seconds 2

Write-Host "开始强制清理所有日志文件..." -ForegroundColor Yellow

# 执行删除操作
Write-Host "开始清理日志文件..." -ForegroundColor Yellow

$DeletedFiles = 0
$DeletedSize = 0
$Errors = @()

# 删除分类日志
foreach ($category in $LogCategories) {
    $categoryPath = Join-Path $LogsDirectory $category.Path

    if (Test-Path $categoryPath) {
        try {
            $files = Get-ChildItem -Path $categoryPath -Recurse -File -Include "*.log", "*.txt", "*.binlog" -ErrorAction SilentlyContinue

            if ($files) {
                $categoryDeletedCount = 0
                $categoryDeletedSize = 0

                foreach ($file in $files) {
                    try {
                        $fileSize = $file.Length
                        # 尝试多次删除，处理文件占用情况
                        $retryCount = 0
                        $maxRetries = 3

                        while ($retryCount -lt $maxRetries) {
                            try {
                                Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                                break
                            }
                            catch {
                                $retryCount++
                                if ($retryCount -lt $maxRetries) {
                                    Start-Sleep -Milliseconds 500
                                }
                                else {
                                    throw
                                }
                            }
                        }
                        $categoryDeletedCount++
                        $categoryDeletedSize += $fileSize
                    }
                    catch {
                        $Errors += "删除文件失败: $($file.FullName) - $($_.Exception.Message)"
                    }
                }

                $DeletedFiles += $categoryDeletedCount
                $DeletedSize += $categoryDeletedSize

                Write-Host "  [$($category.Name)] 已删除 $categoryDeletedCount 个文件，释放 $([math]::Round($categoryDeletedSize / 1MB, 2)) MB" -ForegroundColor Green

                # 删除空的归档目录
                $archivePath = Join-Path $categoryPath "archives"
                if ((Test-Path $archivePath) -and ($null -eq (Get-ChildItem $archivePath -Recurse -ErrorAction SilentlyContinue))) {
                    Remove-Item -Path $archivePath -Recurse -Force -ErrorAction SilentlyContinue
                }

                # 如果分类目录为空，也删除它
                if ($null -eq (Get-ChildItem $categoryPath -ErrorAction SilentlyContinue)) {
                    Remove-Item -Path $categoryPath -Force -ErrorAction SilentlyContinue
                }
            }
        }
        catch {
            $Errors += "处理分类 [$($category.Name)] 时出错: $($_.Exception.Message)"
        }
    }
}

# 删除历史日志文件
if ($oldLogFiles) {
    $oldDeletedCount = 0
    $oldDeletedSize = 0

    foreach ($file in $oldLogFiles) {
        try {
            $fileSize = $file.Length
            # 尝试多次删除，处理文件占用情况
            $retryCount = 0
            $maxRetries = 3

            while ($retryCount -lt $maxRetries) {
                try {
                    Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                    break
                }
                catch {
                    $retryCount++
                    if ($retryCount -lt $maxRetries) {
                        Start-Sleep -Milliseconds 500
                    }
                    else {
                        throw
                    }
                }
            }
            $oldDeletedCount++
            $oldDeletedSize += $fileSize
        }
        catch {
            $Errors += "删除历史日志文件失败: $($file.FullName) - $($_.Exception.Message)"
        }
    }

    $DeletedFiles += $oldDeletedCount
    $DeletedSize += $oldDeletedSize

    Write-Host "  [历史日志文件] 已删除 $oldDeletedCount 个文件，释放 $([math]::Round($oldDeletedSize / 1MB, 2)) MB" -ForegroundColor Green
}

# 删除其他所有日志文件（确保没有遗漏）
$allRemainingLogFiles = Get-ChildItem -Path $LogsDirectory -Recurse -File -Include "*.log", "*.txt", "*.binlog" -ErrorAction SilentlyContinue
if ($allRemainingLogFiles) {
    $remainingDeletedCount = 0
    $remainingDeletedSize = 0

    foreach ($file in $allRemainingLogFiles) {
        try {
            $fileSize = $file.Length
            # 尝试多次删除，处理文件占用情况
            $retryCount = 0
            $maxRetries = 5

            while ($retryCount -lt $maxRetries) {
                try {
                    # 尝试移除只读属性
                    if ($file.IsReadOnly) {
                        $file.IsReadOnly = $false
                    }
                    Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                    break
                }
                catch {
                    $retryCount++
                    if ($retryCount -lt $maxRetries) {
                        Start-Sleep -Milliseconds 1000
                    }
                    else {
                        throw
                    }
                }
            }
            $remainingDeletedCount++
            $remainingDeletedSize += $fileSize
        }
        catch {
            $Errors += "删除剩余日志文件失败: $($file.FullName) - $($_.Exception.Message)"
        }
    }

    if ($remainingDeletedCount -gt 0) {
        $DeletedFiles += $remainingDeletedCount
        $DeletedSize += $remainingDeletedSize

        Write-Host "  [剩余日志文件] 已删除 $remainingDeletedCount 个文件，释放 $([math]::Round($remainingDeletedSize / 1MB, 2)) MB" -ForegroundColor Green
    }
}

# 清理空的归档目录
try {
    $archiveDirs = Get-ChildItem -Path $LogsDirectory -Directory -Recurse -Name "archives" -ErrorAction SilentlyContinue
    foreach ($archiveDir in $archiveDirs) {
        $fullArchivePath = Join-Path $LogsDirectory $archiveDir
        if ($null -eq (Get-ChildItem $fullArchivePath -Recurse -ErrorAction SilentlyContinue)) {
            Remove-Item -Path $fullArchivePath -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}
catch {
    # 忽略归档目录清理错误
}

Write-Host ""
Write-Host "=== 清理完成 ===" -ForegroundColor Green
Write-Host "成功删除: $DeletedFiles 个文件" -ForegroundColor Green
Write-Host "释放空间: $([math]::Round($DeletedSize / 1MB, 2)) MB" -ForegroundColor Green

if ($Errors.Count -gt 0) {
    Write-Host "遇到 $($Errors.Count) 个错误:" -ForegroundColor Red
    foreach ($errorMsg in $Errors) {
        Write-Host "  $errorMsg" -ForegroundColor Red
    }
}

Write-Host "日志清理脚本执行完毕。" -ForegroundColor Cyan
