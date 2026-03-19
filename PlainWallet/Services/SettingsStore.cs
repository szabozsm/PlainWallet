using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlainWallet.Data;

namespace PlainWallet.Services
{
    public class SettingsStore
    {
        private readonly IServiceProvider services;

        public SettingsStore(IServiceProvider services)
        {
            this.services = services;
        }

    }
}