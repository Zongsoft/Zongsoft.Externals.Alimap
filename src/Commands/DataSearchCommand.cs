/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 * 
 * Copyright (C) 2017-2019 Zongsoft Corporation. All rights reserved.
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
	[CommandOption(COMMAND_APP_OPTION, typeof(string), Required = true, Description = "${Text.CommandOption.App.Description}")]
	[CommandOption(COMMAND_TABLE_OPTION, typeof(string), Required = true, Description = "${Text.CommandOption.Table.Description}")]
	[CommandOption(COMMAND_POLYGON_OPTION, typeof(string), Description = "${Text.CommandOption.Polygon.Description}")]
	[CommandOption(COMMAND_CENTER_OPTION, typeof(string), Description = "${Text.CommandOption.Center.Description}")]
	[CommandOption(COMMAND_RADIUS_OPTION, typeof(int), DefaultValue = 3000, Description = "${Text.CommandOption.Radius.Description}")]
	[CommandOption(COMMAND_PAGEINDEX_OPTION, typeof(int), DefaultValue = 1, Description = "${Text.CommandOption.PageIndex.Description}")]
	[CommandOption(COMMAND_PAGESIZE_OPTION, typeof(int), DefaultValue = 20, Description = "${Text.CommandOption.PageSize.Description}")]
	public class DataSearchCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string COMMAND_APP_OPTION = "app";
		private const string COMMAND_TABLE_OPTION = "table";
		private const string COMMAND_POLYGON_OPTION = "polygon";
		private const string COMMAND_CENTER_OPTION = "center";
		private const string COMMAND_RADIUS_OPTION = "radius";
		private const string COMMAND_PAGEINDEX_OPTION = "pageIndex";
		private const string COMMAND_PAGESIZE_OPTION = "pageSize";
		#endregion

		#region 构造函数
		public DataSearchCommand() : base("Search")
		{
		}

		public DataSearchCommand(string name) : base(name)
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(CommandContext context)
		{
			//获取地图客户端应用提供程序
			var provider = AlimapCommand.GetProvider(context.CommandNode);

			if(provider == null)
				throw new CommandException("No found the alimap provider for the command.");

			//获取指定应用编号对应的地图客户端
			var client = provider.Get(context.Expression.Options.GetValue<string>(COMMAND_APP_OPTION));

			if(client == null)
				return null;

			if(context.Expression.Options.Contains(COMMAND_CENTER_OPTION))
			{
				var parts = context.Expression.Options.GetValue<string>(COMMAND_CENTER_OPTION).Split(',', ';', '|');

				if(parts.Length != 2)
					throw new CommandOptionException(COMMAND_CENTER_OPTION, "Invalid format of the center point.");

				if(!decimal.TryParse(parts[0], out var longitude))
					throw new CommandOptionException(COMMAND_CENTER_OPTION, "Invalid longitude value of the center point.");

				if(!decimal.TryParse(parts[1], out var latitude))
					throw new CommandOptionException(COMMAND_CENTER_OPTION, "Invalid latitude value of the center point.");

				return Utility.ExecuteTask(() => client.SearchAsync<IDictionary<string, object>>(
					context.Expression.Options.GetValue<string>(COMMAND_TABLE_OPTION),
					longitude, latitude,
					context.Expression.Options.GetValue<int>(COMMAND_RADIUS_OPTION),
					context.Expression.Arguments.Length > 0 ? context.Expression.Arguments[0] : string.Empty,
					context.Expression.Arguments.Length > 1 ? context.Expression.Arguments[1] : string.Empty,
					context.Expression.Options.GetValue<int>(COMMAND_PAGEINDEX_OPTION),
					context.Expression.Options.GetValue<int>(COMMAND_PAGESIZE_OPTION)));
			}
			else if(context.Expression.Options.Contains(COMMAND_POLYGON_OPTION))
			{
				return Utility.ExecuteTask(() => client.SearchAsync<IDictionary<string, object>>(
					context.Expression.Options.GetValue<string>(COMMAND_TABLE_OPTION),
					context.Expression.Options.GetValue<string>(COMMAND_POLYGON_OPTION),
					context.Expression.Arguments.Length > 0 ? context.Expression.Arguments[0] : string.Empty,
					context.Expression.Arguments.Length > 1 ? context.Expression.Arguments[1] : string.Empty,
					context.Expression.Options.GetValue<int>(COMMAND_PAGEINDEX_OPTION),
					context.Expression.Options.GetValue<int>(COMMAND_PAGESIZE_OPTION)));
			}
			else
			{
				return Utility.ExecuteTask(() => client.SearchAsync<IDictionary<string, object>>(
					context.Expression.Options.GetValue<string>(COMMAND_TABLE_OPTION),
					context.Expression.Arguments.Length > 0 ? context.Expression.Arguments[0] : string.Empty,
					context.Expression.Options.GetValue<int>(COMMAND_PAGEINDEX_OPTION),
					context.Expression.Options.GetValue<int>(COMMAND_PAGESIZE_OPTION)));
			}
		}
		#endregion
	}
}
