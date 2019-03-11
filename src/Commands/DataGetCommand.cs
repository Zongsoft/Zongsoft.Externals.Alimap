/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 * 
 * Copyright (C) 2017 Zongsoft Corporation. All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

using Zongsoft.Services;

namespace Zongsoft.Externals.Alimap.Commands
{
	[CommandOption(APP_COMMAND_OPTION, typeof(string), Description = "${Text.CommandOption.App.Description}")]
	[CommandOption(TABLE_COMMAND_OPTION, typeof(string), Description = "${Text.CommandOption.Table.Description}")]
	public class DataGetCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string APP_COMMAND_OPTION = "app";
		private const string TABLE_COMMAND_OPTION = "table";
		#endregion

		#region 构造函数
		public DataGetCommand() : base("Get")
		{
		}

		public DataGetCommand(string name) : base(name)
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(CommandContext context)
		{
			if(context.Expression.Arguments == null || context.Expression.Arguments.Length == 0)
				throw new CommandException("Missing command arguments.");

			//获取地图客户端应用提供程序
			var provider = AlimapCommand.GetProvider(context.CommandNode);

			if(provider == null)
				throw new CommandException("No found the alimap provider for the command.");

			//获取指定应用编号对应的地图客户端
			var client = provider.Get(context.Expression.Options.GetValue<string>(APP_COMMAND_OPTION));

			if(client == null)
				return null;

			if(context.Expression.Arguments.Length == 1)
			{
				ulong id;

				if(!ulong.TryParse(context.Expression.Arguments[0], out id))
					throw new CommandException(string.Format("Invalid '{0}' argument value, it must be a integer.", context.Expression.Arguments[0]));

				return Utility.ExecuteTask(() => client.GetAsync<IDictionary<string, object>>(
					context.Expression.Options.GetValue<string>(TABLE_COMMAND_OPTION), id));
			}

			var result = new IDictionary<string, object>[context.Expression.Arguments.Length];

			for(int i = 0; i < context.Expression.Arguments.Length; i++)
			{
				ulong id;

				if(!ulong.TryParse(context.Expression.Arguments[i], out id))
					throw new CommandException(string.Format("Invalid '{0}' argument value, it must be a integer.", context.Expression.Arguments[i]));

				result[i] = Utility.ExecuteTask(() => client.GetAsync<IDictionary<string, object>>(
					context.Expression.Options.GetValue<string>(TABLE_COMMAND_OPTION), id));
			}

			return result;
		}
		#endregion
	}
}
