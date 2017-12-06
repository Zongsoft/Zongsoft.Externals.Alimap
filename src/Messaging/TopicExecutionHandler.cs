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
using System.IO;
using System.Collections.Generic;

using Zongsoft.Messaging;
using Zongsoft.Communication;
using Zongsoft.Communication.Composition;

namespace Zongsoft.Externals.Alimap.Messaging
{
	public class TopicExecutionHandler : ExecutionHandlerBase
	{
		#region 常量定义
		private const string URI_POINT_CREATE = "point:create";
		private const string URI_POINT_UPDATE = "point:update";
		private const string URI_POINT_DELETE = "point:delete";
		private const string URI_POINT_DELETEMANY = "point:deletemany";
		#endregion

		#region 成员字段
		private Options.IConfiguration _configuration;
		private AlimapClientProvider _alimapProvider;
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

		public AlimapClientProvider AlimapProvider
		{
			get
			{
				return _alimapProvider;
			}
			set
			{
				_alimapProvider = value ?? throw new ArgumentNullException();
			}
		}
		#endregion

		#region 重写方法
		protected override void OnExecute(IExecutionPipelineContext context)
		{
			var message = context.Parameter as TopicMessage;

			if(message == null)
				return;

			IDictionary<string, object> data = null;
			AlimapResult result;

			using(var stream = new MemoryStream(message.Data))
			{
				var package = PackageSerializer.Default.Deserialize(stream);

				if(package == null || string.IsNullOrEmpty(package.Url))
					return;

				var option = this.GetOption(package.Url);

				if(option == null)
					throw new InvalidOperationException($"Missing mapping for the {package.Url}.");

				var alimap = _alimapProvider.Get(option.AppId);

				if(alimap == null)
					throw new InvalidOperationException($"The '{option.AppId}' appId of the Alimap is not existed.");

				switch(package.Url.ToLowerInvariant())
				{
					case URI_POINT_CREATE:
						data = Runtime.Serialization.Serializer.Json.Deserialize<IDictionary<string, object>>(package.Contents[0].ContentStream);

						result = alimap.CreateDataAsync(option.TableId, data, option.GetMapping(), option.Coordinate).Result;

						if(result.Code != 0)
							throw new InvalidOperationException(result.Message);

						break;
					case URI_POINT_UPDATE:
						data = Runtime.Serialization.Serializer.Json.Deserialize<IDictionary<string, object>>(package.Contents[0].ContentStream);

						result = alimap.UpdateDataAsync(option.TableId, data, option.GetMapping(), option.Coordinate).Result;

						if(result.Code != 0)
							throw new InvalidOperationException(result.Message);

						break;
					case URI_POINT_DELETE:
						var ids = Runtime.Serialization.Serializer.Json.Deserialize<ulong[]>(package.Contents[0].ContentStream);

						result = alimap.DeleteDataAsync(option.TableId, string.Join(",", ids)).Result;

						if(result.Code != 0)
							throw new InvalidOperationException(result.Message);

						break;
				}
			}
		}
		#endregion

		#region 私有方法
		private Options.IHandlerOption GetOption(string url)
		{
			if(string.IsNullOrEmpty(url))
				return null;

			var index = url.IndexOf(':');

			if(index > 0)
			{
				var identity = url.Substring(0, index);

				if(_configuration.Handlers.TryGetValue(identity, out var result))
					return result;
			}

			return null;
		}
		#endregion
	}
}
