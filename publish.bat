::设置模块名字
SET ToolName=upm-skyframework
::设置模块版本
SET ToolVersion=skyframework-0.0.2
::设置模块源路径
SET ToolAssetPath=Assets/Plugins/SkyFramework

::此命令会创建一个ToolName的分支，并同步ToolAssetPath下的内容
git subtree split -P %ToolAssetPath% --branch %ToolName%
:: 在ToolName分支设置标签ToolVersion节点
git tag %ToolVersion% %ToolName%

:: 推送到远端
:: git push origin %ToolName% %ToolVersion%
:: git push origin %ToolName%
pause