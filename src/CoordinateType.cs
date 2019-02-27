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

namespace Zongsoft.Externals.Alimap
{
	/// <summary>
	/// 表示坐标系的枚举。
	/// </summary>
	public enum CoordinateType
	{
		/// <summary>未定义</summary>
		None = 0,

		/// <summary>地球坐标系(国际标准)</summary>
		WGS84 = 1,

		/// <summary>GPS坐标系</summary>
		GPS = 1,

		/// <summary>火星坐标系</summary>
		GCJ02 = 2,

		/// <summary>百度坐标系</summary>
		BD09 = 3,

		/// <summary>高德地图（火星坐标系）</summary>
		Alimap = 2,

		/// <summary>腾讯地图（火星坐标系）</summary>
		Tencent = 2,

		/// <summary>百度地图（百度坐标系）</summary>
		Baidu = 3,
	}
}
