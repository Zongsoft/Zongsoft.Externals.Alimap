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
		private const string XML_APPS_COLLECTION = "apps";
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
		#endregion

		#region 显式实现
		IAppSettings IConfiguration.Apps
		{
			get
			{
				return this.Apps;
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
		#endregion
	}
}
