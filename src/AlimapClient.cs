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
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Zongsoft.Externals.Alimap
{
	public class AlimapClient
	{
		#region 常量定义
		private const string KEY_ID_MAPPING = "Id";
		private const string KEY_NAME_MAPPING = "Name";
		private const string KEY_LONGITUDE_MAPPING = "Longitude";
		private const string KEY_LATITUDE_MAPPING = "Latitude";
		private const string KEY_ADDRESS_MAPPING = "Address";

		private const string BASE_URL = "http://yuntuapi.amap.com/";
		private const string BASE_MANAGE_URL = BASE_URL + "datamanage/";
		private const string BASE_SEARCH_URL = BASE_URL + "datasearch/";

		private const string GET_URL = BASE_SEARCH_URL + "id";
		private const string SEARCH_AROUND_URL = BASE_SEARCH_URL + "around";
		private const string SEARCH_POLYGON_URL = BASE_SEARCH_URL + "polygon";
		private const string SEARCH_URL = BASE_MANAGE_URL + "data/list";

		private const string CREATE_TABLE_URL = BASE_MANAGE_URL + "table/create";
		private const string CREATE_DATA_URL = BASE_MANAGE_URL + "data/create";
		private const string UPDATE_DATA_URL = BASE_MANAGE_URL + "data/update";
		private const string DELETE_DATA_URL = BASE_MANAGE_URL + "data/delete";
		private const string IMPORT_DATA_URL = BASE_MANAGE_URL + "data/batchcreate";
		private const string IMPORT_PROGRESS_URL = BASE_MANAGE_URL + "batch/importstatus";
		#endregion

		#region 成员字段
		private string _appId;
		#endregion

		#region 私有变量
		private string _secret;
		private readonly HttpClient _http;
		private Zongsoft.Runtime.Serialization.TextSerializationSettings _serializationSettings;
		#endregion

		#region 构造函数
		internal protected AlimapClient(string appId, string secret = null)
		{
			if(string.IsNullOrWhiteSpace(appId))
				throw new ArgumentNullException(nameof(appId));

			_appId = appId;
			_secret = secret;
			_http = new HttpClient();

			_serializationSettings = new Runtime.Serialization.TextSerializationSettings()
			{
				SerializationBehavior = Runtime.Serialization.SerializationBehavior.IgnoreNullValue,
			};
		}
		#endregion

		#region 公共属性
		public string AppId
		{
			get
			{
				return _appId;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_appId = value;
			}
		}
		#endregion

		#region 公共方法
		public async Task<IDictionary<string, object>> GetAsync(string tableId, ulong id)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Get, GET_URL, new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "_id", id.ToString() },
			});

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return (await this.GetSearchResultAsync(response)).FirstOrDefault();
		}

		public async Task<IEnumerable<IDictionary<string, object>>> SearchAsync(string tableId, string filter = null, int pageIndex = 1, int pageSize = 20)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			if(pageSize < 1 || pageSize > 100)
				pageSize = 20;

			//构建查询请求的参数集
			var parameters = new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "limit", pageSize.ToString() },
				{ "page", Math.Max(pageIndex, 1).ToString() },
			};

			//如果指定了过滤条件则添加其到查询参数集中
			if(!string.IsNullOrEmpty(filter))
				parameters.Add("filter", filter.Trim());

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Get, SEARCH_URL, parameters);

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return await this.GetSearchResultAsync(response);
		}

		public async Task<IEnumerable<IDictionary<string, object>>> SearchAsync(string tableId, decimal longitude, decimal latitude, int radius = 3000, string filter = null, string keyword = null, int pageIndex = 1, int pageSize = 20)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			//确保半径为正数
			radius = Math.Abs(radius);

			if(pageSize < 1 || pageSize > 100)
				pageSize = 20;

			//构建查询请求的参数集
			var parameters = new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "center", longitude.ToString("0.000000") + "," + latitude.ToString("0.000000")},
				{ "radius", radius.ToString() },
				{ "limit", pageSize.ToString() },
				{ "page", Math.Max(pageIndex, 1).ToString() },
			};

			//如果指定了过滤条件则添加其到查询参数集中
			if(!string.IsNullOrEmpty(filter))
				parameters.Add("filter", filter.Trim());

			//如果指定了关键字过滤条件则添加其到查询参数集中
			if(!string.IsNullOrEmpty(keyword))
				parameters.Add("keywords", keyword.Trim());

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Get, SEARCH_AROUND_URL, parameters);

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return await this.GetSearchResultAsync(response);
		}

		public async Task<IEnumerable<IDictionary<string, object>>> SearchAsync(string tableId, string polygon, string filter = null, string keyword = null, int pageIndex = 1, int pageSize = 20)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			if(string.IsNullOrEmpty(polygon))
				throw new ArgumentNullException(nameof(polygon));

			if(pageSize < 1 || pageSize > 100)
				pageSize = 20;

			//构建查询请求的参数集
			var parameters = new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "polygon", polygon},
				{ "limit", pageSize.ToString() },
				{ "page", Math.Max(pageIndex, 1).ToString() },
			};

			//如果指定了过滤条件则添加其到查询参数集中
			if(!string.IsNullOrEmpty(filter))
				parameters.Add("filter", filter.Trim());

			//如果指定了关键字过滤条件则添加其到查询参数集中
			if(!string.IsNullOrEmpty(keyword))
				parameters.Add("keywords", keyword.Trim());

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Get, SEARCH_POLYGON_URL, parameters);

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return await this.GetSearchResultAsync(response);
		}

		public async Task<AlimapResult> CreateTableAsync(string name)
		{
			if(string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Post, CREATE_TABLE_URL, new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "name", name },
			});

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return await this.GetResult<CreateTableResult>(response);
		}

		public async Task<AlimapResult> CreateDataAsync(string tableId, IDictionary<string, object> data, string mappingString = null, CoordinateType coordinate = CoordinateType.GPS)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			//处理数据映射关系
			this.Mapping(data, coordinate, mappingString);

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Post, CREATE_DATA_URL, new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "loctype", coordinate == CoordinateType.None ? "2" : "1" },
				{ "data", Runtime.Serialization.Serializer.Json.Serialize(data, _serializationSettings) },
			});

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return await this.GetResult<CreateDataResult>(response);
		}

		public async Task<AlimapResult> UpdateDataAsync(string tableId, IDictionary<string, object> data, string mappingString = null, CoordinateType coordinate = CoordinateType.GPS)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			//处理数据映射关系
			this.Mapping(data, coordinate, mappingString);

			//获取更新操作的查询主键值的过滤条件
			var filter = this.GetUpdateFilter(data, mappingString);

			if(!string.IsNullOrEmpty(filter))
			{
				var dictionary = (await this.SearchAsync(tableId, filter, 1, 1)).FirstOrDefault();

				if(dictionary != null && dictionary.TryGetValue("_id", out var id))
					data["_id"] = id;
			}

			Zongsoft.Diagnostics.Logger.Trace("UpdateDataAsync", data);

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Post, UPDATE_DATA_URL, new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "loctype", coordinate == CoordinateType.None ? "2" : "1" },
				{ "data", Runtime.Serialization.Serializer.Json.Serialize(data, _serializationSettings) },
			});

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return await this.GetResult<ResponseResult>(response);
		}

		public async Task<AlimapResult> DeleteDataAsync(string tableId, string mark)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			if(string.IsNullOrEmpty(mark))
				return new AlimapResult(0, "INVALID-IDS", "Missing arguments.");

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Post, DELETE_DATA_URL, new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "ids", mark },
			});

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return await this.GetResult<DeleteDataResult>(response);
		}
		#endregion

		#region 私有方法
		private void Mapping(IDictionary<string, object> data, CoordinateType coordinate, string mappingString)
		{
			if(data == null || string.IsNullOrWhiteSpace(mappingString))
				return;

			string key;
			object value;

			var removedKeys = new List<string>();
			var changedEntries = new Dictionary<string, Func<object, object>>();

			foreach(var entry in data)
			{
				if(entry.Value == null)
					removedKeys.Add(entry.Key);
				else if(entry.Value is bool)
					changedEntries[entry.Key] = p => (bool)p ? 1 : 0;
				else if(entry.Value is DateTime)
					changedEntries[entry.Key] = p => Utility.GetTimestamp((DateTime)p);
				else if(entry.Value is DateTimeOffset)
					changedEntries[entry.Key] = p => ((DateTimeOffset)p).ToUnixTimeSeconds();
				else if(entry.Value.GetType().IsEnum)
					changedEntries[entry.Key] = p => Convert.ChangeType(p, Enum.GetUnderlyingType(p.GetType()));
			}

			//删除为空的所有数据
			if(removedKeys != null && removedKeys.Count > 0)
			{
				foreach(var removedKey in removedKeys)
					data.Remove(removedKey);
			}

			//转换特定类型的数据
			if(changedEntries != null && changedEntries.Count > 0)
			{
				foreach(var changedEntry in changedEntries)
					data[changedEntry.Key] = changedEntry.Value(data[changedEntry.Key]);
			}

			key = Utility.GetMapping(mappingString, KEY_ID_MAPPING);
			if(key != null && data.TryGetValue(key, out value))
				data["_id"] = value;

			key = Utility.GetMapping(mappingString, KEY_NAME_MAPPING, KEY_NAME_MAPPING);
			if(data.TryGetValue(key, out value))
				data["_name"] = value;

			key = Utility.GetMapping(mappingString, KEY_ADDRESS_MAPPING, KEY_ADDRESS_MAPPING);
			if(data.TryGetValue(key, out value))
				data["_address"] = value;

			var keyX = Utility.GetMapping(mappingString, KEY_LONGITUDE_MAPPING, KEY_LONGITUDE_MAPPING);
			var keyY = Utility.GetMapping(mappingString, KEY_LATITUDE_MAPPING, KEY_LATITUDE_MAPPING);

			if(data.TryGetValue(keyX, out var longitude) && data.TryGetValue(keyY, out var latitude))
				data["_location"] = string.Format("{0:0.000000},{1:0.000000}", longitude, latitude);

			if(coordinate != CoordinateType.None)
				data["coordtype"] = (int)coordinate;
		}

		public string GetUpdateFilter(IDictionary<string, object> data, string mappingString)
		{
			var keys = Utility.GetMapping(mappingString, KEY_ID_MAPPING);

			if(string.IsNullOrEmpty(keys))
				return null;

			var filter = string.Empty;
			var parts = keys.Split(',');

			foreach(var part in parts)
			{
				if(data.TryGetValue(part.Trim(), out var value) && value != null)
				{
					if(filter != null && filter.Length > 0)
						filter += "+";

					filter += part + ":" + value;
				}
			}

			return filter;
		}

		private async Task<AlimapResult> GetResult<T>(HttpResponseMessage response) where T : ResponseResult
		{
			if(response == null || response.Content == null)
				return AlimapResult.Unknown;

			var content = await response.Content.ReadAsStringAsync();

			Zongsoft.Diagnostics.Logger.Trace("GetResult", (object)content);

			if(string.IsNullOrEmpty(content))
				return AlimapResult.Unknown;

			var result = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<T>(content);

			if(result == null)
				return AlimapResult.Unknown;
			else
				return result.ToResult();
		}

		private async Task<IEnumerable<IDictionary<string, object>>> GetSearchResultAsync(HttpResponseMessage response)
		{
			if(response == null || response.Content == null)
				return Enumerable.Empty<IDictionary<string, object>>();

			var text = await response.Content.ReadAsStringAsync();

			if(!string.IsNullOrEmpty(text))
			{
				var result = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<SearchResult>(text);

				if(result != null)
				{
					if(result.status == 0)
						throw new AlimapException(result.status, "[" + result.infocode + "]" + result.info);

					return result.datas ?? Enumerable.Empty<IDictionary<string, object>>();
				}
			}

			return Enumerable.Empty<IDictionary<string, object>>();
		}

		private HttpRequestMessage CreateRequest(HttpMethod method, string url, IDictionary<string, string> parameters)
		{
			if(parameters == null)
				return new HttpRequestMessage(method, url);

			if(string.IsNullOrEmpty(_secret))
				return new HttpRequestMessage(method, url);

			int index = 0;
			var signature = string.Empty;
			var parts = new string[parameters.Count];

			if(!(parameters is SortedDictionary<string, string>))
				parameters = new SortedDictionary<string, string>(parameters);

			foreach(var parameter in parameters)
			{
				parts[index++] = parameter.Key + "=" + parameter.Value;
			}

			using(var md5 = System.Security.Cryptography.MD5.Create())
			{
				var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(string.Join("&", parts) + _secret));
				signature = Zongsoft.Common.Convert.ToHexString(bytes, true);

				parameters.Add("sig", signature);
			}

			if(method == HttpMethod.Get)
			{
				return new HttpRequestMessage(method, url + "?" + EncodeUrl(string.Join("&", parts)) + "&sig=" + signature);
			}
			else
			{
				return new HttpRequestMessage(method, url)
				{
					Content = new FormUrlEncodedContent(parameters)
				};
			}
		}

		private string EncodeUrl(string url)
		{
			if(url != null && url.Length > 0)
				return url.Replace("+", "%2B");

			return url;
		}
		#endregion

		#region 嵌套子类
		private class ResponseResult
		{
			public int status;
			public string info;
			public string infocode;

			public virtual AlimapResult ToResult()
			{
				var code = status == 1 ? 0 : (status == 0 ? 1 : status);
				return new AlimapResult(code, infocode, info);
			}
		}

		private class CreateTableResult : ResponseResult
		{
			public string tableid;

			public override AlimapResult ToResult()
			{
				var result = base.ToResult();
				result.Value = tableid;
				return result;
			}
		}

		private class CreateDataResult : ResponseResult
		{
			public string _id;

			public override AlimapResult ToResult()
			{
				var result = base.ToResult();
				result.Value = _id;
				return result;
			}
		}

		private class DeleteDataResult : ResponseResult
		{
			public int success;
			public int fail;

			public override AlimapResult ToResult()
			{
				var result = base.ToResult();
				result.Value = success.ToString();
				return result;
			}
		}

		private class ImportDataResult : ResponseResult
		{
			public string batchid;

			public override AlimapResult ToResult()
			{
				var result = base.ToResult();
				result.Value = batchid;
				return result;
			}
		}

		private class ImportProgressResult : ResponseResult
		{
			public int progress;
			public int imported;
			public int locaccurate;
			public int locinaccurate;
			public int locfail;

			public override AlimapResult ToResult()
			{
				var result = base.ToResult();
				result.Value = progress.ToString();
				return result;
			}
		}

		private class SearchResult : ResponseResult
		{
			public int count;
			public IDictionary<string, object>[] datas;
		}
		#endregion
	}
}
