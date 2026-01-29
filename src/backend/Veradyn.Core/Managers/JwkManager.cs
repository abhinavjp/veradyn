using Veradyn.Core.Interfaces.Services;

namespace Veradyn.Core.Managers
{
    public class JwkManager
    {
        private readonly IKeyMaterialService _keyService;

        public JwkManager(IKeyMaterialService keyService)
        {
            _keyService = keyService;
        }

        public string GetJwkSet()
        {
            return _keyService.GetJwkSetJson();
        }
    }
}
