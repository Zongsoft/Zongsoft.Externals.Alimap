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
	[CommandOption(MAPPING_COMMAND_OPTION, typeof(string), Description = "${Text.CommandOption.Mapping.Description}")]
	[CommandOption(COORDINATE_COMMAND_OPTION, typeof(CoordinateType), CoordinateType.GPS, "${Text.CommandOption.Coordinate.Description}")]
	public class DataSetCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string APP_COMMAND_OPTION = "app";
		private const string TABLE_COMMAND_OPTION = "table";
		private const string MAPPING_COMMAND_OPTION = "mapping";
		private const string COORDINATE_COMMAND_OPTION = "coordinate";
		#endregion

		#region 构造函数
		public DataSetCommand() : base("Set")
		{
		}

		public DataSetCommand(string name) : base(name)
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(CommandContext context)
		{
			if(context.Parameter == null)
				throw new CommandException("Missing parameter of the command.");

			//获取地图客户端应用提供程序
			var provider = AlimapCommand.GetProvider(context.CommandNode);

			if(provider == null)
				throw new CommandException("Obtain the alimap provider failed.");

			//获取指定应用编号对应的地图客户端
			var client = provider.Get(context.Expression.Options.GetValue<string>(APP_COMMAND_OPTION));

			if(client == null)
				return null;

			return Utility.ExecuteTask(() => client.UpdateDataAsync(
				context.Expression.Options.GetValue<string>(TABLE_COMMAND_OPTION),
				context.Parameter as IDictionary<string, object>,
				DataMapping.Resolve(context.Expression.Options.GetValue<string>(MAPPING_COMMAND_OPTION)),
				context.Expression.Options.GetValue<CoordinateType>(COORDINATE_COMMAND_OPTION)));
		}
		#endregion
	}
}
