using UnityEngine;
using Kogetsu.Library.Core;

public class LoadSceneOnClick : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    public void OnClick() => BasicSceneEffectController.Instance.LoadScene(_sceneName);
}
