using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfig", menuName = "Config/Server Configuration")]
public class ServerConfig : ScriptableObject
{
    [Header("Server Configuration")]

    [SerializeField] private string vr_assistant_domain;
    [SerializeField] private string vr_ass_init_endpoint;
    [SerializeField] private string vr_ass_chat_endpoint;
    [SerializeField] private string database_url;
    [SerializeField] private string dataEndpoint;
    
    public string VR_Assistant => vr_assistant_domain;
    public string VR_ass_init_endpoint => vr_ass_init_endpoint;
    public string VR_ass_chat_endpoint => vr_ass_chat_endpoint;
    public string Database_url => database_url;
    public string Database_endpoint => dataEndpoint;


    // Helper methods to construct full URLs
    public string GetFullVRAssistantInitUrl() => $"{vr_assistant_domain.TrimEnd('/')}/{vr_ass_init_endpoint.TrimStart('/')}";
    public string GetFullVRAssistantChatUrl() => $"{vr_assistant_domain.TrimEnd('/')}/{vr_ass_chat_endpoint.TrimStart('/')}";
    public string GetFullDataUrl() => $"{database_url.TrimEnd('/')}/{dataEndpoint.TrimStart('/')}";
}