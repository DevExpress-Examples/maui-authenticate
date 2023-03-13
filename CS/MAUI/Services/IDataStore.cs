
namespace MAUI.Services {
    public interface IDataStore {
        Task<string> Authenticate(string userName, string password);
    }
}