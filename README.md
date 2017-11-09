# Zongsoft.Externals.Alimap
关于阿里地图(高德云图)服务的扩展开发包。


## 高德云图命令说明

### 创建云图表
`alimap.table.create -app:appKey <args>`

*参数说明：*
> - `app`	表示云图应用管理中的应用Key。
> - `args`	命令参数集，包含要创建的表名集。

-----

### 新增一条数据记录
```bash
alimap.data.add
	-app:appKey
	-table:tableId
	-coordinate:[None|Gps|Alimap|Baidu]
	-mapping:'id=pointId;name=;longitude=x;latitude=y;address=addressDetail'
```

*参数说明：*
> - `app`				表示云图应用管理中的应用Key。
> - `table`				表示要新增数据所属的云图表编号。
> - `coordinate`	表示坐标类型的枚举，可选值为`None`、`Gps`、`Alimap`或`Baidu`。
> - `mapping`			表示需要映射的自定义数据中的字段定义。

*其他说明：*
> 要新增的数据实体由命令执行器参数传入或者通过命令管道中方式传入。

-----

### 查询指定条件的数据
```bash
alimap.data.get
	-app:appKey
	-table:tableId
	-id:xxx
	-keyword:'keywords'
	-filter:'query condition'
	-sorting:'field:asc, field:desc'
	-pageIndex:1
	-pageSize:20
```

*参数说明：*
> - `app`			表示云图应用管理中的应用Key。
> - `tableId`		表示要查询数据所属的云图表编号。
> - `id`			表示要查找的数据编号，如果指定了该参数则自动忽略其他参数。
> - `keyword`		表示要查找的关键字。
> - `filter`		表示要查询的条件语句，`field1:value1+field2:[value2,value3]`。
> - `sorting`		表示对结果进行排序的规则，定义`field:asc, field:desc`。
> - `pageIndex`		指定返回结果集的分页页号。
> - `pageSize`		指定返回结果集的分页大小。

-----

### 更新单条数据
```bash
alimap.data.set
	-app:appKey
	-table:tableId
	-coordinate:[None|Gps|Alimap|Baidu]
	-mapping:'id=pointId;name=;longitude=x;latitude=y;address=addressDetail'
```

*参数说明：*
> - `app`				表示云图应用管理中的应用Key。
> - `tableId`			表示要更新数据所属的云图表编号。
> - `coordinate`	表示坐标类型的枚举，可选值为`None`、`Gps`、`Alimap`或`Baidu`。
> - `mapping`			表示需要映射的自定义数据中的字段定义。

*其他说明：*
> 要修改的数据实体由命令执行器参数传入或者通过命令管道中方式传入。

-----

### 删除指定编号的数据
```
alimap.data.remove
	-app:appKey
	-table:tableId
	<args>
```

*参数说明：*
> - `app`		表示云图应用管理中的应用Key。
> - `tableId`	表示要删除数据所属的云图表编号。
> - `args` 		命令参数集，包含要删除的数据编号。

-----

### 批量导入数据
```bash
alimap.data.import
	-app:app
	-table:tableId
	-file:'file path'
	-fileType:[json|csv]
	-coordinate:[None|Gps|Alimap|Baidu]
	-mapping:'id=pointId;name=;longitude=x;latitude=y;address=addressDetail'
```

*参数说明：*
> - `app`				表示云图应用管理中的应用Key。
> - `tableId`			表示要导入数据所属的云图表编号。
> - `file`				表示要导入的数据文件路径。
> - `fileType`			表示要导入的数据文件类型，可选值为`json`或`csv`。
> - `coordinate`	表示坐标类型的枚举，可选值为`None`、`Gps`、`Alimap`或`Baidu`。
> - `mapping`			表示需要映射的自定义数据中的字段定义。

-----

### 获取数据导入的情况
`alimap.data.import.progress -app:appKey -table:tableId <args>`

*参数说明：*
> - `app`		表示云图应用管理中的应用Key。
> - `tableId`	表示要操作数据所属的云图表编号。
> - `args`		命令参数集，包含要查询的导入批号。

-----
