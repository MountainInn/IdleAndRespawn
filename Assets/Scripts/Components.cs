using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.Ecs;


public struct Progress { public float val; }

public struct ProgressViewRef { public Image progImage; }

public class ProgressView : MonoBehaviour {}

public struct EvFinished : IEcsIgnoreInFilter {}
