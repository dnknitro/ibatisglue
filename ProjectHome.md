The tool would parse iBatis maps and display full combined SQL query based on the map's ID.

E.g. there are 2 maps with 1 statement in each map.
```
<!--1st-->
...
	<statements>
		<sql id="sql1">
			SomeColumn1, SomeColumn2
		</sql>
	</statements>
...

<!--2nd-->
...
	<statements>
		<select id="SelectSomething">
			SELECT 
				<include refid="Persistence1.sql1" /> 
			FROM someTable
		</select>
	</statements>
...
```
Thus calling the tool with 1 argument:
```
	>iBatisGlue.ConsoleApp.exe SelectSomething
```
would return following sql:
```
/*Persistence1.Persistence2.SelectSomething*/

                        SELECT

/*Persistence1.sql1*/

                        SomeColumn1, SomeColumn2

                        FROM someTable
```
It can be configured in the Visual Studio external tools to be called with `$(CurText)` argument, thus if you select `SelectSomething` in your code and call this external tool it will output the result into the VS Output window.

![http://imgur.com/6XqA2.jpg](http://imgur.com/6XqA2.jpg)