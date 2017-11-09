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
	public class TableAddCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string APP_COMMAND_OPTION = "app";
		#endregion

		#region 构造函数
		public TableAddCommand() : base("Add")
		{
		}

		public TableAddCommand(string name) : base(name)
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
				throw new CommandException("Obtain the alimap provider failed.");

			//获取指定应用编号对应的地图客户端
			var client = provider.Get(context.Expression.Options.GetValue<string>(APP_COMMAND_OPTION));

			if(client == null)
				return null;

			var result = new AlimapResult[context.Expression.Arguments.Length];

			for(int i = 0; i < context.Expression.Arguments.Length; i++)
			{
				result[i] = Utility.ExecuteTask(() => client.CreateTableAsync(context.Expression.Arguments[i]));
			}

			return result;
		}
		#endregion
	}
}
