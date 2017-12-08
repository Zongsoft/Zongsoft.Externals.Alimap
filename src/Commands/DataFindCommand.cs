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
	[CommandOption(COMMAND_APP_OPTION, typeof(string), Description = "${Text.CommandOption.App.Description}")]
	[CommandOption(COMMAND_TABLE_OPTION, typeof(string), Description = "${Text.CommandOption.Table.Description}")]
	[CommandOption(COMMAND_PAGEINDEX_OPTION, typeof(int), DefaultValue = 1, Description = "${Text.CommandOption.PageIndex.Description}")]
	[CommandOption(COMMAND_PAGESIZE_OPTION, typeof(int), DefaultValue = 20, Description = "${Text.CommandOption.PageSize.Description}")]
	public class DataFindCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string COMMAND_APP_OPTION = "app";
		private const string COMMAND_TABLE_OPTION = "table";
		private const string COMMAND_PAGEINDEX_OPTION = "pageIndex";
		private const string COMMAND_PAGESIZE_OPTION = "pageSize";
		#endregion

		#region 构造函数
		public DataFindCommand() : base("Find")
		{
		}

		public DataFindCommand(string name) : base(name)
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
			var client = provider.Get(context.Expression.Options.GetValue<string>(COMMAND_APP_OPTION));

			if(client == null)
				return null;

			if(context.Expression.Arguments.Length != 1)
				throw new CommandException("The command count of arguments must be zero.");

			return Utility.ExecuteTask(() => client.FindAsync(
				context.Expression.Options.GetValue<string>(COMMAND_TABLE_OPTION),
				context.Expression.Arguments[0],
				context.Expression.Options.GetValue<int>(COMMAND_PAGEINDEX_OPTION),
				context.Expression.Options.GetValue<int>(COMMAND_PAGESIZE_OPTION)));
		}
		#endregion
	}
}
