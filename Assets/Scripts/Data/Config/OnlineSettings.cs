using UnityEngine;
namespace Game.Data.Config
{
    [CreateAssetMenu(menuName="Game/Config/Online Settings",fileName="OnlineSettings")]
    public sealed class OnlineSettings : ScriptableObject
    {
        [SerializeField,Min(5)] private float _connectionTimeoutSeconds=40;
        [SerializeField,Min(5)] private float _commandTimeoutSeconds=20;
        public float ConnectionTimeoutSeconds=>_connectionTimeoutSeconds;
        public float CommandTimeoutSeconds=>_commandTimeoutSeconds;
    }
}
