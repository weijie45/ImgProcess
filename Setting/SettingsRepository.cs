using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public interface SettingsRepository
    {
        // Methods
        void Add(Settings setting);

        void Remove(string key);


        Settings FindByKey(string key);

        List<Settings> FindAll();
    }
}
