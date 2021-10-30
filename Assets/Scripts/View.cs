using UnityEngine;

abstract public class View<T> : MonoBehaviour
{
    protected T viewedObject;

    public void SetViewedObject(T viewedObject)
    {
        this.viewedObject = viewedObject;

        HandleSetViewedObject(viewedObject);
    }

    protected abstract void HandleSetViewedObject(T viewedObject);
}
