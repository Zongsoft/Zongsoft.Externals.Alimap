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
		private const string FIND_URL = BASE_MANAGE_URL + "data/list";

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
		#endregion

		#region 构造函数
		internal protected AlimapClient(string appId, string secret = null)
		{
			if(string.IsNullOrWhiteSpace(appId))
				throw new ArgumentNullException(nameof(appId));

			_appId = appId;
			_secret = secret;
			_http = new HttpClient();
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
			return this.GetSearchResult(response).FirstOrDefault();
		}

		public async Task<IEnumerable<IDictionary<string, object>>> FindAsync(string tableId, string filter, int pageIndex = 1, int pageSize = 20)
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
			var request = this.CreateRequest(HttpMethod.Get, FIND_URL, parameters);

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return this.GetSearchResult(response);
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
			return this.GetResult<CreateTableResult>(response);
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
				{ "data", Runtime.Serialization.Serializer.Json.Serialize(data) },
			});

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return this.GetResult<CreateDataResult>(response);
		}

		public async Task<AlimapResult> UpdateDataAsync(string tableId, IDictionary<string, object> data, string mappingString = null, CoordinateType coordinate = CoordinateType.GPS)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			//处理数据映射关系
			this.Mapping(data, coordinate, mappingString);

			//构造请求消息
			var request = this.CreateRequest(HttpMethod.Post, UPDATE_DATA_URL, new SortedDictionary<string, string>
			{
				{ "key", _appId },
				{ "tableid", tableId },
				{ "loctype", coordinate == CoordinateType.None ? "2" : "1" },
				{ "data", Runtime.Serialization.Serializer.Json.Serialize(data) },
			});

			//调用远程地图服务
			var response = await _http.SendAsync(request);

			//将高德地图服务结果转换为结果描述
			return this.GetResult<ResponseResult>(response);
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
			return this.GetResult<DeleteDataResult>(response);
		}
		#endregion

		#region 私有方法
		private void Mapping(IDictionary<string, object> data, CoordinateType coordinate, string mappingString)
		{
			if(data == null || string.IsNullOrWhiteSpace(mappingString))
				return;

			string key;
			object value;

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
				data["_location"] = string.Format("{0},{1}", longitude, latitude);

			if(coordinate != CoordinateType.None)
				data["coordtype"] = (int)coordinate;
		}

		private AlimapResult GetResult<T>(HttpResponseMessage response) where T : ResponseResult
		{
			if(response == null || response.Content == null)
				return AlimapResult.Unknown;

			var content = response.Content.ReadAsStringAsync().Result;

			if(string.IsNullOrEmpty(content))
				return AlimapResult.Unknown;

			var result = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<T>(content);

			if(result == null)
				return AlimapResult.Unknown;
			else
				return result.ToResult();
		}

		private IEnumerable<IDictionary<string, object>> GetSearchResult(HttpResponseMessage response)
		{
			if(response == null || response.Content == null)
				return Enumerable.Empty<IDictionary<string, object>>();

			var text = response.Content.ReadAsStringAsync().Result;

			if(!string.IsNullOrWhiteSpace(text))
			{
				var result = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<SearchResult>(text);

				if(result != null)
					return result.datas;
			}

			return System.Linq.Enumerable.Empty<IDictionary<string, object>>();
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
				return new HttpRequestMessage(method, url + "?" + string.Join("&", parts) + "&sin=" + signature);
			}
			else
			{
				return new HttpRequestMessage(method, url)
				{
					Content = new FormUrlEncodedContent(parameters)
				};
			}
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
