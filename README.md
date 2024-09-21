## Azure Kinect Kit


```c#
// step1. SceneEntryScript.cs 添加下面代码
 private void Awake()
{
    Managements.Resource.AddConfig("AzureKinect_Util_resources");
    Managements.UI.AddConfig("AzureKinect_Util_uis");
}

// step2. assemblies.json 添加 "AzureKinect_Util"

```

### 版本更新日志
- v1.0.3
 - ***
- v1.0.2
 - 升级UNIHper, 修复网络库bug