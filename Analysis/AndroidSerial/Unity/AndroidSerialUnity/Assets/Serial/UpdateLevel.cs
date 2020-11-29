
namespace Marin2.Decawave.Unity3d
{
    /// <summary>
    /// The update level is used to determine the frequency of updates in automatic system
    /// Either it is manual (None), Done every receiver update (OnUpdate) or
    /// it can be updated every time the anchor value is set (OnValueUpdate)
    /// </summary>
    public enum UpdateLevel
    {
        None,
        OnValueUpdate,
        OnUpdate
    }
}