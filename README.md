# FunctionAction

#### 项目介绍
> 适合经常写各种自定义工具的开发者，或者初学者入门学习，
<br/>该项目本身便是一个相当不错的入门代码，注释详细，简单使用了反射和委托等语法

> 两个下拉框联动，当类名下拉框更改后，方法名下拉框会加载出指定类中所有**返回值为Result类型**的方法

<img src="https://gitee.com/StepDest/FileBad/raw/master/MarkDown/FunAction01.png" width="250">
<img src="https://gitee.com/StepDest/FileBad/raw/master/MarkDown/FunAction02.png" width="250">

1. 参数列表中会出现文本框，每个方法需要的参数以及参数类型会展示出来，文本框中的水印，即为参数的注释，
2. 在执行方法前会对每一个参数进行校验，并给出错误提示，
3. 每一个方法在执行结束后，均会生成一个日志文件，默认存于"根目录\Log\\{ClassName}\"文件夹下，可在配置文件中自行修改

#### 使用说明

1. 关于**类名下拉框**，所有被绑定上的类名，均继承自**BaseFun**
```
        public class TestClass : BaseFun
        {

        }
```
2. 关于**方法名下拉框**，所有可被绑定上的方法，返回值类型均为**Result**
```
        public Result Fun1()
        {
            // 推荐写法，自动计算方法运行时间，自动拼装日志路径，自动记录每一次的执行
            // logPath：日志文件路径
            return RunFun((logPath) =>
            {
                
                // 写入日志文件
                base.WriteLog(logPath, "lalal");

                // 方法运行结束后，在弹出的对话框中展示
                Res.Msg += logPath;
                return Res;
            });
        }
```
3. 窗体右键可以打开日志文件夹
4. 目前仅支持基础类型的参数