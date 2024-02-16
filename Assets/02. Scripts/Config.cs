using UnityEngine;

public class Config : Singleton<Config>
{
    [SerializeField]
    private string clientId;
    
    [SerializeField]
    private string clientSecret;
    
    [SerializeField]
    private string staticMapBaseUrl;

    public IRepository Repository { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        ConfigRepository();
    }

    private void ConfigRepository()
    {
        Repository = new NetworkRepository(clientId, clientSecret, staticMapBaseUrl);
    }
}