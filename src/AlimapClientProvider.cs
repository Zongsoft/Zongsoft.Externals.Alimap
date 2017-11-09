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
using System.Collections.Concurrent;

namespace Zongsoft.Externals.Alimap
{
	public class AlimapClientProvider
	{
		#region 成员字段
		private Options.IConfiguration _configuration;
		private IDictionary<string, AlimapClient> _clients;
		#endregion

		#region 构造函数
		public AlimapClientProvider()
		{
			_clients = new Dictionary<string, AlimapClient>();
		}
		#endregion

		#region 公共属性
		public Options.IConfiguration Configuration
		{
			get
			{
				return _configuration;
			}
			set
			{
				_configuration = value ?? throw new ArgumentNullException();
			}
		}
		#endregion

		#region 公共方法
		public AlimapClient Get(string appId)
		{
			var configuration = _configuration;

			if(string.IsNullOrEmpty(appId))
			{
				if(configuration == null)
					throw new InvalidOperationException("Missing configuration.");

				appId = configuration.Apps.Default;

				if(string.IsNullOrEmpty(appId))
					return null;
			}

			//从缓存中获取指定应用编号的地图客户端对象
			if(_clients.TryGetValue(appId, out var client))
				return client;

			//从配置中获取对应的应用秘钥
			var secret = (string)configuration.Apps.GetValue(appId);

			//如果秘钥对象为空，则表示指定的应用编号是不存在的
			if(secret == null)
				throw new InvalidOperationException($"Specified '{appId}' app of alimap is not existed.");

			return _clients[appId] = new AlimapClient(appId, secret);
		}
		#endregion
	}
}
