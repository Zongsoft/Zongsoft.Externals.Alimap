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

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Alimap.Options.Configuration
{
	public class GeneralConfiguration : OptionConfigurationElement, IConfiguration
	{
		#region 常量定义
		private const string XML_APP_ELEMENT = "app";
		private const string XML_HANDLER_ELEMENT = "handler";

		private const string XML_APPS_COLLECTION = "apps";
		private const string XML_HANDLERS_COLLECTION = "handlers";
		#endregion

		#region 公共属性
		[OptionConfigurationProperty(XML_APPS_COLLECTION, ElementName = XML_APP_ELEMENT)]
		public AppSettingsCollection Apps
		{
			get
			{
				return (AppSettingsCollection)this[XML_APPS_COLLECTION];
			}
		}

		[OptionConfigurationProperty(XML_HANDLERS_COLLECTION, ElementName = XML_HANDLER_ELEMENT)]
		public HandlerElementCollection Handlers
		{
			get
			{
				return (HandlerElementCollection)this[string.Empty];
			}
		}
		#endregion

		#region 显式实现
		IAppSettings IConfiguration.Apps
		{
			get
			{
				return this.Apps;
			}
		}

		IReadOnlyDictionary<string, IHandlerOption> IConfiguration.Handlers
		{
			get
			{
				return this.Handlers;
			}
		}
		#endregion

		#region 嵌套子类
		public class AppSettingsCollection : SettingElementCollection, IAppSettings
		{
			#region 构造函数
			public AppSettingsCollection() : base(XML_APP_ELEMENT)
			{
			}
			#endregion

			#region 常量定义
			private const string XML_DEFAULT_ATTRIBUTE = "default";
			#endregion

			#region 公共属性
			[OptionConfigurationProperty(XML_DEFAULT_ATTRIBUTE)]
			public string Default
			{
				get
				{
					return (string)this.GetAttributeValue(XML_DEFAULT_ATTRIBUTE);
				}
				set
				{
					this.SetAttributeValue(XML_DEFAULT_ATTRIBUTE, value);
				}
			}
			#endregion
		}

		public class HandlerElement : OptionConfigurationElement, IHandlerOption
		{
			#region 常量定义
			private const string XML_NAME_ATTRIBUTE = "name";
			private const string XML_APPID_ATTRIBUTE = "appId";
			private const string XML_TABLEID_ATTRIBUTE = "tableId";
			private const string XML_MAPPING_ATTRIBUTE = "mapping";
			private const string XML_COORDINATE_ATTRIBUTE = "coordinate";
			#endregion

			#region 公共属性
			[OptionConfigurationProperty(XML_NAME_ATTRIBUTE, OptionConfigurationPropertyBehavior.IsKey)]
			public string Name
			{
				get
				{
					return (string)this[XML_NAME_ATTRIBUTE];
				}
				set
				{
					this[XML_NAME_ATTRIBUTE] = value;
				}
			}

			[OptionConfigurationProperty(XML_APPID_ATTRIBUTE)]
			public string AppId
			{
				get
				{
					return (string)this[XML_APPID_ATTRIBUTE];
				}
				set
				{
					this[XML_APPID_ATTRIBUTE] = value;
				}
			}

			[OptionConfigurationProperty(XML_TABLEID_ATTRIBUTE)]
			public string TableId
			{
				get
				{
					return (string)this[XML_TABLEID_ATTRIBUTE];
				}
				set
				{
					this[XML_TABLEID_ATTRIBUTE] = value;
				}
			}

			[OptionConfigurationProperty(XML_MAPPING_ATTRIBUTE)]
			public string Mapping
			{
				get
				{
					return (string)this[XML_MAPPING_ATTRIBUTE];
				}
				set
				{
					this[XML_MAPPING_ATTRIBUTE] = value;
				}
			}

			[OptionConfigurationProperty(XML_COORDINATE_ATTRIBUTE)]
			public CoordinateType Coordinate
			{
				get
				{
					return (CoordinateType)this[XML_COORDINATE_ATTRIBUTE];
				}
				set
				{
					this[XML_COORDINATE_ATTRIBUTE] = value;
				}
			}
			#endregion

			#region 公共方法
			public DataMapping? GetMapping()
			{
				return Zongsoft.Externals.Alimap.DataMapping.Resolve(this.Mapping);
			}
			#endregion
		}

		public class HandlerElementCollection : OptionConfigurationElementCollection<HandlerElement>, IReadOnlyDictionary<string, IHandlerOption>
		{
			#region 重写方法
			protected override OptionConfigurationElement CreateNewElement()
			{
				return new HandlerElement();
			}

			protected override string GetElementKey(OptionConfigurationElement element)
			{
				return ((HandlerElement)element).Name;
			}
			#endregion

			#region 显式实现
			IHandlerOption IReadOnlyDictionary<string, IHandlerOption>.this[string key] => throw new NotImplementedException();

			IEnumerable<string> IReadOnlyDictionary<string, IHandlerOption>.Keys
			{
				get
				{
					return this.InnerDictionary.Keys;
				}
			}

			IEnumerable<IHandlerOption> IReadOnlyDictionary<string, IHandlerOption>.Values
			{
				get
				{
					return this.InnerDictionary.Values.Cast<IHandlerOption>();
				}
			}

			bool IReadOnlyDictionary<string, IHandlerOption>.TryGetValue(string key, out IHandlerOption value)
			{
				OptionConfigurationElement element;

				if(this.InnerDictionary.TryGetValue(key, out element))
				{
					value = (IHandlerOption)element;
					return true;
				}

				value = null;
				return false;
			}

			IEnumerator<KeyValuePair<string, IHandlerOption>> IEnumerable<KeyValuePair<string, IHandlerOption>>.GetEnumerator()
			{
				foreach(var entry in this.InnerDictionary)
				{
					yield return new KeyValuePair<string, IHandlerOption>(entry.Key, (IHandlerOption)entry.Value);
				}
			}
			#endregion
		}
		#endregion
	}
}
