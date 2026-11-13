/// <summary>
/// 掉进 DeadPlane 后可以复位的物体（箱子、以后的敌人等）。
/// </summary>
public interface IResettable
{
    void ResetObject();
}
