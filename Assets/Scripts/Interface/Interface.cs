using UnityEngine;

public interface IWeaponControlable
{
    WeaponStates currentState { get; }

    void Fire();
    void Reload();
    void Hide();
    void Show(Transform attachment);
}
