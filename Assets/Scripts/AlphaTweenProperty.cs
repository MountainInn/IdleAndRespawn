using System;
using UnityEngine;

public class AlphaTweenProperty : AbstractAlphaTweenProperty, IGenericProperty

{
    public string propertyName { get; private set; }
    private Action<Color> _setter;


    public AlphaTweenProperty( string propertyName, float endValue, bool isRelative = false )
        : base( endValue, isRelative )
    {
        this.propertyName = propertyName;
    }


    /// <summary>
    /// validation checks to make sure the target has a valid property with an accessible setter
    /// </summary>
    public override bool validateTarget( object target )
    {
        // cache the setter
        _setter = GoTweenUtils.setterForProperty<Action<Color>>( target, propertyName );
        return _setter != null;
    }


    public override void prepareForUse()
    {
        var getter = GoTweenUtils.getterForProperty<Func<Color>>( _ownerTween.target, propertyName );

        _endValue = _originalEndValue;

        // if this is a from tween we need to swap the start and end values
        if( _ownerTween.isFrom )
        {
            _startValue = _endValue;
            _endValue = getter().a;
        }
        else
        {
            _startValue = getter().a;
        }

        base.prepareForUse();
    }


    public override void tick( float totalElapsedTime )
    {
        var easedTime = _easeFunction(totalElapsedTime, 0, 1, _ownerTween.duration);

        float newAlpha = Mathf.LerpUnclamped(_startValue, _diffValue, easedTime);
        var getter = GoTweenUtils.getterForProperty<Func<Color>>( _ownerTween.target, propertyName );
        var newColor = getter.Invoke().SetA(newAlpha);

        _setter(newColor);
    }

}

public abstract class AbstractAlphaTweenProperty : AbstractTweenProperty
{
    protected Material _target;

    protected float _originalEndValue;
    protected float _startValue;
    protected float _endValue;
    protected float _diffValue;


    public AbstractAlphaTweenProperty( float endValue, bool isRelative ) : base( isRelative )
    {
        _originalEndValue = endValue;
    }


    public override bool validateTarget( object target )
    {
        return ( target is Material || target is GameObject || target is Transform || target is Renderer );
    }


    public override void init( GoTween owner )
    {
        // setup our target before initting
        if( owner.target is Material )
            _target = (Material)owner.target;
        else if( owner.target is GameObject )
            _target = ((GameObject)owner.target).GetComponent<Renderer>().material;
        else if( owner.target is Transform )
            _target = ((Transform)owner.target).GetComponent<Renderer>().material;
        else if( owner.target is Renderer )
            _target = ((Renderer)owner.target).material;

        base.init( owner );
    }


    public override void prepareForUse()
    {
        if( _isRelative && !_ownerTween.isFrom )
            _diffValue = _endValue;
        else
            _diffValue = _endValue - _startValue;
    }

}
