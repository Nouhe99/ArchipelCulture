using UnityEngine;

public class Variable<T> : ScriptableObject
{
    private T _value;
    public virtual T Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            RaiseOnValueChanged();
        }
    }

    public delegate void ValueChanged();
    public event ValueChanged OnValueChanged;
    public virtual void RaiseOnValueChanged()
    {
        if (OnValueChanged != null)
        {
            OnValueChanged();
        }
    }
}
