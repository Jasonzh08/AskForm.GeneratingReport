using WimMain.Common;

namespace WimMain.Fun
{
    /// <summary>
    /// 弘扬
    /// </summary>
    public class HongYang : BaseFun
    {
        public HongYang(DBHelper db) : base(db)
        {
        }

        public Result GetFun()
        {
            return RunFun((logPath, dataPath) =>
            {
                int max = 50;

                ToolFile.CreatFile(dataPath, "取整", true);

                string line = "";

                for (int i = 1; i <= max; i++)
                {
                    line = i + "\t";

                    for (int j = 1; j <= i; j++)
                    {
                        line += "";
                    }

                }


                return Res;
            });
        }



    }
}
