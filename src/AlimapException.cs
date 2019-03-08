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
	[Serializable]
	public class AlimapException : ApplicationException
	{
		#region 成员字段
		private int _code;
		#endregion

		#region 构造函数
		public AlimapException(int code, string message = null) : base(message)
		{
			_code = code;
		}
		#endregion

		#region 重写属性
		public int Code
		{
			get
			{
				return _code;
			}
		}
		#endregion
	}
}