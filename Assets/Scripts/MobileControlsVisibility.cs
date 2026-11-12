using UnityEngine;

/// <summary>
/// 挂在装着摇杆和跳跃按钮的 UI 父物体上。
/// 电脑上运行时隐藏手机控件（编辑器里可勾选 alwaysShowInEditor 方便测试）。
/// </summary>
public class MobileControlsVisibility : MonoBehaviour
{
    public bool alwaysShowInEditor = true;

    void Awake()
    {
        bool show = Application.isMobilePlatform || (Application.isEditor && alwaysShowInEditor);
        gameObject.SetActive(show);
    }
}
