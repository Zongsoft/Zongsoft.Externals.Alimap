﻿/*
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
	[CommandOption(APP_COMMAND_OPTION, typeof(string), Required = true, Description = "${Text.CommandOption.App.Description}")]
	[CommandOption(TABLE_COMMAND_OPTION, typeof(string), Required = true, Description = "${Text.CommandOption.Table.Description}")]
	public class DataRemoveCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string APP_COMMAND_OPTION = "app";
		private const string TABLE_COMMAND_OPTION = "table";
		#endregion

		#region 构造函数
		public DataRemoveCommand() : base("Remove")
		{
		}

		public DataRemoveCommand(string name) : base(name)
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

			//获取指定的应用编号参数
			var appId = context.Expression.Options.GetValue<string>(APP_COMMAND_OPTION);

			//获取指定应用编号对应的地图客户端
			var client = provider.Get(appId) ??
				throw new CommandException($"The alimap-client of the specified '{appId}' appId does not exist or is undefined.");

			string mark;

			if(context.Expression.Arguments.Length == 1)
				mark = context.Expression.Arguments[0];
			else
				mark = string.Join(",", context.Expression.Arguments);

			return Utility.ExecuteTask(() => client.DeleteDataAsync(
					context.Expression.Options.GetValue<string>(TABLE_COMMAND_OPTION), mark));
		}
		#endregion
	}
}
