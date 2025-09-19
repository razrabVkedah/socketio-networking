using ClientSocketIO.NetworkData.NetworkVariables;
using UnityEngine;

namespace ClientSocketIO.NetworkComponents
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Transform))]
    public class NetworkTestComponent : BaseNetworkComponent
    {
        [SerializeField] private NetworkVariableFloat floatVariable = new();
        [SerializeField] private NetworkVariableColor colorVariable = new();
        [SerializeField] private Renderer rRenderer;

        private void Start()
        {
            colorVariable.OnGetValueFromServer.AddListener((color) => { rRenderer.material.color = color; });
        }

        /// <summary>
        /// Override Update2() function instead of using Update().
        /// Update2() also calls from Update().
        /// </summary>
        protected override void Update2()
        {
            transform.position = new Vector3(floatVariable.Value, 0, 0);
        }
    }
}