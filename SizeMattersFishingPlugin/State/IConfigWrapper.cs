namespace SizeMattersFishing.State;

public interface IConfigWrapper
{
    public int Version { get; set; }
    public void Save();
}
