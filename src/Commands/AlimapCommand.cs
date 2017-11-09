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
	public class AlimapCommand : CommandBase<CommandContext>
	{
		#region 成员字段
		private AlimapClientProvider _provider;
		#endregion

		#region 构造函数
		public AlimapCommand() : base("Alimap")
		{
		}

		public AlimapCommand(string name) : base(name)
		{
		}
		#endregion

		#region 公共属性
		public AlimapClientProvider Provider
		{
			get
			{
				return _provider;
			}
			set
			{
				_provider = value ?? throw new ArgumentNullException();
			}
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(CommandContext context)
		{
			return null;
		}
		#endregion

		#region 静态方法
		public static AlimapClientProvider GetProvider(CommandTreeNode node)
		{
			if(node == null)
				return null;

			if(node.Command is AlimapCommand)
				return ((AlimapCommand)node.Command).Provider;

			if(node.Parent != null)
				return GetProvider(node.Parent);

			return null;
		}
		#endregion
	}
}
