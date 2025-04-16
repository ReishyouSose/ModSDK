using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class MiscHelper
    {
        public static void NewText(string msg)
        {
            Manager.ui.chatWindow.AddInfoText(new string[1] { msg }, ChatWindow.MessageTextType.Sent);
        }

        public static PugDatabase.DatabaseBankCD GetDatabaseBankCD(this EntityManager entityManager)
        {
            var database = entityManager.CreateEntityQuery(typeof(PugDatabase.DatabaseBankCD))
                .GetSingleton<PugDatabase.DatabaseBankCD>();
            return database;
        }
    }
}
