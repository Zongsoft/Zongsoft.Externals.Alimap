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

namespace Zongsoft.Externals.Alimap
{
	internal static class Utility
	{
		private static readonly IDictionary<string, IDictionary<string, string>> _mappingCache = new Dictionary<string, IDictionary<string, string>>();

		public static string GetMapping(string mappingString, string key, string defaultKey = null)
		{
			var mapping = GetMapping(mappingString);

			if(mapping != null && mapping.TryGetValue(key, out var value))
				return string.IsNullOrWhiteSpace(value) ? defaultKey : value.Trim();

			return defaultKey;
		}

		public static IDictionary<string, string> GetMapping(string mappingString)
		{
			if(string.IsNullOrEmpty(mappingString))
				return null;

			IDictionary<string, string> mapping;

			if(_mappingCache.TryGetValue(mappingString, out mapping))
				return mapping;

			lock(_mappingCache)
			{
				if(_mappingCache.TryGetValue(mappingString, out mapping))
					return mapping;

				var parts = mappingString.Split(';', '|');
				mapping = new Dictionary<string, string>(parts.Length, StringComparer.OrdinalIgnoreCase);

				foreach(var part in parts)
				{
					if(string.IsNullOrWhiteSpace(part))
						continue;

					var pair = part.Split(':', '=');
					var key = pair[0].Trim();

					mapping[key] = pair.Length > 1 ? pair[1].Trim() : key;
				}

				//将解析完成的映射字典加入到缓存容器中
				_mappingCache.Add(mappingString, mapping);

				return mapping;
			}
		}

		#region 异步处理
		/// <summary>
		/// 异步包装方法：确保在Web程序中不会被异步操作的并发线程乱入。
		/// </summary>
		/// <typeparam name="T">返回值的类型。</typeparam>
		/// <param name="thunk">异步任务的委托。</param>
		/// <returns>返回以同步方式返回异步任务的执行结果。</returns>
		public static T ExecuteTask<T>(Func<System.Threading.Tasks.Task<T>> thunk)
		{
			return System.Threading.Tasks.Task.Run(() => ExecuteTaskDelegate(() => thunk())).Result;
		}

		private static async System.Threading.Tasks.Task<T> ExecuteTaskDelegate<T>(Func<System.Threading.Tasks.Task<T>> thunk)
		{
			return await thunk();
		}
		#endregion
	}
}
