using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;

    // Host works as both a client and the server
    // Everyone can read but just the Owner can write
    // Synced variable over the network
    private NetworkVariable<MyCustomData> randomNumber = new (new MyCustomData
    {
        _int = 50,
        _bool = true, 
    },
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + ";  " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }

    private void Update()
    {
        // Will be true only for the player object that this player owns,
        // So the script only run for the owner and does not run on the other prefab.
        // If we are not the owner of this object 
        if (!IsOwner)
        {
            return;
        }


        if(Input.GetKeyDown(KeyCode.T))
        {
            // We can only spawn Network objects directly on the server
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectPrefab.GetComponent<NetworkObject>().Spawn(true);
            //TestServerRpc();
            /*randomNumber.Value = new MyCustomData
            {
                _int = 56,
                _bool = false,
                message = "Yeeah"
            };*/
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            spawnedObjectPrefab.GetComponent<NetworkObject>().Despawn(true);
            Destroy(spawnedObjectTransform.gameObject);
        }

            Vector3 moveDir = new(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveSpeed * Time.deltaTime * moveDir;

    }

    [ServerRpc]
    // Does not run on the client at all, Only runs on the server
    private void TestServerRpc()
    {
        Debug.Log("Test Server RPC " + OwnerClientId);
    }
}
