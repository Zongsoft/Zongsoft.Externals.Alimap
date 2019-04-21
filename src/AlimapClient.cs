/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Zongsoft.Externals.Alimap
{
	public class AlimapClient
	{
		#region 常量定义
		private const int MAX_PAGE_SIZE = 100;

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
		private const string SEARCH_SIMPLEX_URL = BASE_MANAGE_URL + "data/list";

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
		public T Get<T>(string tableId, ulong id)
		{
			return Utility.ExecuteTask(() => this.GetAsync<T>(tableId, id));
		}

		public IEnumerable<T> Search<T>(string tableId, string filter = null, int pageIndex = 1, int pageSize = 20)
		{
			return Utility.ExecuteTask(() => this.SearchAsync<T>(tableId, filter, pageIndex, pageSize));
		}

		public IEnumerable<T> Search<T>(string tableId, decimal longitude, decimal latitude, int radius = 3000, string filter = null, string keyword = null, int pageIndex = 1, int pageSize = 20)
		{
			return Utility.ExecuteTask(() => this.SearchAsync<T>(tableId, longitude, latitude, radius, filter, keyword, pageIndex, pageSize));
		}

		public IEnumerable<T> Search<T>(string tableId, string polygon, string filter = null, string keyword = null, int pageIndex = 1, int pageSize = 20)
		{
			return Utility.ExecuteTask(() => this.SearchAsync<T>(tableId, polygon, filter, keyword, pageIndex, pageSize));
		}

		public async Task<T> GetAsync<T>(string tableId, ulong id)
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
			return (await this.GetSearchResultAsync<T>(response)).FirstOrDefault();
		}

		public async Task<IEnumerable<T>> SearchAsync<T>(string tableId, string filter = null, int pageIndex = 1, int pageSize = 20)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			async Task<HttpResponseMessage> Fetch(int page)
			{
				//构建查询请求的参数集
				var parameters = new SortedDictionary<string, string>
				{
					{ "key", _appId },
					{ "tableid", tableId },
					{ "limit", GetPageSize(pageSize).ToString() },
					{ "page", Math.Max(page, 1).ToString() },
				};

				//如果指定了过滤条件则添加其到查询参数集中
				if(!string.IsNullOrEmpty(filter))
					parameters.Add("filter", filter.Trim());

				//构造请求消息
				var request = this.CreateRequest(HttpMethod.Get, SEARCH_SIMPLEX_URL, parameters);

				//调用远程地图服务
				return await _http.SendAsync(request);
			}

			//将高德地图服务结果转换为结果实体集
			if(pageSize > 0)
				return await this.GetSearchResultAsync<T>(await Fetch(pageIndex));
			else
				return await this.GetSearchResultAsync<T>(await Fetch(pageIndex), GetPageSize(pageSize), page => Utility.ExecuteTask(() => Fetch(page)));
		}

		public async Task<IEnumerable<T>> SearchAsync<T>(string tableId, decimal longitude, decimal latitude, int radius = 3000, string filter = null, string keyword = null, int pageIndex = 1, int pageSize = 20)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			//确保半径为正数
			radius = Math.Abs(radius);

			async Task<HttpResponseMessage> Fetch(int page)
			{
				//构建查询请求的参数集
				var parameters = new SortedDictionary<string, string>
				{
					{ "key", _appId },
					{ "tableid", tableId },
					{ "center", longitude.ToString("0.000000") + "," + latitude.ToString("0.000000")},
					{ "radius", radius.ToString() },
					{ "limit", GetPageSize(pageSize).ToString() },
					{ "page", Math.Max(page, 1).ToString() },
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
				return await _http.SendAsync(request);
			}

			//将高德地图服务结果转换为结果实体集
			if(pageSize > 0)
				return await this.GetSearchResultAsync<T>(await Fetch(pageIndex));
			else
				return await this.GetSearchResultAsync<T>(await Fetch(pageIndex), GetPageSize(pageSize), page => Utility.ExecuteTask(() => Fetch(page)));
		}

		public async Task<IEnumerable<T>> SearchAsync<T>(string tableId, string polygon, string filter = null, string keyword = null, int pageIndex = 1, int pageSize = 20)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));
			if(string.IsNullOrEmpty(polygon))
				throw new ArgumentNullException(nameof(polygon));

			async Task<HttpResponseMessage> Fetch(int page)
			{
				//构建查询请求的参数集
				var parameters = new SortedDictionary<string, string>
				{
					{ "key", _appId },
					{ "tableid", tableId },
					{ "polygon", polygon},
					{ "limit", GetPageSize(pageSize).ToString() },
					{ "page", Math.Max(page, 1).ToString() },
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
				return await _http.SendAsync(request);
			}

			//将高德地图服务结果转换为结果实体集
			if(pageSize > 0)
				return await this.GetSearchResultAsync<T>(await Fetch(pageIndex));
			else
				return await this.GetSearchResultAsync<T>(await Fetch(pageIndex), GetPageSize(pageSize), page => Utility.ExecuteTask(() => Fetch(page)));
		}

		public async Task<string> CreateTableAsync(string name)
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
			return (string)await this.GetResult<CreateTableResult>(response);
		}

		public async Task<string> CreateDataAsync(string tableId, IDictionary<string, object> data, string mappingString = null, CoordinateType coordinate = CoordinateType.GPS)
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
			return (string)await this.GetResult<CreateDataResult>(response);
		}

		public async Task UpdateDataAsync(string tableId, IDictionary<string, object> data, string mappingString = null, CoordinateType coordinate = CoordinateType.GPS)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			//处理数据映射关系
			this.Mapping(data, coordinate, mappingString);

			//获取更新操作的查询主键值的过滤条件
			var filter = this.GetUpdateFilter(data, mappingString);

			if(!string.IsNullOrEmpty(filter))
			{
				var dictionary = (await this.SearchAsync<IDictionary<string, object>>(tableId, filter, 1, 1)).FirstOrDefault();

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

			//将高德地图服务结果转换为结果描述（如果失败则抛出异常）
			await this.GetResult<ResponseResult>(response);
		}

		public async Task<int> DeleteDataAsync(string tableId, string mark)
		{
			if(string.IsNullOrEmpty(tableId))
				throw new ArgumentNullException(nameof(tableId));

			if(string.IsNullOrEmpty(mark))
				throw new ArgumentNullException(nameof(mark));

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
			return (int)await this.GetResult<DeleteDataResult>(response);
		}
		#endregion

		#region 私有方法
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private static int GetPageSize(int pageSize)
		{
			return pageSize > 0 && pageSize <= MAX_PAGE_SIZE ? pageSize : MAX_PAGE_SIZE;
		}

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
					changedEntries[entry.Key] = p => Utility.GetUnixTimestamp((DateTime)p);
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

		private async Task<object> GetResult<T>(HttpResponseMessage response) where T : ResponseResult
		{
			if(response == null || response.Content == null)
				throw new AlimapException($"No response context, the status code is '{response.StatusCode.ToString()}'.");

			var content = await response.Content.ReadAsStringAsync();

			if(string.IsNullOrEmpty(content))
				throw new AlimapException($"No response context, the status code is '{response.StatusCode.ToString()}'.");

			var result = Runtime.Serialization.Serializer.Json.Deserialize<T>(content) ??
			             throw new AlimapException("Invalid response content, the original response content is:" + Environment.NewLine + content);

			if(result.Succeed)
				return result.GetValue();

			throw new AlimapException(result.Status, content);
		}

		private async Task<IEnumerable<T>> GetSearchResultAsync<T>(HttpResponseMessage response, int pageSize = 0, Func<int, HttpResponseMessage> next = null)
		{
			if(response == null || response.Content == null)
				return Enumerable.Empty<T>();

			var text = await response.Content.ReadAsStringAsync();

			if(!string.IsNullOrEmpty(text))
			{
				var result = Runtime.Serialization.Serializer.Json.Deserialize<SearchResult<T>>(text);

				if(result != null)
				{
					if(result.Status == 0)
						throw new AlimapException(result.Status, "[" + result.InfoCode + "]" + result.Info);

					if(pageSize > 0 && next != null && result.Count > pageSize)
						return new SearchResultEnumerable<T>(pageSize, result, next);

					return result.Datas ?? Enumerable.Empty<T>();
				}
			}

			return Enumerable.Empty<T>();
		}
		#endregion

		#region 嵌套子类
		private abstract class ResponseResult
		{
			public int Status;
			public string Info;
			public string InfoCode;

			public bool Succeed
			{
				get => this.Status == 1;
			}

			internal protected virtual object GetValue()
			{
				return this.Status == 1 ? 0 : (this.Status == 0 ? 1 : Status);
			}
		}

		private class CreateTableResult : ResponseResult
		{
			public string TableId;

			internal protected override object GetValue()
			{
				return this.TableId;
			}
		}

		private class CreateDataResult : ResponseResult
		{
			public string _id;

			protected internal override object GetValue()
			{
				return _id;
			}
		}

		private class DeleteDataResult : ResponseResult
		{
			public int Success;
			public int Fail;

			protected internal override object GetValue()
			{
				return this.Success;
			}
		}

		private class ImportDataResult : ResponseResult
		{
			public string BatchId;

			protected internal override object GetValue()
			{
				return this.BatchId;
			}
		}

		private class ImportProgressResult : ResponseResult
		{
			public int Progress;
			public int Imported;
			public int LocAccurate;
			public int LocInaccurate;
			public int LocFail;

			protected internal override object GetValue()
			{
				return this.Progress;
			}
		}

		private class SearchResult<T> : ResponseResult
		{
			public int Count;
			public T[] Datas;

			protected internal override object GetValue()
			{
				return this.Datas;
			}
		}

		private class SearchResultEnumerable<T> : IEnumerable<T>
		{
			#region 成员字段
			private int _pageSize;
			private SearchResult<T> _entity;
			private Func<int, HttpResponseMessage> _next;
			#endregion

			#region 构造函数
			public SearchResultEnumerable(int pageSize, SearchResult<T> result, Func<int, HttpResponseMessage> next)
			{
				_pageSize = pageSize;
				_entity = result;
				_next = next;
			}
			#endregion

			#region 公共方法
			public IEnumerator<T> GetEnumerator()
			{
				return new SearchResultIterator(_pageSize, _entity.Datas, _next);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			#endregion

			#region 嵌套子类
			private class SearchResultIterator : IEnumerator<T>
			{
				private int _index;
				private int _pageIndex;
				private int _pageSize;
				private T[] _items;
				private Func<int, HttpResponseMessage> _next;

				public SearchResultIterator(int pageSize, T[] items, Func<int, HttpResponseMessage> next)
				{
					_index = -1;
					_pageIndex = 1;
					_pageSize = pageSize;
					_items = items;
					_next = next;
				}

				public T Current
				{
					get
					{
						if(_index >= 0 && _index < _items.Length)
							return _items[_index];

						throw new IndexOutOfRangeException();
					}
				}

				object IEnumerator.Current
				{
					get => this.Current;
				}

				public bool MoveNext()
				{
					_index++;

					if(_index >= _items.Length && _pageSize > 0)
					{
						var response = _next(++_pageIndex);

						if(response != null && response.IsSuccessStatusCode && response.Content != null)
						{
							var text = Utility.ExecuteTask(() => response.Content.ReadAsStringAsync());
							var result = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<SearchResult<T>>(text);

							if(result != null && result.Datas != null && result.Datas.Length > 0)
							{
								//如果返回的数据量小于页大小则说明没有后续数据了，即清零页大小以示不用再进行后续查找了
								if(result.Datas.Length < _pageSize)
									_pageSize = 0;

								_index = 0;
								_items = result.Datas;
							}
						}
					}

					return _index < _items.Length;
				}

				public void Reset()
				{
					_index = -1;
					_pageIndex = 1;
				}

				public void Dispose()
				{
					_index = -1;
					_items = null;
					_next = null;
				}
			}
			#endregion
		}
		#endregion
	}
}
