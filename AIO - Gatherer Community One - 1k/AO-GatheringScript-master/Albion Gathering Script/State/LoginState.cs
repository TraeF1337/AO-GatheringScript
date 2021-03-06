﻿using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Meta;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class LoginState : StateScript
    {
        private Configuration config;
        private Context context;

        public LoginState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public override int OnLoop(IScriptEngine se)
        {
			if (!LoginWindow.IsOpen && !CharacterSelectWindow.IsOpen && !LoginErrorWindow.IsOpen)
			{
                parent.EnterState("gather");
				return 0;
            }

			if (LoginErrorWindow.IsOpen)
			{
				context.State = "Closing error " + LoginErrorWindow.Message;
				LoginErrorWindow.ClickOk();
				Time.SleepUntil(() => !LoginErrorWindow.IsOpen, 3000);
			}

			if (LoginWindow.IsOpen)
            {
                context.State = "Attempting to login";
                LoginWindow.SetEmail(config.LoginEmail);
                LoginWindow.SetPassword(config.LoginPassword);
                LoginWindow.Login();
                Time.SleepUntil(() => CharacterSelectWindow.IsOpen, 10000);
            }

            if (CharacterSelectWindow.IsOpen)
            {
                context.State = "Selecting character";

                var record = CharacterSelectWindow.RecordByName(config.LoginCharacterName);
                if (record == null)
                {
                    Logging.Log("Failed to find character with the name " + config.LoginCharacterName);
                    return 5000;
                }

                CharacterSelectWindow.LoginWith(record);
                Time.SleepUntil(() => !CharacterSelectWindow.IsOpen, 10000);
                return 100;
            }

            return 1000;
        }
    }
}